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
    Model = ChatModel.Abab6_5SChat,
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

## Features

- **Chat**: 支持 Abab6.5S, Abab6.5G, ChatGLM4 等模型
- **T2A**: Text-to-Speech 语音合成
- **T2V**: Text-to-Video 文生视频
- **I2V**: Image-to-Video 图生视频
- **S2V**: Subject-to-Video 主体参考视频
- **FL2V**: First-Last-Frame Video 首尾帧视频
- **Music**: 音乐生成
- **Voice Clone**: 语音克隆

## API Documentation

https://platform.minimaxi.com/docs