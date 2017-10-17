# Do not set this to 2.0 otherwise testing whether a property exists in the json file will cause the script to
# crash with:
#   The property 'issues' cannot be found on this object. Verify that the property exists.
#   System.Management.Automation.PropertyNotFoundException: The property 'issues' cannot be found on this object.
#   Verify that the property exists.
Set-StrictMode -version 1.0
$ErrorActionPreference = "Stop"

$thrd = [Threading.Thread]::CurrentThread
$thrd.CurrentCulture = [Globalization.CultureInfo]::InvariantCulture
$thrd.CurrentUICulture = $thrd.CurrentCulture

[void][System.Reflection.Assembly]::LoadWithPartialName("System.Web.Extensions")
$jsonserial= New-Object -TypeName System.Web.Script.Serialization.JavaScriptSerializer
$jsonserial.MaxJsonLength = 100000000

function Restore-UriDeclaration($files, $pathPrefix) {
	$files | Foreach-Object {
        if ($_.uri) {
            # Remove the URI prefix
            $_.uri = $_.uri -replace "file:///", ""
            # Remove the common absolute path prefix
            $_.uri = ([System.IO.FileInfo]$_.uri).FullName
            $_.uri = $_.uri -replace $pathPrefix, ""
            $_.uri = $_.uri -replace "/", "\"
        }
    }
}

function Get-Issue($entry) {
    $issue = New-Object –Type System.Object
    $issue | Add-Member –Type NoteProperty –Name id –Value $entry.ruleId
    if ($entry.shortMessage) {
        $issue | Add-Member –Type NoteProperty –Name message –Value $entry.shortMessage
    }
    elseif ($entry.fullMessage) {
        $issue | Add-Member –Type NoteProperty –Name message –Value $entry.fullMessage
    }
        $issue | Add-Member –Type NoteProperty –Name location –Value $entry.locations.analysisTarget

    $issue
    return
}

function Get-IssueV3($entry) {
    $issue = New-Object –Type System.Object
    $issue | Add-Member –Type NoteProperty –Name id –Value $entry.ruleId
    $issue | Add-Member –Type NoteProperty –Name message –Value $entry.message
    if ($entry.relatedLocations.physicalLocation) {
        $issue | Add-Member –Type NoteProperty –Name location –Value (@($entry.locations.resultFile) + `
            $entry.relatedLocations.physicalLocation)
    }
    else {
        $issue | Add-Member –Type NoteProperty –Name location –Value $entry.locations.resultFile
    }

  $issue
  return
}

function New-IssueReports([string]$sarifReportPath) {
    # Load the JSON, working around CovertFrom-Json max size limitation
    # See http://stackoverflow.com/questions/16854057/convertfrom-json-max-length
    $contents = Get-Content $sarifReportPath -Raw
    $json = $jsonserial.DeserializeObject($contents)

    $pathPrefix = ([System.IO.DirectoryInfo]$pwd.Path).FullName + "\"
    $pathPrefix = $pathPrefix -Replace '\\', '\\' # escape path

    # Is there any issue?
    if ($json.issues) { # sarif v0.1
        $allIssues = $json.issues

        # Adjust positions to previous SARIF format
        $allIssues.locations.analysisTarget.region | Foreach-Object {
            if ($_.startLine -ne $null) {
                $_.startLine = $_.startLine + 1
            }
            if ($_.startColumn -ne $null) {
                $_.startColumn = $_.startColumn + 1
            }
            if ($_.endLine -ne $null) {
                $_.endLine = $_.endLine + 1
            }
            if ($_.endColumn -ne $null) {
                $_.endColumn = $_.endColumn + 1
            }
        }

        Restore-UriDeclaration $allIssues.locations.analysisTarget $pathPrefix
        $allIssues = $allIssues | Foreach-Object { Get-Issue($_) }
    }
    elseif ($json.runLogs) { # sarif v0.4
        $allIssues = $json.runLogs | Foreach-Object { $_.results }
        Restore-UriDeclaration $allIssues.locations.analysisTarget $pathPrefix
        $allIssues = $allIssues | Foreach-Object { Get-Issue($_) }
    }
    elseif ($json.runs) { # sarif v1.0.0

        $allIssues = $json.runs | Foreach-Object { $_.results }
        Restore-UriDeclaration $allIssues.locations.resultFile $pathPrefix
	    Restore-UriDeclaration $allIssues.relatedLocations.physicalLocation $pathPrefix
        $allIssues = $allIssues | Foreach-Object { Get-IssueV3($_) }
    }

    # Change spaces to %20
    $allIssues.location | Foreach-Object {
	    if ($_.uri) {
	        $_.uri = $_.uri.replace(' ', '%20')
	    }
    }

    # Filter, Sort & Group issues to get a stable SARIF report
    # AD0001's stack traces in the message are unstable
    # CS???? messages are not of interest
    $issuesByRule = $allIssues |
        Where-Object { $_.id -match '^S[0-9]+$' } |                  # Keep SonarAnalyzer rules only
        Sort-Object @{Expression={$_.location.uri}},                 # Regroup same file issues
                    @{Expression={$_.location.region.startLine}},    # Sort by source position
                    @{Expression={$_.location.region.startColumn}},  # .. idem
                    @{Expression={$_.location.region.endLine}},      # .. idem
                    @{Expression={$_.location.region.endColumn}},    # .. idem
                    @{Expression={$_.message}} |                     # .. and finally by message
        Group-Object @{Expression={$_.id}}                           # Group issues generated by the same rule

    $file = [System.IO.FileInfo]$sarifReportPath

    $project = ([System.IO.FileInfo]$file.DirectoryName).Name

    $issuesByRule | Foreach-Object {
        $object = New-Object –Type System.Object
        $object | Add-Member –Type NoteProperty –Name issues –Value $_.Group

        $path = Join-Path (Join-Path 'actual' $project) ($file.BaseName + '-' + $_.Name + $file.Extension)
        $lines =
            (
                (ConvertTo-Json $object -Depth 42 |
                    ForEach-Object { [System.Text.RegularExpressions.Regex]::Unescape($_) }  # Unescape powershell to json automatic escape
                ) -split "`r`n"										            	         # Convert JSON to String and split lines
            ) | Foreach-Object { $_.TrimStart() }                                            # Remove leading spaces
        Set-Content $path $lines
    }
}

# Normalize & overwrite all *.json SARIF files found under the "actual" folder
Get-ChildItem output -filter *.json -recurse | Foreach-Object { New-IssueReports($_.FullName) }