using System.Text.Json.Serialization;
namespace ArcGisMcpServer.Ipc
{
    public record IpcRequest(
        [property: JsonPropertyName("op")] string Op,
        [property: JsonPropertyName("args")] Dictionary<string, string>? Args
    );
    public record IpcResponse(
        [property: JsonPropertyName("ok")] bool Ok,
        [property: JsonPropertyName("error")] string? Error,
        [property: JsonPropertyName("data")] object? Data
    );
}

