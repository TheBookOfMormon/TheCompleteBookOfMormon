# Ensure c:\temp\bom exists
$bomDir = 'c:\temp\bom'
if (-not (Test-Path $bomDir)) {
    New-Item -Path $bomDir -ItemType Directory | Out-Null
}

# Delete everything inside c:\temp\bom
Get-ChildItem -Path $bomDir -Recurse -Force | Remove-Item -Force -Recurse

# Define Sources path
$sourcesPath = Join-Path (Get-Location) 'Sources'

# Get all directories in Sources directory
Get-ChildItem -Path $sourcesPath -Directory | ForEach-Object {
    $dir = $_
    $indexJson = Join-Path $dir.FullName 'index.json'
    
    if (-not (Test-Path $indexJson)) {
        # Skip directory if index.json not present
        return
    }

    # Check for 03-OCR subfolder
    $ocrPath = Join-Path $dir.FullName '03-OCR'
    if (-not (Test-Path $ocrPath)) {
        # Skip if 03-OCR doesn't exist
        return
    }

    # Zip contents of 03-OCR into c:\temp\bom\{directoryname}.zip
    $zipTarget = Join-Path $bomDir ($dir.Name + '.zip')
    Compress-Archive -Path (Join-Path $ocrPath '*.PageJson') -DestinationPath $zipTarget -CompressionLevel NoCompression -Force
}

# Zip up all files in c:\temp\bom into All.zip
$allZipPath = Join-Path $bomDir 'All.zip'
Compress-Archive -Path (Join-Path $bomDir '*') -DestinationPath $allZipPath -CompressionLevel Optimal -Force
