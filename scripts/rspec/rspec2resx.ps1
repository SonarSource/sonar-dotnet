<#

.SYNOPSIS
This script allows to convert a directory full of rspec files into a resx file
You should not call it manually, it will be called during the build

.DESCRIPTION

Usage: rspec2resx.ps1
    [cs|vbnet]        The language to synchronize
    <rspecfolder>       The path of the folder containing .rspec files
    <resxfile>          The full path to the resx file to generate

NOTES:
- All operations recreate the projects' resources, do not edit manually.

#>
param (
    [Parameter(Mandatory = $true, HelpMessage = "language: cs or vbnet", Position = 0)]
    [ValidateSet("cs", "vbnet")]
    [string]
    $language,
    [Parameter(Mandatory = $true, HelpMessage = "The path of the folder containing .rspec files.", Position = 1)]
    [string]
    $rspecFolder,
    [Parameter(Mandatory = $true, HelpMessage = "The full path to the resx file to generate.", Position = 2)]
    [string]
    $resxFile
)

Set-StrictMode -version 1.0
$ErrorActionPreference = "Stop"
$RuleTemplateFolder = "${PSScriptRoot}\\rspec-templates"

$resgenPath = "${Env:ProgramFiles(x86)}\\Microsoft SDKs\\Windows\\v10.0A\\bin\\NETFX 4.6.1 Tools\\ResGen.exe"
if (-Not (Test-Path $resgenPath)) {
    throw "You need to install the Windows SDK before using this script."
}

$sonaranalyzerPath = "${PSScriptRoot}\\..\\..\\sonaranalyzer-dotnet"

$categoriesMap = @{
    "BUG" = "Bug";
    "CODE_SMELL" = "Code Smell";
    "VULNERABILITY" = "Vulnerability";
    "SECURITY_HOTSPOT" = "Security Hotspot";
}

$severitiesMap = @{
    "Critical" = "Critical";
    "Major" = "Major";
    "Minor" = "Minor";
    "Info" = "Info";
    "Blocker" = "Blocker";
}

$ruleapiLanguageMap = @{
    "cs" = "c#";
    "vbnet" = "vb.net";
}

$remediationsMap = @{
    "" = "";
    "Constant/Issue" = "Constant/Issue";
}

$helpLanguageMap = @{
    "cs" = "csharp";
    "vbnet" = "vbnet";
}

# Values have to match the ones from Microsoft.CodeAnalysis.LanguageNames
$roslynLanguageMap = @{
    "cs" = "C#";
    "vbnet" = "Visual Basic";
}

# Returns a string array with rule keys for the specified language.
function GetRules() {
    $suffix = $ruleapiLanguageMap.Get_Item($language)

    $htmlFiles = Get-ChildItem "${rspecFolder}\\*" -Include "*.html"
    foreach ($htmlFile in $htmlFiles) {
        if ($htmlFile -Match "(S\d+)_(${suffix}).html") {
            $Matches[1]
        }
    }
}

function CreateStringResources($rules) {
    $suffix = $ruleapiLanguageMap.Get_Item($language)
    
    $sonarWayRules = Get-Content -Raw "${rspecFolder}\\Sonar_way_profile.json" | ConvertFrom-Json

    $resources = New-Object System.Collections.ArrayList
    foreach ($rule in $rules) {
        $json = Get-Content -Raw "${rspecFolder}\\${rule}_${suffix}.json" | ConvertFrom-Json
        $html = Get-Content -Raw "${rspecFolder}\\${rule}_${suffix}.html"

        # take the first paragraph of the HTML file
        if ($html -Match "<p>((.|\n)*?)</p>") {
            # strip HTML tags and new lines
            $description = $Matches[1] -replace '<[^>]*>', ''
            $description = $description -replace '\n|( +)', ' '
            $description = $description -replace '&amp;', '&'
            $description = $description -replace '&lt;', '<'
            $description = $description -replace '&gt;', '>'
            $description = $description -replace '\\!', '!'
            $description = $description -replace '\\}', '}'
        }
        else {
            throw "The downloaded HTML for rule '${rule}' does not contain any paragraphs."
        }

        $severity = $severitiesMap.Get_Item(${json}.defaultSeverity)

        [void]$resources.Add("${rule}_Description=${description}")
        [void]$resources.Add("${rule}_Type=$(${json}.type)")
        [void]$resources.Add("${rule}_Title=$(${json}.title)")
        [void]$resources.Add("${rule}_Category=${severity} $($categoriesMap.Get_Item(${json}.type))")
        [void]$resources.Add("${rule}_IsActivatedByDefault=$(${sonarWayRules}.ruleKeys -Contains ${rule})")
        [void]$resources.Add("${rule}_Severity=${severity}") # TODO see how can we implement lowering the severity for certain rules
        [void]$resources.Add("${rule}_Tags=" + (${json}.tags -Join ","))
        [void]$resources.Add("${rule}_Scope=$(${json}.scope)")

        if (${json}.remediation.func) {
            [void]$resources.Add("${rule}_Remediation=$($remediationsMap.Get_Item(${json}.remediation.func))")
            [void]$resources.Add("${rule}_RemediationCost=$(${json}.remediation.constantCost)") # TODO see if we have remediations other than constantConst and fix them
        }

        # Remove hotspots from the JSON object, we will save this object in a new file that will be
        # used to define Sonar Way on SonarQube that is older than 7.3 and does not support hotspots
        if ($json.type -eq "SECURITY_HOTSPOT") {
            $sonarWayRules.ruleKeys = $sonarWayRules.ruleKeys | ? {$_ -ne $rule}
        }
    }

    # Create a new Sonar Way definition without hotspots to be loaded on SonarQube older than 7.3
    ConvertTo-Json $sonarWayRules | Set-Content "${rspecFolder}\\Sonar_way_profile_no_hotspot.json"

    # improve readability of the generated file
    [void]$resources.Sort()

    [void]$resources.Add("HelpLinkFormat=https://rules.sonarsource.com/$($helpLanguageMap.Get_Item($language))/RSPEC-{0}")
    [void]$resources.Add("RoslynLanguage=$($roslynLanguageMap.Get_Item($language))")

    $rawResourcesPath = "${PSScriptRoot}\\${lang}_strings.restext"

    Set-Content $rawResourcesPath $resources

    # generate resx file
    Invoke-Expression "& `"${resgenPath}`" ${rawResourcesPath} ${resxFile}"
}

### SCRIPT START ###

$rules = GetRules
CreateStringResources $rules

