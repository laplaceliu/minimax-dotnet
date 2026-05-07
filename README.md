# MiniMax-DotNet

MiniMax AI Platform .NET SDK

## Installation

```bash
dotnet add package MiniMax-DotNet
```

## Usage

```csharp
using MiniMax;

var client = new MiniMaxClient("your-api-key");

// Chat
var chatResponse = await client.ChatAsync(new ChatReq
{
    Model = ChatModel.M2_7,
    Messages = new List<ChatMessage>
    {
        new ChatMessage { Role = "user", Content = "Hello!" }
    }
});

// Text-to-Audio
var audioResponse = await client.TextToAudioAsync(new T2aReq
{
    Model = T2AModel.Speech02Fast,
    Text = "Hello, world!"
});

// Video Generation
var videoResponse = await client.GenerateVideoAsync(new VideoGenerationReq
{
    Model = VideoModel.MiniMaxHailuo23Fast,
    Prompt = "A beautiful sunset"
});

// Image Generation
var imageResponse = await client.GenerateImageAsync(new ImageGenerationReq
{
    Model = ImageModel.Image01,
    Prompt = "A cute cat"
});
```

## MCP Client (Model Context Protocol)

SDK 支持连接 MCP Server，使用 MCP 提供的工具（如 `web_search`、`understand_image` 等）。

### Token Plan MCP 使用示例

```csharp
// 创建 MCP Client，连接到 Token Plan MCP Server
var mcpClient = client.CreateMcpClient(
    "uvx",
    new[] { "minimax-coding-plan-mcp", "-y" },
    new Dictionary<string, string>
    {
        ["MINIMAX_API_KEY"] = "your-api-key",
        ["MINIMAX_API_HOST"] = "https://api.minimaxi.com"
    }
);

// 初始化连接
var initResult = await mcpClient.InitializeAsync();
Console.WriteLine($"Connected to MCP server: {initResult.ServerInfo.Name}");

// 列出可用工具
var tools = await mcpClient.ListToolsAsync();
foreach (var tool in tools)
{
    Console.WriteLine($"Tool: {tool.Name} - {tool.Description}");
}

// 使用 web_search 工具
var searchResult = await mcpClient.CallToolAsync("web_search", new Dictionary<string, object>
{
    ["query"] = "C# async programming"
});

// 使用 understand_image 工具
// 注意：image_source 支持 HTTP/HTTPS URL 或本地文件路径
var imageResult = await mcpClient.CallToolAsync("understand_image", new Dictionary<string, object>
{
    ["prompt"] = "What is in this image?",
    ["image_source"] = "https://example.com/image.jpg"  // 或本地路径如 "/path/to/image.jpg"
});

mcpClient.Dispose();
```

### MiniMax MCP 使用示例

```csharp
// 创建 MCP Client
var mcpClient = client.CreateMcpClient(
    "uvx",
    new[] { "minimax-mcp" },
    new Dictionary<string, string>
    {
        ["MINIMAX_API_KEY"] = "your-api-key",
        ["MINIMAX_MCP_BASE_PATH"] = "/tmp/mcp-output",
        ["MINIMAX_API_HOST"] = "https://api.minimaxi.com"
    }
);

// 初始化
await mcpClient.InitializeAsync();

// 列出工具
var tools = await mcpClient.ListToolsAsync();

// 生成图片
var imageResult = await mcpClient.CallToolAsync("text_to_image", new Dictionary<string, object>
{
    ["prompt"] = "A beautiful sunset",
    ["model"] = "image-01"
});

mcpClient.Dispose();
```

## Features

- **Chat**: 支持 MiniMax-M2.7, MiniMax-M2.5, MiniMax-M2.1 等模型
- **T2A**: Text-to-Speech 语音合成
- **T2V**: Text-to-Video 文生视频
- **I2V**: Image-to-Video 图生视频
- **S2V**: Subject-to-Video 主体参考视频
- **FL2V**: First-Last-Frame Video 首尾帧视频
- **Music**: 音乐生成
- **Voice Clone**: 语音克隆
- **MCP Client**: 支持连接 MCP Server 调用工具

## API Documentation

https://platform.minimaxi.com/docs
