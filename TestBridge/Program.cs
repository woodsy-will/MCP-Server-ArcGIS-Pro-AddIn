using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("==========================================================");
        Console.WriteLine("ArcGIS Pro Named Pipe Bridge Connection Test");
        Console.WriteLine("==========================================================\n");

        string pipeName = "ArcGisProBridgePipe";
        Console.WriteLine($"Pipe Name: {pipeName}");
        Console.WriteLine($"Full Path: \\\\.\\pipe\\{pipeName}\n");

        Console.WriteLine("Attempting to connect to Named Pipe...");
        Console.WriteLine("(Make sure ArcGIS Pro is running with 'Start server mcp' clicked)\n");

        try
        {
            using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);

            Console.WriteLine("Connecting with 5 second timeout...");
            await client.ConnectAsync(5000);

            Console.WriteLine("[SUCCESS] Connected to Named Pipe!\n");

            // Test 1: Ping (doesn't require ArcGIS)
            await TestOperation(client, "pro.ping", null, "Ping Test");

            // Test 2: Get Active Map Name
            await TestOperation(client, "pro.getActiveMapName", null, "Get Active Map Name");

            // Test 3: List Layers
            await TestOperation(client, "pro.listLayers", null, "List Layers");

            Console.WriteLine("\n[SUCCESS] All tests completed!");
            return;
        }
        catch (TimeoutException)
        {
            Console.WriteLine("[ERROR] Connection timed out.");
            Console.WriteLine("\nTroubleshooting:");
            Console.WriteLine("  1. Is ArcGIS Pro running?");
            Console.WriteLine("  2. Did you click the 'Start server mcp' button?");
            Console.WriteLine("  3. Check ArcGIS Pro for any error messages");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"[ERROR] IO Exception: {ex.Message}");
            Console.WriteLine("\nPossible causes:");
            Console.WriteLine("  - Named Pipe server not started");
            Console.WriteLine("  - Bridge service failed to start");
            Console.WriteLine("  - Pipe name mismatch");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Unexpected error: {ex.Message}");
            Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
        }
    }

    static async Task TestOperation(NamedPipeClientStream pipe, string operation, object? args, string testName)
    {
        Console.WriteLine($"--- {testName} ---");

        try
        {
            // Create request
            var request = new
            {
                op = operation,
                args = args
            };

            string requestJson = JsonSerializer.Serialize(request) + "\n";
            Console.WriteLine($"Request: {requestJson.Trim()}");

            // Send request
            byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);
            await pipe.WriteAsync(requestBytes, 0, requestBytes.Length);
            await pipe.FlushAsync();

            // Read response
            using var reader = new StreamReader(pipe, Encoding.UTF8, leaveOpen: true);
            string? responseLine = await reader.ReadLineAsync();

            if (responseLine == null)
            {
                Console.WriteLine("[WARN] No response received");
                return;
            }

            Console.WriteLine($"Response: {responseLine}");

            // Parse response
            var responseDoc = JsonDocument.Parse(responseLine);
            bool ok = responseDoc.RootElement.GetProperty("ok").GetBoolean();

            if (ok)
            {
                var data = responseDoc.RootElement.GetProperty("data");
                Console.WriteLine($"[SUCCESS] Data: {data}");
            }
            else
            {
                string error = responseDoc.RootElement.GetProperty("error").GetString() ?? "Unknown error";
                Console.WriteLine($"[ERROR] {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
        }

        Console.WriteLine();
    }
}
