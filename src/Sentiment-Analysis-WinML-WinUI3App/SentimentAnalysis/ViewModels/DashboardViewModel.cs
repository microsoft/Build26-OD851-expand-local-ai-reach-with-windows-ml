using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using SentimentAnalysis.Models;
using SentimentAnalysis.Services;

namespace SentimentAnalysis.ViewModels;

public partial class DashboardViewModel : ObservableObject, IDisposable
{
    private readonly DispatcherQueueTimer _messageTimer;
    private readonly DispatcherQueueTimer _snapshotTimer;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly List<SupportMessage> _intervalMessages = [];
    private readonly object _lock = new();
    private readonly ConcurrentQueue<(SupportMessageWrapper Wrapper, SupportMessage Message)> _analysisQueue = new();
    private readonly CancellationTokenSource _cts = new();
    private DateTime _startTime;

    public DashboardViewModel()
    {
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        // Generate messages rapidly � every 100ms (10 messages/sec)
        _messageTimer = _dispatcherQueue.CreateTimer();
        _messageTimer.Interval = TimeSpan.FromMilliseconds(200);
        _messageTimer.Tick += OnMessageTimerTick;

        // Snapshot every 3 seconds for the chart
        _snapshotTimer = _dispatcherQueue.CreateTimer();
        _snapshotTimer.Interval = TimeSpan.FromSeconds(3);
        _snapshotTimer.Tick += OnSnapshotTimerTick;

        _startTime = DateTime.Now;

        _ = InitializeAndStartAsync();
    }

    public ObservableCollection<SupportMessageWrapper> Messages { get; } = [];
    public ObservableCollection<SentimentSnapshot> Snapshots { get; } = [];

    [ObservableProperty]
    private int _totalMessages;

    [ObservableProperty]
    private int _positiveCount;

    [ObservableProperty]
    private int _neutralCount;

    [ObservableProperty]
    private int _negativeCount;

    [ObservableProperty]
    private double _averageSentiment;

    [ObservableProperty]
    private double _messagesPerSecond;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _toggleButtonText = "\u23F8 Pause";

    [ObservableProperty]
    private string _modelStatus = "Initializing...";

    [ObservableProperty]
    private bool _isModelReady;

    [ObservableProperty]
    private Microsoft.UI.Xaml.Visibility _modelStatusVisibility = Microsoft.UI.Xaml.Visibility.Visible;

    private const int MaxDisplayedMessages = 200;
    private const int MaxSnapshots = 40;

