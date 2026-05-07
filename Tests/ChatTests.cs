using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MiniMax;
using MiniMax.Models;
using MiniMax.Models.Chat;
using static MiniMax.Models.Enums;

namespace Tests
{
    public class ChatTests : IDisposable
    {
        private readonly MiniMaxClient _client;

        public ChatTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            _client = new MiniMaxClient(apiKey);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        [Fact]
        public async Task Chat_Completion_Returns_Success()
        {
            var request = new ChatCompletionReq
            {
                Model = ChatModel.M2_7,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = "user",
                        Content = "Hello, who are you?"
                    }
                }
            };

            var response = await _client.ChatCompletionAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0 || response.BaseResp.StatusCode == 1000, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Choices);
            Assert.NotEmpty(response.Choices);
            var firstChoice = response.Choices[0];
            Assert.NotNull(firstChoice.Message);
            Assert.NotNull(firstChoice.Message.Content);
            Assert.NotNull(response.Usage);
            Console.WriteLine($"ID: {response.Id}");
            Console.WriteLine($"Model: {response.Model}");
            Console.WriteLine($"Input Tokens: {response.Usage.PromptTokens}");
            Console.WriteLine($"Output Tokens: {response.Usage.CompletionTokens}");
            Console.WriteLine($"Response: {firstChoice.Message.Content}");
        }

        [Fact]
        public async Task Chat_With_Temperature_And_TopP_Success()
        {
            var request = new ChatCompletionReq
            {
                Model = ChatModel.M2_7,
                Temperature = 0.7,
                TopP = 0.9,
                MaxCompletionTokens = 100,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = "user",
                        Content = "What is the capital of France?"
                    }
                }
            };

            var response = await _client.ChatCompletionAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0 || response.BaseResp.StatusCode == 1000, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Choices);
            Assert.NotEmpty(response.Choices);
            var text = response.Choices[0].Message?.Content;
            Assert.NotNull(text);
            Console.WriteLine($"Response: {text}");
        }

        [Fact]
        public async Task Chat_With_M2_1_Model_Success()
        {
            var request = new ChatCompletionReq
            {
                Model = ChatModel.M2_1,
                MaxCompletionTokens = 50,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = "user",
                        Content = "Give me a short answer: 2+2=?"
                    }
                }
            };

            var response = await _client.ChatCompletionAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0 || response.BaseResp.StatusCode == 1000, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Choices);
            Assert.NotEmpty(response.Choices);
            var text = response.Choices[0].Message?.Content;
            Assert.NotNull(text);
            Assert.NotNull(response.Usage);
            Console.WriteLine($"Input Tokens: {response.Usage.PromptTokens}");
            Console.WriteLine($"Output Tokens: {response.Usage.CompletionTokens}");
            Console.WriteLine($"Response: {text}");
        }

        [Fact]
        public async Task Chat_Multi_Turn_Conversation_Success()
        {
            var request = new ChatCompletionReq
            {
                Model = ChatModel.M2_7,
                MaxCompletionTokens = 100,
                Messages = new List<Message>
                {
                    new Message
                    {
                        Role = "user",
                        Content = "My name is Alice."
                    },
                    new Message
                    {
                        Role = "assistant",
                        Content = "Hello Alice! How can I help you today?"
                    },
                    new Message
                    {
                        Role = "user",
                        Content = "What is my name?"
                    }
                }
            };

            var response = await _client.ChatCompletionAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0 || response.BaseResp.StatusCode == 1000, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Choices);
            Assert.NotEmpty(response.Choices);
            var text = response.Choices[0].Message?.Content;
            Assert.NotNull(text);
            Console.WriteLine($"Response: {text}");
        }
    }
}