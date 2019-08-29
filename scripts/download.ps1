param(
  [string]$url,
  [string]$outfolder,
  [string]$outfile
)


New-Item -ItemType Directory -Force -Path $outfolder | Out-Null
(New-Object System.Net.WebClient).DownloadFile($url, $outfolder + $outfile)
