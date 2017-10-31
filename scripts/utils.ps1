Add-Type -AssemblyName "System.IO.Compression.FileSystem"

# Original: http://jameskovacs.com/2010/02/25/the-exec-problem
function Exec ([scriptblock]$command, [string]$errorMessage = "ERROR: Command '${command}' FAILED.") {
    Write-Debug "Invoking command:${command}"

    $output = ""
    & $command | Tee-Object -Variable output
    if ((-not $?) -or ($lastexitcode -ne 0)) {
        throw $errorMessage
    }

    return $output
}

function Test-ExitCode([string]$errorMessage = "ERROR: Command FAILED.") {
    if ((-not $?) -or ($lastexitcode -ne 0)) {
        throw $errorMessage
    }
}

# Sets the current folder and executes the given script.
# When the script finishes sets the original current folder.
function Invoke-InLocation([string]$path, [scriptblock]$command) {
    try {
        Write-Debug "Changing current directory to: '${path}'"
        Push-Location $path

        & $command
    }
    finally {
        Write-Debug "Changing current directory back to previous one"
        Pop-Location
    }
}

function ConvertTo-UnixLineEndings([string]$fileName) {
    Get-ChildItem $fileName | ForEach-Object {
        $currentFile = $_
        Write-Debug "Changing line ending for '${currentFile}'"
        $content = [IO.File]::ReadAllText($currentFile) -Replace "`r`n?", "`n"
        $utf8 = New-Object System.Text.UTF8Encoding $false
        [IO.File]::WriteAllText($currentFile, $content, $utf8)
    }
}

function Write-Header([string]$text) {
    Write-Host
    Write-Host "================================================"
    Write-Host $text
    Write-Host "================================================"
}

function Get-ExecutablePath([string]$name, [string]$directory, [string]$envVar) {
    Write-Debug "Trying to find '${name}' using '${envVar}' environment variable"
    $path = [environment]::GetEnvironmentVariable($envVar, "Process")

    try {
        if (!$path) {
            Write-Debug "Environment variable not found"

            if (!$directory) {
                Write-Debug "Trying to find path using 'where.exe'"
                $path = Exec { & where.exe $name } | Select-Object -First 1
            }
            else {
                Write-Debug "Trying to find path using 'where.exe' in '${directory}'"
                $path = Exec { & where.exe /R $directory $name } | Select-Object -First 1
            }
        }
    }
    catch {
        throw "Failed to locate executable '${name}' on the path"
    }

    if (Test-Path $path) {
        Write-Debug "Found '${name}' at '${path}'"
        [environment]::SetEnvironmentVariable($envVar, $path)
        return $path
    }

    throw "'${name}' located at '${path}' doesn't exist"
}

function Expand-ZIPFile($source, $destination) {
    Write-Host "Unzipping '${source}' into '${destination}'"

    if (Get-Command "Expand-Archive" -errorAction SilentlyContinue) {
        # PS v5.0+
        Write-Debug "Unzipping using 'Expand-Archive'"
        Expand-Archive $source $destination -Force
    }
    else {
        if (-Not (Test-Path $destination)) {
            Write-Debug "Creating folder '${destination}'"
            New-Item $destination -ItemType Directory
        }

        Write-Debug "Unzipping using 'Shell.Application'"
        $application = New-Object -Com Shell.Application

        $zip = $application.NameSpace($source)
        $application.NameSpace($destination).CopyHere($zip.items(), 0x14)
    }
}

function Test-Debug() {
    return $DebugPreference -ne "SilentlyContinue"
}
