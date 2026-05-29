# Sentiment Analysis — WebNN Web App

A web-based customer support sentiment analysis dashboard that classifies incoming messages as **positive**, **neutral**, or **negative** using a RoBERTa sentiment model running **locally in the browser** via [ONNX Runtime Web](https://onnxruntime.ai/docs/tutorials/web/) and [WebNN](https://aka.ms/webnn) — powered by [Windows ML](https://aka.ms/winml) on Windows.

No cloud inference costs — the model runs entirely on the user's device and can be accelerated on **CPU**, **GPU**, or **NPU**.

---

## Prerequisites

| Requirement | Details |
|:------------|:--------|
| **Node.js** | Required to install npm packages |
| **Python 3** | Required to run the local development server |
| **Browser** | Microsoft Edge or Google Chrome with WebNN enabled (see [Enable WebNN](#enable-webnn) below) |
| **Windows ML CLI** | Used to export the sentiment model from Hugging Face (see [Model Setup](#model-setup)) |

## Model Setup

Before running the app you need to export the sentiment analysis model into the `model/` folder.

### Install Windows ML CLI

>  Go to aka.ms/winmlcli for instructions to install WinML CLI 

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
winml export -m cardiffnlp/twitter-roberta-base-sentiment-latest -o model/model.onnx
```

After export, your `model/` folder should contain:

```
model/
├── model.onnx
├── model.onnx.data
├── vocab.json
└── merges.txt
```

## Getting Started

1. **Install npm dependencies**

   ```bash
   npm install
   ```

2. **Start the local server**

   The app requires special HTTP headers (`Cross-Origin-Opener-Policy` and `Cross-Origin-Embedder-Policy`) for `SharedArrayBuffer` support, so it must be served through the included Python server:

   ```bash
   python server.py
   ```

3. **Open the app** — navigate to `http://localhost:8080` in Edge or Chrome.

## Enable WebNN

WebNN is currently in developer preview. To enable it:

1. Open `edge://flags` (Edge) or `chrome://flags` (Chrome)
2. Search for **WebNN**
3. Set the **WebNN API** flag to **Enabled**
4. Restart the browser

## How It Works

### Architecture

```
Browser
├── index.html              — App shell and import map for ONNX Runtime Web
├── js/
│   ├── app.js              — Main app logic, message queue, and UI updates
│   ├── sentimentAnalyzer.js — Model loading, WebNN session, and inference
│   ├── tokenizer.js        — RoBERTa BPE tokenizer (vocab.json + merges.txt)
│   ├── sentimentChart.js   — Canvas-based sentiment trend chart
│   └── messageGenerator.js — Simulated customer support message stream
├── css/
│   └── styles.css          — Dashboard styling
└── server.py               — Python HTTP server with COOP/COEP headers
```

### Inference Pipeline

1. **Tokenize** — Input text is tokenized using a RoBERTa BPE tokenizer, padded/truncated to 512 tokens.
2. **Create WebNN session** — An ONNX Runtime inference session is created with the WebNN execution provider, targeting the specified device (CPU, GPU, or NPU).
3. **Run inference** — The tokenized input (`input_ids` + `attention_mask`) is fed to the model via `session.run()`.
4. **Post-process** — A softmax is applied to the model's logits, and the highest-scoring label (positive, neutral, negative) is returned along with a sentiment score.

### Changing the Hardware Target

In `js/sentimentAnalyzer.js`, the WebNN device type is configured when creating the ML context:

```js
const mlContext = await navigator.ml.createContext({ deviceType: 'npu' });
```

Change `'npu'` to `'gpu'` or `'cpu'` to target different hardware.

## Dependencies

| Package | Version |
|:--------|:--------|
| `onnxruntime-web` | `^1.24.3` |

## Learn More

- [Windows ML documentation](https://aka.ms/winml)
- [ONNX Runtime Web](https://onnxruntime.ai/docs/tutorials/web/)
- [WebNN API specification](https://www.w3.org/TR/webnn/)
- [AI Dev Gallery](https://learn.microsoft.com/windows/ai/ai-dev-gallery/)
