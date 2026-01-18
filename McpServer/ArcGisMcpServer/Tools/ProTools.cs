using ArcGisMcpServer.Ipc;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace ArcGisMcpServer.Tools
{

    [McpServerToolType]
    public static class ProTools
    {
        private static BridgeClient? _client;
        public static void Configure(BridgeClient client) => _client = client;

        [McpServerTool, Description("Name of the active map in ArcGIS Pro")]
        public static async Task<string> GetActiveMapName()
        {
            var r = await _client!.OpAsync("pro.getActiveMapName");
            if (!r.Ok) throw new Exception(r.Error);
            return ((System.Text.Json.JsonElement)r.Data!).GetProperty("name").GetString()!;
        }

        [McpServerTool, Description("List of layers in the active map")]
        public static async Task<List<string>> ListLayers()
        {
            var r = await _client!.OpAsync("pro.listLayers");
            if (!r.Ok) throw new Exception(r.Error);
            return
            System.Text.Json.JsonSerializer.Deserialize<List<string>>(r.Data!.ToString()!)!;
        }

        [McpServerTool, Description("Count features in a layer by name")]
        public static async Task<int> CountFeatures(string layer)
        {
            var r = await _client!.OpAsync("pro.countFeatures", new()
            {
                ["layer"] = layer
            });
            if (!r.Ok) throw new Exception(r.Error);
            return
            ((System.Text.Json.JsonElement)r.Data!).GetProperty("count").GetInt32();
        }

        [McpServerTool, Description("Zoom to a layer's extent by name")]
        public static async Task<bool> ZoomToLayer(string layer)
        {
            var r = await _client!.OpAsync("pro.zoomToLayer", new()
            {
                ["layer"] = layer
            });
            if (!r.Ok) throw new Exception(r.Error);
            return true;
        }

        [McpServerTool, Description("Select features in a layer by SQL where clause")]
        public static async Task<bool> SelectByAttribute(string layer, string where)
        {
            var r = await _client!.OpAsync("pro.selectByAttribute", new()
            {
                ["layer"] = layer,
                ["where"] = where
            });
            if (!r.Ok) throw new Exception(r.Error);
            return true;
        }

        [McpServerTool, Description("Ping test to validate the MCP server (without depending on ArcGIS Pro)")]
        public static Task<string> Ping()
        {
            // Non usare il bridge IPC: questo ping verifica solo che il server MCP sia attivo.
            return Task.FromResult($"pong {DateTimeOffset.UtcNow:O}");
        }

        [McpServerTool, Description("MCP echo test")]
        public static string Echo(string text)
        {
            return $"echo: {text}";
        }
    }
}
