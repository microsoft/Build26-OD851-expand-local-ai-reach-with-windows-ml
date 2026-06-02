# /src

This folder contains the source code and demo apps for **OD851: Inferencing Local AI Models on all Windows PCs with Windows ML**.

## Code Samples

### 1. [Sentiment Analysis — WebNN Web App](./Sentiment-Analysis-WinML-WebNN-WebApp)

A web-based customer support sentiment analysis dashboard that classifies incoming messages as **positive**, **neutral**, or **negative** using a RoBERTa sentiment model running **locally in the browser** via [ONNX Runtime Web](https://onnxruntime.ai/docs/tutorials/web/) and [WebNN](https://aka.ms/webnn) — powered by [Windows ML](https://aka.ms/winml) on Windows.

- **Technology:** JavaScript, ONNX Runtime Web, WebNN
- **How to run:** See the [README](./Sentiment-Analysis-WinML-WebNN-WebApp/README.md) for setup and instructions.

### 2. [Sentiment Analysis — WinUI 3 Native App](./Sentiment-Analysis-WinML-WinUI3App)

A native Windows desktop app that provides a real-time customer support sentiment analysis dashboard. It uses the same RoBERTa sentiment model running **locally** via [Windows ML](https://aka.ms/winml) and [ONNX Runtime](https://onnxruntime.ai/), with hardware acceleration on **NPU**, **GPU**, or **CPU**.

- **Technology:** C#, WinUI 3, Windows App SDK, Windows ML
- **How to run:** See the [README](./Sentiment-Analysis-WinML-WinUI3App/README.md) for setup and instructions.