    private async Task InitializeAndStartAsync()
    {
        var analyzer = SentimentAnalyzerService.Instance;
        analyzer.StatusChanged += (_, status) =>
        {
            _dispatcherQueue.TryEnqueue(() => ModelStatus = status);
        };

        try
        {
            await analyzer.InitializeAsync();
        }
        catch (Exception ex)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                ModelStatus = $"Model failed: {ex.Message}";
            });
            return;
        }

        _dispatcherQueue.TryEnqueue(() =>
        {
            IsModelReady = true;
            ModelStatusVisibility = Microsoft.UI.Xaml.Visibility.Collapsed;
            _messageTimer.Start();
            _snapshotTimer.Start();
            IsRunning = true;
        });

        // Start background analysis workers
        var workerCount = Math.Max(1, Environment.ProcessorCount / 2);
        for (int i = 0; i < workerCount; i++)
        {
            _ = Task.Run(() => AnalysisWorkerAsync(_cts.Token));
        }
    }

    private async Task AnalysisWorkerAsync(CancellationToken ct)
    {
        var analyzer = SentimentAnalyzerService.Instance;
        var batch = new List<(SupportMessageWrapper Wrapper, SupportMessage Message, Sentiment Sentiment)>();

        while (!ct.IsCancellationRequested)
        {
            if (_analysisQueue.TryDequeue(out var item))
            {
                try
                {
                    var (sentiment, score) = analyzer.Analyze(item.Message.Message);
                    item.Message.Sentiment = sentiment;
                    item.Message.SentimentScore = score;
                    item.Message.IsAnalyzed = true;
                    batch.Add((item.Wrapper, item.Message, sentiment));
                }
                catch
                {
                    item.Message.Sentiment = Sentiment.Neutral;
                    item.Message.IsAnalyzed = true;
                    batch.Add((item.Wrapper, item.Message, Sentiment.Neutral));
                }

                // Process remaining queued items before dispatching to UI
                while (batch.Count < 20 && _analysisQueue.TryDequeue(out var next))
                {
                    try
                    {
                        var (nextSentiment, nextScore) = analyzer.Analyze(next.Message.Message);
                        next.Message.Sentiment = nextSentiment;
                        next.Message.SentimentScore = nextScore;
                        next.Message.IsAnalyzed = true;
                        batch.Add((next.Wrapper, next.Message, nextSentiment));
                    }
                    catch
                    {
                        next.Message.Sentiment = Sentiment.Neutral;
                        next.Message.IsAnalyzed = true;
                        batch.Add((next.Wrapper, next.Message, Sentiment.Neutral));
                    }
                }

                var completedBatch = batch.ToList();
                batch.Clear();

                _dispatcherQueue.TryEnqueue(() =>
                {
                    foreach (var (wrapper, _, sentiment2) in completedBatch)
                    {
                        wrapper.SetSentiment(sentiment2);
                        UpdateSentimentCounts(sentiment2);
                    }
                });
            }
            else
            {
                await Task.Delay(10, ct).ConfigureAwait(false);
            }
        }
    }

    private void UpdateSentimentCounts(Sentiment sentiment)
    {
        switch (sentiment)
        {
            case Sentiment.Positive:
                PositiveCount++;
                break;
            case Sentiment.Neutral:
                NeutralCount++;
                break;
            case Sentiment.Negative:
                NegativeCount++;
                break;
        }

        AverageSentiment = TotalMessages > 0
            ? Math.Round((double)PositiveCount / TotalMessages * 100, 1)
            : 0;
    }

    [RelayCommand]
    private void ToggleStream()
    {
        if (IsRunning)
        {
            _messageTimer.Stop();
            _snapshotTimer.Stop();
            IsRunning = false;
            ToggleButtonText = "\u25B6 Resume";
        }
        else
        {
            _startTime = DateTime.Now;
            _messageTimer.Start();
            _snapshotTimer.Start();
            IsRunning = true;
            ToggleButtonText = "\u23F8 Pause";
        }
    }

    private void OnMessageTimerTick(DispatcherQueueTimer sender, object args)
    {
        // Generate a small batch each tick for high volume
        var batchSize = Random.Shared.Next(1, 4);
        for (int i = 0; i < batchSize; i++)
        {
            var msg = MessageGeneratorService.GenerateMessage();
            var wrapper = new SupportMessageWrapper(msg);

            Messages.Insert(0, wrapper);
            if (Messages.Count > MaxDisplayedMessages)
            {
                Messages.RemoveAt(Messages.Count - 1);
            }

            lock (_lock)
            {
                _intervalMessages.Add(msg);
            }

            TotalMessages++;

            // Queue for background sentiment analysis
            _analysisQueue.Enqueue((wrapper, msg));
        }

        var elapsed = (DateTime.Now - _startTime).TotalSeconds;
        MessagesPerSecond = elapsed > 0 ? Math.Round(TotalMessages / elapsed, 1) : 0;
    }

    private void OnSnapshotTimerTick(DispatcherQueueTimer sender, object args)
    {
        List<SupportMessage> batch;
        lock (_lock)
        {
            // Only take messages that have been analyzed; leave pending ones for next tick
            var analyzed = _intervalMessages.Where(m => m.IsAnalyzed).ToList();
            if (analyzed.Count == 0) return;

            foreach (var msg in analyzed)
                _intervalMessages.Remove(msg);

            batch = analyzed;
        }

        var snapshot = new SentimentSnapshot
        {
            Timestamp = DateTime.Now,
            TotalMessages = batch.Count,
            PositiveCount = batch.Count(m => m.Sentiment == Sentiment.Positive),
            NeutralCount = batch.Count(m => m.Sentiment == Sentiment.Neutral),
            NegativeCount = batch.Count(m => m.Sentiment == Sentiment.Negative),
            AverageScore = batch.Count > 0 ? Math.Round(batch.Average(m => m.SentimentScore), 2) : 0
        };

        Snapshots.Add(snapshot);
        if (Snapshots.Count > MaxSnapshots)
        {
            Snapshots.RemoveAt(0);
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _messageTimer.Stop();
        _snapshotTimer.Stop();
        _cts.Dispose();
    }
}
