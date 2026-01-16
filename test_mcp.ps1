# Test MCP Server Connection
$projectPath = "C:\Users\wsteinley\AAA_CODE_ROOT_FOLDERS\Custom_Tools\Cloned_repos_from_github\MCP-Server-ArcGIS-Pro-AddIn\McpServer\ArcGisMcpServer\ArcGisMcpServer.csproj"

Write-Host "Testing MCP Server Tools..." -ForegroundColor Cyan
Write-Host ""

# Start the MCP server process
$process = Start-Process -FilePath "dotnet" -ArgumentList "run","--project",$projectPath -NoNewWindow -PassThru -RedirectStandardInput "test_input.txt" -RedirectStandardOutput "test_output.txt" -RedirectStandardError "test_error.txt"

# Wait a moment for server to start
Start-Sleep -Seconds 2

# Create test requests
$requests = @(
    '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"test-client","version":"1.0"}}}'
    '{"jsonrpc":"2.0","id":2,"method":"tools/list"}'
    '{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"Ping"}}'
)

# Send requests
foreach ($req in $requests) {
    $req | Out-File -FilePath "test_input.txt" -Encoding utf8 -Append
}

# Wait for responses
Start-Sleep -Seconds 3

# Stop the process
Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue

# Show output
Write-Host "Output:" -ForegroundColor Green
if (Test-Path "test_output.txt") {
    Get-Content "test_output.txt"
}

Write-Host ""
Write-Host "Errors:" -ForegroundColor Yellow
if (Test-Path "test_error.txt") {
    Get-Content "test_error.txt" | Select-Object -First 20
}

# Cleanup
Remove-Item "test_input.txt" -ErrorAction SilentlyContinue
Remove-Item "test_output.txt" -ErrorAction SilentlyContinue
Remove-Item "test_error.txt" -ErrorAction SilentlyContinue
