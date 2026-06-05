<a name="start-building"></a>
<br>
<p align="center">
<img src="img/banner-build-26.png" alt="Microsoft Build 2026" width="1200"/>
</p>

# [Microsoft Build 2026](https://build.microsoft.com)

## 🔥 OD851: Inferencing Local AI Models on all Windows PCs with Windows ML

### Session Description

Learn how to run custom and open-source AI models locally on Windows using Windows ML. This demo covers converting and optimizing models from Hugging Face using the Windows ML CLI, running inference in a web app via ONNX Runtime Web and WebNN — powered by Windows ML on Windows, and building a native Windows app that uses the same model for deeper Windows integration — all accelerated on CPU, GPU, and NPU without cloud costs.

### 🚀 Getting started

If you're following along at your own pace:
- Clone this repository
- Then see instructions in the code samples for next steps

### 💾 Code Samples

The demo code for this session lives in the [`src/`](./src) folder. There are two sample apps:

| Sample | Description |
|:-------|:------------|
| [Sentiment Analysis — WebNN Web App](./src/Sentiment-Analysis-WinML-WebNN-WebApp) | Web-based sentiment analysis dashboard using ONNX Runtime Web and WebNN, powered by Windows ML in the browser |
| [Sentiment Analysis — WinUI 3 Native App](./src/Sentiment-Analysis-WinML-WinUI3App) | Native Windows desktop sentiment analysis dashboard using Windows ML and Windows App SDK |

See the [`src/` README](./src/README.md) for an overview, and each sample's own README for setup and run instructions.

### 🧠 Learning Outcomes

By the end of this demo, you will be able to:

- Convert and optimize a model from Hugging Face to run on NPU, GPU, and CPU Windows PCs using the Windows ML CLI
- Use that model in a web app running inside Microsoft Edge or Google Chrome via ONNX Runtime Web and WebNN — powered by Windows ML on Windows
- Use that same model in a native Windows app using Windows ML for deeper Windows integration

### 💬 Keep Learning with Copilot

Try these prompts with GitHub Copilot to explore the topics from this demo. Open Copilot Chat in Visual Studio Code (`Ctrl+Alt+I` on Windows/Linux, `Cmd+Shift+I` on Mac), paste a prompt, and see what you learn. Try connecting the [Microsoft Learn MCP Server](#-microsoft-learn-mcp-server) for the latest official documentation.

Use these as a starting point — or write your own!

- *How do I install and use the Windows ML CLI to export and optimize a model from Hugging Face?*
- *What is the difference between running a model on CPU, GPU, and NPU with Windows ML, and when should I choose each?*
- *How do I use ONNX Runtime Web with WebNN to run a model locally in a web browser?*
- *Show me how to set up execution providers in a native Windows app using Windows ML and Windows App SDK.*
- *What is the difference between Windows ML, Windows AI APIs, and Foundry Local — when should I use each?*

### 💻 Technologies Used

1. [Windows ML](https://learn.microsoft.com/windows/ai/new-windows-ml/overview) — Unified local AI inferencing framework for Windows, powered by ONNX Runtime
1. [Windows ML CLI](https://aka.ms/winmlcli) — Command-line tool for converting and optimizing AI models for Windows
1. [ONNX Runtime](https://onnxruntime.ai/docs/) — Cross-platform ML inferencing engine used by Windows ML
1. [ONNX Runtime Web](https://onnxruntime.ai/docs/tutorials/web/) — ONNX Runtime for browser-based inference
1. [WebNN](https://aka.ms/webnn) — Web Neural Network API for hardware-accelerated ML in the browser, powered by Windows ML on Windows
1. [Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/) — Framework for building modern Windows apps
1. [Hugging Face](https://huggingface.co/) — Community platform hosting thousands of open-source AI models

### 📚 Resources and Next Steps

| Resource | Description |
|:---------|:------------|
| [Windows ML documentation](https://aka.ms/winml) | Official Windows ML docs — get started, tutorials, and API reference |
| [Windows ML on GitHub](https://github.com/microsoft/WindowsAppSDK) | File issues, browse samples, and follow development |
| [AI Dev Gallery](https://learn.microsoft.com/windows/ai/ai-dev-gallery/) | Sample app with numerous AI models running locally via Windows ML, including code snippets |
| [Microsoft Foundry on Windows overview](https://learn.microsoft.com/windows/ai/overview) | Learn about Windows AI APIs, Foundry Local, and Windows ML together |
| [https://aka.ms/build26-next-steps](https://aka.ms/build26-next-steps) | Explore lab and demo repos to further your learning from Microsoft Build |
| [Watch the session recording](https://aka.ms/build26/OD851/youtube) | Watch the recorded Microsoft Build session. |


### 🌟 Microsoft Learn MCP Server

The Microsoft Learn MCP Server gives your AI agent direct access to Microsoft's official documentation — grounded, up-to-date answers about the products and services covered in this session.

**VS Code** — One click installation: 

[![Install in VS Code](https://img.shields.io/badge/VS_Code-Install_Microsoft_Learn_MCP-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect/mcp/install?name=microsoft-learn&config=%7B%22type%22%3A%22http%22%2C%22url%22%3A%22https%3A%2F%2Flearn.microsoft.com%2Fapi%2Fmcp%22%7D)


**GitHub Copilot CLI** — Run this to install the Learn MCP Server as a plugin:
```
/plugin install microsoftdocs/mcp
```

For more info, other clients, and to post questions, visit the [Learn MCP Server repo](https://aka.ms/learnmcp).

## Content Owners

<table>
<tr>
    <td align="center"><a href="http://github.com/andrewleader">
        <img src="https://github.com/andrewleader.png" width="100px;" alt="Andrew Leader"/><br />
        <sub><b>Andrew Leader</b></sub></a><br />
            <a href="https://github.com/andrewleader" title="talk">📢</a>
    </td>
    <td align="center"><a href="http://github.com/mahabayana">
        <img src="https://github.com/mahabayana.png" width="100px;" alt="Maha Bayana"/><br />
        <sub><b>Maha Bayana</b></sub></a><br />
            <a href="https://github.com/mahabayana" title="talk">📢</a>
    </td>
</tr></table>

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit [Contributor License Agreements](https://cla.opensource.microsoft.com).

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft
trademarks or logos is subject to and must follow
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
