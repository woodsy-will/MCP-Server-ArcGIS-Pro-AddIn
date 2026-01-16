# Check for Named Pipes
Write-Host "Checking for Named Pipes..." -ForegroundColor Cyan
Write-Host ""

# Try to list all pipes (requires admin in some cases)
try {
    $pipes = [System.IO.Directory]::GetFiles("\\.\pipe\")

    Write-Host "Total pipes found: $($pipes.Count)" -ForegroundColor Green
    Write-Host ""

    # Look for ArcGIS-related pipes
    $arcgisPipes = $pipes | Where-Object { $_ -like "*ArcGis*" -or $_ -like "*Bridge*" }

    if ($arcgisPipes) {
        Write-Host "ArcGIS/Bridge-related pipes:" -ForegroundColor Yellow
        $arcgisPipes | ForEach-Object {
            Write-Host "  - $_" -ForegroundColor White
        }
    } else {
        Write-Host "No ArcGIS or Bridge-related pipes found." -ForegroundColor Red
        Write-Host ""
        Write-Host "This means the bridge service is NOT running or failed to start." -ForegroundColor Red
    }

    # Show first 10 pipes for reference
    Write-Host ""
    Write-Host "Sample of existing pipes (first 10):" -ForegroundColor Cyan
    $pipes | Select-Object -First 10 | ForEach-Object {
        Write-Host "  - $($_.Replace('\\.\pipe\', ''))"
    }
} catch {
    Write-Host "Error accessing Named Pipes: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Try running this script as Administrator" -ForegroundColor Yellow
}
