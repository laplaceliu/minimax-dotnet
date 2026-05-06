using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Microsoft.Kiota.Abstractions;
using Xunit;
using MiniMax.Models;
using MiniMaxClient = MiniMax.MiniMaxClient;

namespace Tests
{
    public class SpeechTests : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly MiniMaxClient _client;
        private readonly string _outputDir;

        public SpeechTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            var authHandler = new AuthHandler(new HttpClientHandler(), apiKey);
            _httpClient = new HttpClient(authHandler);
            var adapter = new HttpClientRequestAdapter(new FixedAuthProvider(), null, null, _httpClient, null);
            _client = new MiniMaxClient(adapter);
            _outputDir = Path.Combine(Path.GetTempPath(), "minimax-speech-tests");
            Directory.CreateDirectory(_outputDir);
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        [Fact]
        public async Task T2a_Http_NonStreaming_Success()
        {
            var request = new T2aV2Req
            {
                Model = T2aV2Req_model.Speech28Hd,
                Text = "Hello, this is a test.",
                Stream = false,
                VoiceSetting = new T2AVoiceSetting
                {
                    VoiceId = "male-qn-qingse",
                    Speed = 1.0f,
                    Vol = 1.0f,
                    Pitch = 0
                },
                AudioSetting = new T2AAudioSetting
                {
                    SampleRate = 32000,
                    Bitrate = 128000,
                    Format = T2AAudioSetting_format.Mp3,
                    Channel = 1
                },
                OutputFormat = T2aV2Req_output_format.Hex
            };

            var response = await _client.V1.T2a_v2.PostAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Console.WriteLine($"StatusCode: {response.BaseResp.StatusCode}, StatusMsg: {response.BaseResp.StatusMsg}");
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Audio);
            Assert.True(response.Data.Status == 2, $"Status: {response.Data.Status}");

            var audioBytes = HexToBytes(response.Data.Audio);
            Assert.True(audioBytes.Length > 0);

            var audioPath = Path.Combine(_outputDir, "http_nonstreaming.mp3");
            await File.WriteAllBytesAsync(audioPath, audioBytes);
            Console.WriteLine($"Audio saved to: {audioPath}");
        }

        [Fact]
        public async Task T2a_WebSocket_Success()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            var wsUrl = "wss://api.minimaxi.com/ws/v1/t2a_v2";

            using var client = new ClientWebSocket();
            client.Options.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            await client.ConnectAsync(new Uri(wsUrl), CancellationToken.None);
            Console.WriteLine("WebSocket connected");

            var taskStart = new Dictionary<string, object>
            {
                { "event", "task_start" },
                { "model", "speech-2.8-hd" },
                { "voice_setting", new Dictionary<string, object>
                    {
                        { "voice_id", "male-qn-qingse" },
                        { "speed", 1.0 },
                        { "vol", 1.0 },
                        { "pitch", 0 }
                    }
                },
                { "audio_setting", new Dictionary<string, object>
                    {
                        { "sample_rate", 32000 },
                        { "bitrate", 128000 },
                        { "format", "mp3" },
                        { "channel", 1 }
                    }
                }
            };

            var startJson = JsonSerializer.Serialize(taskStart);
            await client.SendAsync(Encoding.UTF8.GetBytes(startJson), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine("Sent task_start");

            var allAudio = new List<byte>();
            var taskStarted = false;
            var complete = false;

            while (client.State == WebSocketState.Open && !complete)
            {
                WebSocketReceiveResult result;
                var message = new List<byte>();

                do
                {
                    var buffer = new byte[8192];
                    result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("Received Close message");
                        complete = true;
                        break;
                    }

                    if (result.Count > 0)
                    {
                        message.AddRange(buffer.Take(result.Count));
                    }
                } while (!result.EndOfMessage);

                if (complete) break;

                var json = Encoding.UTF8.GetString(message.ToArray());
                Console.WriteLine($"Received: {json}");

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("event", out var eventProp))
                {
                    var eventStr = eventProp.GetString();
                    Console.WriteLine($"Event: {eventStr}");

                    if (eventStr == "task_started" && !taskStarted)
                    {
                        taskStarted = true;
                        var taskContinue = new Dictionary<string, object>
                        {
                            { "event", "task_continue" },
                            { "text", "Hello, this is a WebSocket test." }
                        };
                        var continueJson = JsonSerializer.Serialize(taskContinue);
                        await client.SendAsync(Encoding.UTF8.GetBytes(continueJson), WebSocketMessageType.Text, true, CancellationToken.None);
                        Console.WriteLine("Sent task_continue");
                    }
                }

                if (root.TryGetProperty("event", out var evt) && evt.GetString() == "task_continued")
                {
                    if (root.TryGetProperty("data", out var dataProp) && dataProp.ValueKind != JsonValueKind.Null)
                    {
                        if (dataProp.TryGetProperty("audio", out var audioProp))
                        {
                            var hexAudio = audioProp.GetString();
                            if (!string.IsNullOrEmpty(hexAudio))
                            {
                                allAudio.AddRange(HexToBytes(hexAudio));
                                Console.WriteLine($"Added audio chunk, total: {allAudio.Count}");
                            }
                        }
                    }
                    if (root.TryGetProperty("is_final", out var isFinal) && isFinal.GetBoolean())
                    {
                        var taskFinish = new Dictionary<string, object> { { "event", "task_finish" } };
                        var finishJson = JsonSerializer.Serialize(taskFinish);
                        await client.SendAsync(Encoding.UTF8.GetBytes(finishJson), WebSocketMessageType.Text, true, CancellationToken.None);
                        break;
                    }
                }
            }

            Console.WriteLine($"Total audio bytes: {allAudio.Count}");
            Assert.True(allAudio.Count > 0, "No audio data received");
            var audioPath = Path.Combine(_outputDir, "websocket_audio.mp3");
            await File.WriteAllBytesAsync(audioPath, allAudio.ToArray());
            Console.WriteLine($"WebSocket audio saved to: {audioPath}");

            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }

        private static byte[] HexToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return Array.Empty<byte>();
            hex = hex.Replace(" ", "").Replace("\n", "").Replace("\r", "");
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }
    }
}