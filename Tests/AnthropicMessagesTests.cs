using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MiniMax;
using MiniMax.Models;
using MiniMax.Models.Anthropic;
using static MiniMax.Models.Enums;

namespace Tests
{
    public class AnthropicMessagesTests : IDisposable
    {
        private readonly MiniMaxClient _client;

        public AnthropicMessagesTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            _client = new MiniMaxClient(apiKey);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        [Fact]
        public async Task Anthropic_Messages_Create_Success()
        {
            var request = new CreateMessageReq
            {
                Model = ChatModel.M2_7,
                MaxTokens = 1024,
                Messages = new List<ContentBlock>
                {
                    new ContentBlock
                    {
                        Role = "user",
                        Content = "Hello, who are you?"
                    }
                }
            };

            var response = await _client.CreateAnthropicMessageAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Content);
            Assert.NotEmpty(response.Content);
            Assert.NotNull(response.Id);
            Console.WriteLine($"Response ID: {response.Id}");
            Console.WriteLine($"Model: {response.Model}");
            Console.WriteLine($"StopReason: {response.StopReason}");
            var textBlock = response.Content.FirstOrDefault(b => b.Type == "text");
            if (textBlock != null)
                Console.WriteLine($"Response: {textBlock.Text}");
        }

        [Fact]
        public async Task Anthropic_Messages_With_SystemPrompt_Success()
        {
            var request = new CreateMessageReq
            {
                Model = ChatModel.M2_7,
                MaxTokens = 1024,
                Temperature = 0.7,
                SystemInstruction = new List<ContentBlock>
                {
                    new ContentBlock
                    {
                        Content = "You are a helpful assistant.",
                        Type = "text"
                    }
                },
                Messages = new List<ContentBlock>
                {
                    new ContentBlock
                    {
                        Role = "user",
                        Content = "What is the capital of France?"
                    }
                }
            };

            var response = await _client.CreateAnthropicMessageAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Content);
            Assert.NotEmpty(response.Content);
            var textBlock = response.Content.FirstOrDefault(b => b.Type == "text");
            Assert.NotNull(textBlock);
            Console.WriteLine($"Response: {textBlock.Text}");
        }

        [Fact]
        public async Task Anthropic_Messages_With_M2_1_Model_Success()
        {
            var request = new CreateMessageReq
            {
                Model = ChatModel.M2_1,
                MaxTokens = 512,
                Messages = new List<ContentBlock>
                {
                    new ContentBlock
                    {
                        Role = "user",
                        Content = "Give me a short answer: 2+2=?"
                    }
                }
            };

            var response = await _client.CreateAnthropicMessageAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Content);
            Assert.NotEmpty(response.Content);
            Assert.NotNull(response.Usage);
            Console.WriteLine($"Input Tokens: {response.Usage.InputTokens}");
            Console.WriteLine($"Output Tokens: {response.Usage.OutputTokens}");
            var textBlock = response.Content.FirstOrDefault(b => b.Type == "text");
            Assert.NotNull(textBlock);
            Console.WriteLine($"Response: {textBlock.Text}");
        }

        [Fact]
        public async Task Anthropic_Messages_Multi_Turn_Conversation_Success()
        {
            var request = new CreateMessageReq
            {
                Model = ChatModel.M2_7,
                MaxTokens = 1024,
                Messages = new List<ContentBlock>
                {
                    new ContentBlock
                    {
                        Role = "user",
                        Content = "My name is Alice."
                    },
                    new ContentBlock
                    {
                        Role = "assistant",
                        Content = "Hello Alice! How can I help you today?"
                    },
                    new ContentBlock
                    {
                        Role = "user",
                        Content = "What is my name?"
                    }
                }
            };

            var response = await _client.CreateAnthropicMessageAsync(request);

            Assert.NotNull(response);
            Assert.NotNull(response.BaseResp);
            Assert.True(response.BaseResp.StatusCode == 0, $"StatusCode: {response.BaseResp.StatusCode}");
            Assert.NotNull(response.Content);
            Assert.NotEmpty(response.Content);
            var textBlock = response.Content.FirstOrDefault(b => b.Type == "text");
            Assert.NotNull(textBlock);
            Console.WriteLine($"Response: {textBlock.Text}");
        }
    }
}
