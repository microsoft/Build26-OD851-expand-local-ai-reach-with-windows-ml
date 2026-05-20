using Microsoft.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using SentimentAnalysis.Models;

namespace SentimentAnalysis;

public partial class SupportMessageWrapper : ObservableObject
{
    private static readonly SolidColorBrush PositiveBrush = new(ColorHelper.FromArgb(255, 16, 185, 129));
    private static readonly SolidColorBrush NeutralBrush = new(ColorHelper.FromArgb(255, 99, 102, 241));
    private static readonly SolidColorBrush NegativeBrush = new(ColorHelper.FromArgb(255, 239, 68, 68));
    private static readonly SolidColorBrush PendingBrush = new(ColorHelper.FromArgb(255, 120, 120, 120));

    public SupportMessageWrapper(SupportMessage msg)
    {
        CustomerName = msg.CustomerName;
        Message = msg.Message;
        TimeLabel = msg.Timestamp.ToString("HH:mm:ss");

        // Initial state before analysis
        _emoji = "?";
        _sentimentLabel = "Analyzing...";
        _sentimentBrush = PendingBrush;
    }

    public string CustomerName { get; }
    public string Message { get; }
    public string TimeLabel { get; }

    [ObservableProperty]
    private string _emoji;

    [ObservableProperty]
    private string _sentimentLabel;

    [ObservableProperty]
    private SolidColorBrush _sentimentBrush;

    public Sentiment Sentiment { get; private set; }

    public void SetSentiment(Sentiment sentiment)
    {
        Sentiment = sentiment;

        Emoji = sentiment switch
        {
            Sentiment.Positive => "??",
            Sentiment.Neutral => "??",
            Sentiment.Negative => "??",
            _ => "??"
        };

        SentimentLabel = sentiment switch
        {
            Sentiment.Positive => "Positive",
            Sentiment.Neutral => "Neutral",
            Sentiment.Negative => "Negative",
            _ => "Unknown"
        };

        SentimentBrush = sentiment switch
        {
            Sentiment.Positive => PositiveBrush,
            Sentiment.Neutral => NeutralBrush,
            Sentiment.Negative => NegativeBrush,
            _ => NeutralBrush
        };
    }
}
