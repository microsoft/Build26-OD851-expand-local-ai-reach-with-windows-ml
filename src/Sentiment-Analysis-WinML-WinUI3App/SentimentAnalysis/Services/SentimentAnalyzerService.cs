using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;
using Microsoft.Windows.AI.MachineLearning;
using SentimentAnalysis.Models;

namespace SentimentAnalysis.Services;

public sealed class SentimentAnalyzerService
{
    private static SentimentAnalyzerService? _instance;
    private static readonly object _instanceLock = new();

    private bool _isInitialized;
    private InferenceSession? _inferenceSession;
    private BpeTokenizer? _tokenizer;
    private const int MaxSequenceLength = 512;
    private const int PadTokenId = 1; // RoBERTa pad token

    public event EventHandler<string>? StatusChanged;

    private SentimentAnalyzerService() { }

    public static SentimentAnalyzerService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_instanceLock)
                {
                    _instance ??= new SentimentAnalyzerService();
                }
            }
            return _instance;
        }
    }

    public bool IsReady => _isInitialized;

    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        var basePath = Path.Combine(AppContext.BaseDirectory, "AIModels", "sentiment", "generic");

        OnStatusChanged("Loading tokenizer...");
        var vocabPath = Path.Combine(basePath, "vocab.json");
        var mergesPath = Path.Combine(basePath, "merges.txt");
        _tokenizer = await Task.Run(() => BpeTokenizer.Create(vocabPath, mergesPath));

        // Initialize ORT environment first (must exist before EP registration) - 1
        var env = OrtEnv.Instance();
        Debug.WriteLine("[EP] OrtEnv initialized");

        // Discover and register execution providers via WinML - 2
        await DiscoverAndRegisterEpsAsync();

        // Select best available device and configure session options
        var sessionOptions = CreateConfiguredSessionOptions(env);
        //1
        OnStatusChanged("Loading ONNX model...");
        var modelPath = Path.Combine(basePath, "sentiment.onnx");
        _inferenceSession = await Task.Run(() =>
        {
            try
            {
                return new InferenceSession(modelPath, sessionOptions);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[EP] InferenceSession failed with configured EP: {ex.Message}");
                Debug.WriteLine("[EP] Falling back to CPU...");
                OnStatusChanged("NPU load failed, falling back to CPU...");
                return new InferenceSession(modelPath, new SessionOptions());
            }
        });

        _isInitialized = true;
        OnStatusChanged("Model ready");
    }

    public (Sentiment Sentiment, double Score) Analyze(string text)
    {
        if (!_isInitialized || _inferenceSession == null || _tokenizer == null)
            return (Sentiment.Neutral, 0);

        if (string.IsNullOrWhiteSpace(text))
            return (Sentiment.Neutral, 0);

        return RunInference(text);
    }

    public async Task<(Sentiment Sentiment, double Score)> AnalyzeAsync(string text)
    {
        if (!_isInitialized || _inferenceSession == null || _tokenizer == null)
            return (Sentiment.Neutral, 0);

        if (string.IsNullOrWhiteSpace(text))
            return (Sentiment.Neutral, 0);

        return await Task.Run(() => RunInference(text));
    }

    private (Sentiment Sentiment, double Score) RunInference(string text)
    {
        var encoded = _tokenizer!.EncodeToIds(text, MaxSequenceLength, out _, out _);

        var inputIds = new DenseTensor<int>([1, MaxSequenceLength]);
        var attentionMask = new DenseTensor<int>([1, MaxSequenceLength]);

        for (int i = 0; i < MaxSequenceLength; i++)
        {
            if (i < encoded.Count)
            {
                inputIds[0, i] = encoded[i];
                attentionMask[0, i] = 1;
            }
            else
            {
                inputIds[0, i] = PadTokenId;
                attentionMask[0, i] = 0;
            }
        }

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask)
        };

        using var results = _inferenceSession!.Run(inputs);
        var logits = results.First().AsEnumerable<float>().ToArray();
        var probabilities = Softmax(logits);
        int predictedClass = Array.IndexOf(probabilities, probabilities.Max());

        // Score: positive probability minus negative probability, range [-1, 1]
        double score = probabilities.Length >= 3 ? probabilities[2] - probabilities[0] : 0;

        // cardiffnlp/twitter-roberta-base-sentiment-latest:
        // 0 = Negative, 1 = Neutral, 2 = Positive
        var sentiment = predictedClass switch
        {
            0 => Sentiment.Negative,
            1 => Sentiment.Neutral,
            2 => Sentiment.Positive,
            _ => Sentiment.Neutral
        };

        return (sentiment, score);
    }

    private static float[] Softmax(float[] values)
    {
        var maxVal = values.Max();
        var exp = values.Select(v => Math.Exp(v - maxVal)).ToArray();
        var sum = exp.Sum();
        return exp.Select(e => (float)(e / sum)).ToArray();
    }

    private void OnStatusChanged(string status)
    {
        StatusChanged?.Invoke(this, status);
    }
    
    private async Task DiscoverAndRegisterEpsAsync()
    {
        
        try
        {
            Debug.WriteLine("[EP] Discovering execution providers...");
            OnStatusChanged("Discovering execution providers...");
            var catalog = ExecutionProviderCatalog.GetDefault();
            var providers = catalog.FindAllProviders();

            Debug.WriteLine($"[EP] Found {providers.Length} EP provider(s)");
            OnStatusChanged($"Found {providers.Length} EP provider(s)");

            foreach (var provider in providers)
            {
             
                var name = provider.Name;
                Debug.WriteLine($"[EP] Provider: {name}");
                if (provider.Name == "QNNExecutionProvider")
                {
                    Debug.WriteLine($"[EP] EnsureReadyAsync for {name}...");
                    var ensureResult = await provider.EnsureReadyAsync();

                    Debug.WriteLine($"[EP] EnsureReady result: {ensureResult.Status}");
                    if (ensureResult.Status != ExecutionProviderReadyResultState.Success)
                    {
                        Debug.WriteLine($"[EP] EP {name} not ready: {ensureResult.Status}");
                        OnStatusChanged($"EP {name} not ready: {ensureResult.Status}");
                        continue;
                    }

                    if (!provider.TryRegister())
                    {
                        Debug.WriteLine($"[EP] EP {name} registration failed");
                        OnStatusChanged($"EP {name} registration failed");
                        continue;
                    }

                    Debug.WriteLine($"[EP] Registered EP: {name}");
                    OnStatusChanged($"Registered EP: {name}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EP] EP discovery exception: {ex}");
            OnStatusChanged($"EP discovery unavailable: {ex.Message}");
        }
    }

    private SessionOptions CreateConfiguredSessionOptions(OrtEnv env)
    {
        var sessionOptions = new SessionOptions();
        var epDevices = env.GetEpDevices();

        Debug.WriteLine($"[EP] Found {epDevices.Count} ORT EP device(s)");
        OnStatusChanged($"Found {epDevices.Count} ORT EP device(s)");

        OrtEpDevice? npuDevice = null;
        foreach (var device in epDevices)
        {
            var epName = device.EpName;
            var hwDevice = device.HardwareDevice;
            Debug.WriteLine($"[EP]   EP: {epName}, Device: {hwDevice.Type}");
            OnStatusChanged($"  EP: {epName}, Device: {hwDevice.Type}");

            if (hwDevice.Type == OrtHardwareDeviceType.NPU && epName == "QNNExecutionProvider")
            {
                npuDevice = device;
            }
        }

        if (npuDevice != null)
        {
            Debug.WriteLine($"[EP] Selecting NPU via {npuDevice.EpName}...");
            OnStatusChanged($"Selecting NPU via {npuDevice.EpName}...");
            var cacheDir = Path.Combine(AppContext.BaseDirectory, "model_cache");
            Directory.CreateDirectory(cacheDir);
            var epOptions = new Dictionary<string, string>
            {
                ["CacheDir"] = cacheDir
            };
            sessionOptions.AppendExecutionProvider(env, [npuDevice], epOptions);
            sessionOptions.SetEpSelectionPolicy(ExecutionProviderDevicePolicy.PREFER_NPU);
            sessionOptions.AddSessionConfigEntry("session.disable_cpu_ep_fallback", "1");
            Debug.WriteLine($"[EP] Configured EP: {npuDevice.EpName} (NPU), CacheDir: {cacheDir}, CPU fallback disabled");
            OnStatusChanged($"Configured EP: {npuDevice.EpName} (NPU)");
        }
        else
        {
            Debug.WriteLine("[EP] No NPU available, using CPU");
            OnStatusChanged("No NPU available, using CPU");
        }

        return sessionOptions;
    }
}
