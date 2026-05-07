using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MiniMax;
using MiniMax.Models;
using MiniMax.Models.Speech;
using static MiniMax.Models.Enums;

namespace Tests
{
    public class SpeechTests : IDisposable
    {
        private readonly MiniMaxClient _client;
        private readonly string _outputDir;

        public SpeechTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            _client = new MiniMaxClient(apiKey);
            _outputDir = Path.Combine(Path.GetTempPath(), "minimax-speech-tests");
            Directory.CreateDirectory(_outputDir);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        [Fact]
        public async Task T2a_Http_NonStreaming_Success()
        {
            var request = new T2aV2Req
            {
                Model = SpeechModel.Speech28Hd,
                Text = "Hello, this is a test.",
                Stream = false,
                VoiceSetting = new T2aV2VoiceSetting
                {
                    VoiceId = "male-qn-qingse",
                    Speed = 1.0f,
                    Vol = 1.0f,
                    Pitch = 0
                },
                AudioSetting = new T2aV2AudioSetting
                {
                    SampleRate = AudioSampleRate.Rate32000,
                    Bitrate = AudioBitrate.Rate128000,
                    Format = T2AAudioFormat.Mp3
                }
            };

            var response = await _client.TextToAudioAsync(request);

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

        [Fact]
        public async Task T2a_Async_Create_And_Query()
        {
            var request = new T2AAsyncV2Req
            {
                Model = SpeechModel.Speech28Hd,
                Text = "This is a test for async text-to-speech synthesis. The async API is designed for processing large amounts of text content.",
                VoiceSetting = new T2AAsyncV2VoiceSetting
                {
                    VoiceId = "male-qn-qingse",
                    Speed = 1.0f,
                    Vol = 1.0f,
                    Pitch = 0
                },
                AudioSetting = new T2AAsyncV2AudioSetting
                {
                    SampleRate = AudioSampleRate.Rate32000,
                    Bitrate = AudioBitrate.Rate128000,
                    Format = AudioFormat.Mp3,
                    Channel = 1
                }
            };

            var createResponse = await _client.TextToAudioAsyncCreateAsync(request);

            Assert.NotNull(createResponse);
            Assert.NotNull(createResponse.BaseResp);
            Console.WriteLine($"Create StatusCode: {createResponse.BaseResp.StatusCode}, StatusMsg: {createResponse.BaseResp.StatusMsg}");
            Assert.True(createResponse.BaseResp.StatusCode == 0, $"StatusCode: {createResponse.BaseResp.StatusCode}");

            var taskId = createResponse.TaskId;
            Assert.True(taskId > 0, $"TaskId is not valid: {taskId}");
            Console.WriteLine($"TaskId: {taskId}");

            var queryResponse = await _client.TextToAudioAsyncQueryAsync(taskId);

            Assert.NotNull(queryResponse);
            Assert.NotNull(queryResponse.BaseResp);
            Console.WriteLine($"Query StatusCode: {queryResponse.BaseResp.StatusCode}, Status: {queryResponse.Status}");
            Assert.True(queryResponse.BaseResp.StatusCode == 0, $"StatusCode: {queryResponse.BaseResp.StatusCode}");

            string? status = null;
            long? resultFileId = null;

            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(2000);

                queryResponse = await _client.TextToAudioAsyncQueryAsync(taskId);

                Assert.NotNull(queryResponse);
                Assert.NotNull(queryResponse.BaseResp);
                Assert.True(queryResponse.BaseResp.StatusCode == 0, $"Query StatusCode: {queryResponse.BaseResp.StatusCode}");

                status = queryResponse.Status;
                resultFileId = queryResponse.FileId;

                Console.WriteLine($"Poll {i + 1}: Status = {status}, FileId = {resultFileId}");

                if (status?.ToLowerInvariant() == "success" || status?.ToLowerInvariant() == "failed")
                    break;
            }

            Assert.Equal("success", status?.ToLowerInvariant());
            Assert.NotNull(resultFileId);
            Assert.True(resultFileId > 0);

            Console.WriteLine($"Downloading file_id: {resultFileId}");

            var fileStream = await _client.RetrieveFileContentAsync(resultFileId.Value);

            Assert.NotNull(fileStream);
            var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            var audioBytes = memoryStream.ToArray();

            Assert.True(audioBytes.Length > 0, "Downloaded audio is empty");
            Console.WriteLine($"Downloaded audio size: {audioBytes.Length} bytes");

            var audioPath = Path.Combine(_outputDir, "async_create_query.mp3");
            await File.WriteAllBytesAsync(audioPath, audioBytes);
            Console.WriteLine($"Audio saved to: {audioPath}");
        }

        [Fact]
        public async Task T2a_Async_With_File_Download()
        {
            var createRequest = new T2AAsyncV2Req
            {
                Model = SpeechModel.Speech28Hd,
                Text = "Hello world, this is async speech synthesis test.",
                VoiceSetting = new T2AAsyncV2VoiceSetting
                {
                    VoiceId = "male-qn-qingse",
                    Speed = 1.0f,
                    Vol = 1.0f,
                    Pitch = 0
                },
                AudioSetting = new T2AAsyncV2AudioSetting
                {
                    SampleRate = AudioSampleRate.Rate32000,
                    Bitrate = AudioBitrate.Rate128000,
                    Format = AudioFormat.Mp3,
                    Channel = 1
                }
            };

            var createResponse = await _client.TextToAudioAsyncCreateAsync(createRequest);

            Assert.NotNull(createResponse);
            Assert.NotNull(createResponse.BaseResp);
            Assert.True(createResponse.BaseResp.StatusCode == 0, $"Create StatusCode: {createResponse.BaseResp.StatusCode}");

            var taskId = createResponse.TaskId;
            Assert.True(taskId > 0, $"TaskId is not valid: {taskId}");
            Console.WriteLine($"Created task: {taskId}");

            string? status = null;
            long? resultFileId = null;

            for (int i = 0; i < 30; i++)
            {
                await Task.Delay(2000);

                var queryResponse = await _client.TextToAudioAsyncQueryAsync(taskId);

                Assert.NotNull(queryResponse);
                Assert.NotNull(queryResponse.BaseResp);
                Assert.True(queryResponse.BaseResp.StatusCode == 0, $"Query StatusCode: {queryResponse.BaseResp.StatusCode}");

                status = queryResponse.Status;
                resultFileId = queryResponse.FileId;

                Console.WriteLine($"Poll {i + 1}: Status = {status}, FileId = {resultFileId}");

                if (status?.ToLowerInvariant() == "success" || status?.ToLowerInvariant() == "failed")
                    break;
            }

            Assert.Equal("success", status?.ToLowerInvariant());
            Assert.NotNull(resultFileId);
            Assert.True(resultFileId > 0);

            Console.WriteLine($"Downloading file_id: {resultFileId}");

            var fileStream = await _client.RetrieveFileContentAsync(resultFileId.Value);

            Assert.NotNull(fileStream);
            var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            var audioBytes = memoryStream.ToArray();

            Assert.True(audioBytes.Length > 0, "Downloaded audio is empty");
            Console.WriteLine($"Downloaded audio size: {audioBytes.Length} bytes");

            var audioPath = Path.Combine(_outputDir, "async_audio.mp3");
            await File.WriteAllBytesAsync(audioPath, audioBytes);
            Console.WriteLine($"Audio saved to: {audioPath}");
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
