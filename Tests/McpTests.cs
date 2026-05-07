using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using MiniMax;
using MiniMax.Models.Mcp;

namespace Tests
{
    public class McpTests : IDisposable
    {
        private readonly MiniMaxClient _client;
        private McpClient? _mcpClient;
        private readonly string _outputDir;

        public McpTests()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");
            _client = new MiniMaxClient(apiKey);
            _outputDir = Path.Combine(Path.GetTempPath(), "minimax-mcp-tests");
            Directory.CreateDirectory(_outputDir);
        }

        public void Dispose()
        {
            _mcpClient?.Dispose();
            _client.Dispose();
        }

        [Fact]
        public async Task Mcp_Connect_To_TokenPlan_Server()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");

            _mcpClient = _client.CreateMcpClient(
                "uvx",
                new[] { "minimax-coding-plan-mcp", "-y" },
                new Dictionary<string, string>
                {
                    ["MINIMAX_API_KEY"] = apiKey,
                    ["MINIMAX_API_HOST"] = "https://api.minimaxi.com"
                }
            );

            var initResult = await _mcpClient.InitializeAsync();
            Assert.NotNull(initResult);
            Assert.NotNull(initResult.ServerInfo);
            Console.WriteLine($"MCP Server: {initResult.ServerInfo.Name} v{initResult.ServerInfo.Version}");
            Console.WriteLine($"Protocol Version: {initResult.ProtocolVersion}");
        }

        [Fact]
        public async Task Mcp_List_Tools()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");

            _mcpClient = _client.CreateMcpClient(
                "uvx",
                new[] { "minimax-coding-plan-mcp", "-y" },
                new Dictionary<string, string>
                {
                    ["MINIMAX_API_KEY"] = apiKey,
                    ["MINIMAX_API_HOST"] = "https://api.minimaxi.com"
                }
            );

            await _mcpClient.InitializeAsync();
            var tools = await _mcpClient.ListToolsAsync();

            Assert.NotNull(tools);
            Assert.True(tools.Count > 0, "Should have at least one tool");

            foreach (var tool in tools)
            {
                Console.WriteLine($"Tool: {tool.Name}");
                if (tool.Description != null)
                {
                    Console.WriteLine($"  Description: {tool.Description}");
                }
            }
        }

        [Fact]
        public async Task Mcp_Web_Search()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");

            _mcpClient = _client.CreateMcpClient(
                "uvx",
                new[] { "minimax-coding-plan-mcp", "-y" },
                new Dictionary<string, string>
                {
                    ["MINIMAX_API_KEY"] = apiKey,
                    ["MINIMAX_API_HOST"] = "https://api.minimaxi.com"
                }
            );

            await _mcpClient.InitializeAsync();
            var result = await _mcpClient.CallToolAsync("web_search", new Dictionary<string, object>
            {
                ["query"] = "C# programming language latest version"
            });

            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            Assert.True(result.Content.Count > 0, "Should have at least one content item");

            foreach (var content in result.Content)
            {
                if (content.Type == "text" && content.Text != null)
                {
                    Console.WriteLine($"Search Result:\n{content.Text}");
                }
            }
        }

        [Fact]
        public async Task Mcp_Understand_Image()
        {
            var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY") ?? throw new InvalidOperationException("MINIMAX_API_KEY not set");

            _mcpClient = _client.CreateMcpClient(
                "uvx",
                new[] { "minimax-coding-plan-mcp", "-y" },
                new Dictionary<string, string>
                {
                    ["MINIMAX_API_KEY"] = apiKey,
                    ["MINIMAX_API_HOST"] = "https://api.minimaxi.com"
                }
            );

            await _mcpClient.InitializeAsync();

            var imagePath = "/tmp/minimax-image-tests/t2i_output.jpg";
            if (!File.Exists(imagePath))
            {
                Console.WriteLine($"Test image not found at {imagePath}, skipping test");
                return;
            }

            var result = await _mcpClient.CallToolAsync("understand_image", new Dictionary<string, object>
            {
                ["prompt"] = "What is in this image? Please describe it in detail.",
                ["image_source"] = imagePath
            });

            Assert.NotNull(result);
            Assert.NotNull(result.Content);
            Assert.True(result.Content.Count > 0, "Should have at least one content item");

            foreach (var content in result.Content)
            {
                if (content.Type == "text" && content.Text != null)
                {
                    Console.WriteLine($"Image Understanding Result:\n{content.Text}");
                }
            }
        }
    }
}