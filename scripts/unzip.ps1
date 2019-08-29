param(
  [string]$zipfile,
  [string]$outpath
)

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::ExtractToDirectory($zipfile, $outpath)

