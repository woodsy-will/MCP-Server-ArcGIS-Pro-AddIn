using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
namespace APBridgeAddIn
{
    internal class ProBridgeService : IDisposable
    {
        private readonly string _pipeName;
        private CancellationTokenSource _cts;
        private Task _serverLoop;
        public ProBridgeService(string pipeName) => _pipeName = pipeName;
        public void Start()
        {
            _cts = new CancellationTokenSource();
            _serverLoop = Task.Run(async () =>
            {
                try
                {
                    await RunAsync(_cts.Token);
                }
                catch (Exception ex)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(
                        $"Bridge service error: {ex.Message}\n\n{ex.StackTrace}",
                        "Bridge Service Error");
                }
            });
        }
        public void Dispose()
        {
            try { _cts?.Cancel(); _serverLoop?.Wait(2000); } catch { }
        }
        private async Task RunAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                using var server = new NamedPipeServerStream(_pipeName,
                PipeDirection.InOut, 1, PipeTransmissionMode.Message,
                PipeOptions.Asynchronous);

                // Pipe is now created and waiting for connections
                await server.WaitForConnectionAsync(ct);
                using var reader = new StreamReader(server, Encoding.UTF8,
                leaveOpen: true);
                using var writer = new StreamWriter(server, new
                UTF8Encoding(false), leaveOpen: true)
                { AutoFlush = true };
                // loop richieste su questa connessione
                while (server.IsConnected && !ct.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null) break;
                    IpcRequest req;
                    try
                    {
                        req =
                    JsonSerializer.Deserialize<IpcRequest>(line);
                    }
                    catch (Exception ex)
                    {
                        await SendAsync(writer, new IpcResponse(false,
                        $"parse:{ex.Message}", null));
                        continue;
                    }
                    try
                    {
                        var resp = await HandleAsync(req, ct);
                        await SendAsync(writer, resp);
                    }
                    catch (Exception ex)
                    {
                        await SendAsync(writer, new IpcResponse(false,
                        ex.Message, null));
                    }
                }
            }
        }
        private static Task SendAsync(StreamWriter w, IpcResponse resp)
        => w.WriteLineAsync(JsonSerializer.Serialize(resp));
        private static async Task<IpcResponse> HandleAsync(IpcRequest req,
        CancellationToken ct)
        {
            switch (req.Op)
            {
                case "pro.getActiveMapName":
                    var name = MapView.Active?.Map?.Name ?? "<none>";
                    return new(true, null, new { name });
                case "pro.listLayers":
                    var layers = await QueuedTask.Run(() =>
                    MapView.Active?.Map?.Layers.Select(l => l.Name).ToList() ?? new
                    List<string>());
                    return new(true, null, layers);
                case "pro.countFeatures":
                    {
                        if (req.Args == null)
                            return new(false, "arg 'layer' required", null);


                        if (!req.Args.TryGetValue("layer", out string? layerName) ||
                        string.IsNullOrWhiteSpace(layerName))
                            return new(false, "arg 'layer' required", null);


                        int count = await QueuedTask.Run(() =>
                        {
                            var fl = MapView.Active?.Map?.Layers
                            .OfType<FeatureLayer>()
                            .FirstOrDefault(l => l.Name.Equals(layerName, StringComparison.OrdinalIgnoreCase));
                            if (fl == null) return 0;
                            using var fc = fl.GetFeatureClass();
                            return (int)fc.GetCount();
                        });


                        return new(true, null, new { count });
                    }
                case "pro.zoomToLayer":
                    {
                        if (req.Args == null ||
                            !req.Args.TryGetValue("layer", out string? layerName) ||
                            string.IsNullOrWhiteSpace(layerName))
                            return new(false, "arg 'layer' required", null);


                        await QueuedTask.Run(async () =>
                        {
                            var fl = MapView.Active?.Map?.Layers
                            .OfType<FeatureLayer>()
                            .FirstOrDefault(l => l.Name.Equals(layerName, StringComparison.OrdinalIgnoreCase));
                            if (fl != null)
                                await MapView.Active!.ZoomToAsync(fl);
                        });


                        return new(true, null, new { done = true });
                    }
                case "pro.selectByAttribute":
                    {
                        if (req.Args == null ||
                        !req.Args.TryGetValue("layer", out string? layerName) ||
                        string.IsNullOrWhiteSpace(layerName) ||
                        !req.Args.TryGetValue("where", out string? where) ||
                        string.IsNullOrWhiteSpace(where))
                            return new(false, "args 'layer' & 'where' required", null);


                        await QueuedTask.Run(() =>
                        {
                            var fl = MapView.Active?.Map?.Layers
                            .OfType<FeatureLayer>()
                            .FirstOrDefault(l => l.Name.Equals(layerName, StringComparison.OrdinalIgnoreCase));
                            if (fl != null)
                            {
                                fl.Select(new ArcGIS.Core.Data.QueryFilter { WhereClause = where });
                            }
                        });


                        return new(true, null, new { done = true });
                    }
                default:
                    return new(false, $"op not found: {req.Op}", null);
            }
        }
    }
}
