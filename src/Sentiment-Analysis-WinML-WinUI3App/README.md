# Sentiment Analysis — WinUI 3 Native App

A native Windows desktop app that provides a real-time customer support sentiment analysis dashboard. It uses a RoBERTa sentiment model running **locally** via [Windows ML](https://aka.ms/winml) and [ONNX Runtime](https://onnxruntime.ai/), with hardware acceleration on **NPU**, **GPU**, or **CPU** — no cloud costs.

The app includes a pop-out overlay window so support managers can monitor customer sentiment trends at a glance while working in other applications.

---

## Prerequisites

| Requirement | Details |
|:------------|:--------|
| **Visual Studio 2022** | With the **.NET desktop development** and **Windows App SDK** workloads installed |
| **Windows 10** | Version 1809 (build 17763) or later; targets build 19041 |
| **.NET 8** | Project targets `net8.0-windows10.0.19041.0` |
| **Windows ML CLI** | Used to export the sentiment model from Hugging Face (see [Model Setup](#model-setup)) |

## Model Setup

The model files are included in the project under `AIModels\sentiment\generic\` and are copied to the output on build. If you need to re-export or customize the model:

### Install Windows ML CLI

> **TBD** — Windows ML CLI acquisition steps will be added here.

<!--
# Create a Python 3.10 virtual environment
uv venv --python 3.10
.\.venv\Scripts\activate

# Install Modelkit from wheel
uv pip install '.\<your path>\winml_modelkit-0.0.1.dev1-py3-none-any.whl'

# Sanity check — verify NPU device and EP are available
winml sys --list-device --list-ep
winml hub  # to get the models available
-->

### Export the Model

```bash
winml export -m cardiffnlp/twitter-roberta-base-sentiment-latest -o AIModels/sentiment/generic/model.onnx
```

The model folder should contain:

```
AIModels/sentiment/generic/
├── model.onnx
├── model.onnx.data
├── vocab.json
├── merges.txt
└── sentiment_htp_metadata.json
```

## Getting Started

1. **Clone the repository** and open `SentimentAnalysis.sln` in Visual Studio 2022.

2. **Restore NuGet packages** — Visual Studio will restore them automatically on build, or run:

   ```bash
   dotnet restore
   ```

3. **Select a platform** — Choose `x64` or `ARM64` from the Solution Platforms dropdown.

4. **Build and run** — Press `F5` to build and launch the app.

## How It Works

### Architecture

```
SentimentAnalysis/
├── App.xaml(.cs)                          — Application entry point
├── MainWindow.xaml(.cs)                   — Main dashboard window
├── PopoutChartWindow.xaml(.cs)            — Floating overlay chart window
├── Models/
│   ├── SupportMessage.cs                  — Customer message data model
│   └── SentimentSnapshot.cs               — Point-in-time sentiment data
├── Services/
│   ├── SentimentAnalyzerService.cs        — Model loading, EP setup, inference
│   └── MessageGeneratorService.cs         — Simulated message stream
├── ViewModels/
│   └── DashboardViewModel.cs              — MVVM view model, timers, analysis workers
├── Controls/
│   └── SentimentChart.cs                  — Custom chart control
└── Converters/
    ├── SentimentConverters.cs             — Sentiment-to-color/emoji converters
    └── InverseBoolToVisibilityConverter.cs
```

### Execution Provider Acquisition

Windows ML dynamically acquires hardware-specific execution providers (EPs) so your app doesn't need to ship ~80 MB of EP binaries:

1. **Initialize ORT environment** — `OrtEnv.Instance()`
2. **Discover providers** — `ExecutionProviderCatalog.GetDefault().FindAllProviders()` to list available EPs
3. **Install if needed** — `EnsureReadyAsync()` downloads the EP from Windows Update if not present
4. **Register** — `TryRegister()` registers the EP with ONNX Runtime
5. **Configure session** — Use `AppendExecutionProvider()`, `SetEpSelectionPolicy()`, and optional config entries to target NPU, GPU, or CPU

If the preferred EP is unavailable, the app falls back to CPU automatically.

### Inference Pipeline

1. **Tokenize** — Input text is tokenized using a RoBERTa BPE tokenizer (`Microsoft.ML.Tokenizers`), truncated to 512 tokens.
2. **Create tensors** — `DenseTensor<int>` inputs for `input_ids` and `attention_mask`.
3. **Run inference** — `InferenceSession.Run()` executes the model.
4. **Post-process** — Softmax over logits → argmax to get the sentiment label and a composite score (`positive − negative`).

### Pop-out Overlay

Click the **pop-out** button on the sentiment chart to open a floating overlay window that stays visible on top of other apps — perfect for monitoring sentiment trends at a glance.

## NuGet Dependencies

| Package | Version |
|:--------|:--------|
| `Microsoft.WindowsAppSDK` | `2.0.1` |
| `Microsoft.WindowsAppSDK.ML` | `2.0.300` |
| `Microsoft.ML.Tokenizers` | `2.0.0` |
| `CommunityToolkit.Mvvm` | `8.4.0` |
| `Microsoft.Windows.SDK.BuildTools` | `10.0.28000.1721` |

## Supported Platforms

- x86
- x64
- ARM64

## Learn More

- [Windows ML documentation](https://aka.ms/winml)
- [Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/)
- [ONNX Runtime](https://onnxruntime.ai/docs/)
- [AI Dev Gallery](https://learn.microsoft.com/windows/ai/ai-dev-gallery/)
- [Microsoft Foundry on Windows](https://learn.microsoft.com/windows/ai/overview)
