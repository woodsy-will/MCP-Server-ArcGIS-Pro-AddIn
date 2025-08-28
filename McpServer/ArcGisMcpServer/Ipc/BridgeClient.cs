using System.IO.Pipes;
using System.Text;
using System.Text.Json;
namespace ArcGisMcpServer.Ipc
{
    public class BridgeClient
    {
        private readonly string _pipeName;
        public BridgeClient(string pipeName) => _pipeName = pipeName;
        public async Task<IpcResponse> SendAsync(IpcRequest req,
        CancellationToken ct = default)
        {
            using var client = new NamedPipeClientStream(".", _pipeName,
            PipeDirection.InOut, PipeOptions.Asynchronous);
            await client.ConnectAsync(2000, ct); // timeout 2s
            using var reader = new StreamReader(client, Encoding.UTF8,
            leaveOpen: true);
            using var writer = new StreamWriter(client, new
            UTF8Encoding(false), leaveOpen: true)
            { AutoFlush = true };
            await writer.WriteLineAsync(JsonSerializer.Serialize(req));
            var line = await reader.ReadLineAsync();
            if (line is null) throw new IOException("bridge no response");
            return JsonSerializer.Deserialize<IpcResponse>(line) ??
            new(false, "deserialize", null);
        }
        public Task<IpcResponse> OpAsync(string op,
        Dictionary<string, string>? args = null, CancellationToken ct = default)
        => SendAsync(new IpcRequest(op, args), ct);
    }
}
