Add-Type -AssemblyName "System.IO.Compression.FileSystem"

# Original: http://jameskovacs.com/2010/02/25/the-exec-problem
function Exec ([scriptblock]$command, [string]$errorMessage = "Error executing command: " + $command) {
    $output = & $command
    if ((-not $?) -or ($lastexitcode -ne 0)) {
        Write-Host $output
        throw $errorMessage
    }
    return $output
}

function Test-ExitCode([string]$errorMessage = "Error executing command.") {
    if ((-not $?) -or ($lastexitcode -ne 0)) {
        throw $errorMessage
    }
}

# Sets the current folder and executes the given script.
# When the script finishes sets the original current folder.
function Invoke-InLocation([string]$path, [scriptblock]$command) {
    try {
        Push-Location $path
        & $command
    }
    finally {
        Pop-Location
    }
}

function ConvertTo-UnixLineEndings([string]$fileName) {
    Get-ChildItem $fileName | ForEach-Object {
        $content = [IO.File]::ReadAllText($_) -Replace "`r`n?", "`n"
        $utf8 = New-Object System.Text.UTF8Encoding $false
        [IO.File]::WriteAllText($_, $content, $utf8)
    }
}

function Write-Header([string]$text) {
    Write-Host
    Write-Host "================================================"
    Write-Host $text
    Write-Host "================================================"
}

function Get-ExecutablePath([string]$name, [string]$directory, [string]$envVar) {
    $path = [environment]::GetEnvironmentVariable($envVar, "Process")

    if (!$path) {
        if (!$directory) {
            $path = Exec { & where.exe $name } `
                | Select-Object -First 1
        }
        else {
            $path = Exec { & where.exe /R $directory $name } `
                | Select-Object -First 1
        }
    }

    if (Test-Path $path) {
        Write-Debug "Found ${name} at ${path}"
        [environment]::SetEnvironmentVariable($envVar, $path)
        return $path
    }

    Write-Error "Cannot find $name"
    exit 1
}

function Expand-ZIPFile($source, $destination) {
    Write-Host "Expanding ZIP file ${source} into ${destination}"

    if (Get-Command "Expand-Archive" -errorAction SilentlyContinue) {
        # PS v5.0+
        Expand-Archive $source $destination -Force
    }
    else {
        if (-Not (Test-Path $destination)) {
            Write-Host "Creating ${destination}"
            New-Item $destination -ItemType Directory
        }

        $application = New-Object -Com Shell.Application

        $zip = $application.NameSpace($source)
        $application.NameSpace($destination).CopyHere($zip.items(), 0x14)
    }
}