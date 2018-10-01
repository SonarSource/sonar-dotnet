<#

.SYNOPSIS
This script allows to download metadata for the given language from RSPEC repository.

.DESCRIPTION
This script allows to download metadata for the given language from RSPEC repository:
https://jira.sonarsource.com/browse/RSPEC
If you want to add a rule to both c# and vb.net execute the operation for each language.

Usage: rspec.ps1
    [cs|vbnet]        The language to synchronize
    <rulekey>         The specific rule to synchronize
    <className>       The name to use for the automatically generated classes

NOTES:
- All operations recreate the projects' resources, do not edit manually.

#>
param (
    [Parameter(Mandatory = $true, HelpMessage = "language: cs or vbnet", Position = 0)]
    [ValidateSet("cs", "vbnet")]
    [string]
    $language,
    [Parameter(HelpMessage = "The key of the rule to add/update, e.g. S1234. If omitted will update all existing rules.", Position = 1)]
    [ValidatePattern("^S[0-9]+$")]
    [string]
    $ruleKey,
    [Parameter(HelpMessage = "The name of the rule class.", Position = 2)]
    [string]
    $className
)

Set-StrictMode -version 1.0
$ErrorActionPreference = "Stop"
$RuleTemplateFolder = "${PSScriptRoot}\\rspec-templates"

# Update the following variable when a new version of rule-api has to be used.
$rule_api_version = "1.22.0.1199"
$rule_api_error = "Download Rule-api from " + `
    "'https://repox.sonarsource.com/sonarsource-private-releases/com/sonarsource/rule-api/rule-api/${rule_api_version}' " +`
    "to a folder and set the %rule_api_path% environment variable with the full path of that folder. For example 'c:\\work\\tools'."
if (-Not $Env:rule_api_path) {
    throw $rule_api_error
}
$rule_api_jar = "${Env:rule_api_path}\\rule-api-${rule_api_version}.jar"
if (-Not (Test-Path $rule_api_jar)) {
    throw $rule_api_error
}

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

$remediationsMap = @{
    "" = "";
    "Constant/Issue" = "Constant/Issue";
}

$projectsMap = @{
    "cs" = "SonarAnalyzer.CSharp";
    "vbnet" = "SonarAnalyzer.VisualBasic";
}

$ruleapiLanguageMap = @{
    "cs" = "c#";
    "vbnet" = "vb.net";
}

$resourceLanguageMap = @{
    "cs" = "cs";
    "vbnet" = "vb";
}

$helpLanguageMap = @{
    "cs" = "csharp";
    "vbnet" = "vbnet";
}

$sonarpediaMap = @{
    "cs" = ".\sonaranalyzer-dotnet\src\SonarAnalyzer.CSharp";
    "vbnet" = ".\sonaranalyzer-dotnet\src\SonarAnalyzer.VisualBasic";
}

# Values have to match the ones from Microsoft.CodeAnalysis.LanguageNames
$roslynLanguageMap = @{
    "cs" = "C#";
    "vbnet" = "Visual Basic";
}

# Returns the path to the folder where the RSPEC html and json files for the specified language will be downloaded.
function GetRspecDownloadPath($lang) {
    $rspecFolder = "${sonaranalyzerPath}\\rspec\\${lang}"
    if (-Not (Test-Path $rspecFolder)) {
        New-Item $rspecFolder | Out-Null
    }

    return $rspecFolder
}

# Returns a string array with rule keys for the specified language.
function GetRules($lang) {
    $suffix = $ruleapiLanguageMap.Get_Item($lang)

    $htmlFiles = Get-ChildItem "$(GetRspecDownloadPath $lang)\\*" -Include "*.html"
    foreach ($htmlFile in $htmlFiles) {
        if ($htmlFile -Match "(S\d+)_(${suffix}).html") {
            $Matches[1]
        }
    }
}

# Copies the downloaded RSPEC html files for all rules in the specified language
# to 'SonarAnalyzer.Utilities\Rules.Description'. If a rule is present in the otherLanguageRules
# collection, a language suffix will be added to the target file name, so that the VB.NET and
# C# files could have different html resources.
function CopyResources($lang, $rules, $otherLanguageRules) {
    $descriptionsFolder = "${sonaranalyzerPath}\\src\\SonarAnalyzer.Utilities\\Rules.Description"
    $rspecFolder = GetRspecDownloadPath $lang

    $source_suffix = "_" + $ruleapiLanguageMap.Get_Item($lang)

    foreach ($rule in $rules) {
        $suffix = ""
        if ($otherLanguageRules -contains $rule) {
            $suffix = "_$($resourceLanguageMap.Get_Item($lang))"

            $sharedLanguageDescription = "${descriptionsFolder}\\${rule}.html"
            if (Test-Path $sharedLanguageDescription) {
                Remove-Item $sharedLanguageDescription -Force
            }
        }

        Copy-Item "${rspecFolder}\\${rule}${source_suffix}.html" "${descriptionsFolder}\\${rule}${suffix}.html"
    }
}

function UpdateTestEntry($rule) {
    $ruleType = $rule.type
    if (!$ruleType) {
        return
    }

    $fileToEdit = if ($language -eq "cs") {"CsRuleTypeMapping"} else {"VbRuleTypeMapping"}
    $ruleTypeTestCase = "${sonaranalyzerPath}\\tests\\SonarAnalyzer.UnitTest\\PackagingTests\\$fileToEdit.cs"
    $ruleId = $ruleKey.Substring(1)
    (Get-Content "${ruleTypeTestCase}") -replace "//\[`"$ruleId`"\]", "[`"$ruleId`"] = `"$ruleType`"" |
        Set-Content "${ruleTypeTestCase}"
}

function CreateStringResources($lang, $rules) {
    $rspecFolder = GetRspecDownloadPath $lang
    $suffix = $ruleapiLanguageMap.Get_Item($lang)

    $sonarWayRules = Get-Content -Raw "${rspecFolder}\\Sonar_way_profile.json" | ConvertFrom-Json

    $newRuleData = @{}
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

        if ($rule -eq $ruleKey) {
            $newRuleData = $json
        }
    }

    # improve readability of the generated file
    [void]$resources.Sort()

    [void]$resources.Add("HelpLinkFormat=https://rules.sonarsource.com/$($helpLanguageMap.Get_Item($lang))/RSPEC-{0}")
    [void]$resources.Add("RoslynLanguage=$($roslynLanguageMap.Get_Item($lang))")

    $rawResourcesPath = "${PSScriptRoot}\\${lang}_strings.restext"
    $resourcesPath = "${sonaranalyzerPath}\\src\\$($projectsMap.Get_Item($lang))\\RspecStrings.resx"

    Set-Content $rawResourcesPath $resources

    # generate resx file
    Invoke-Expression "& `"${resgenPath}`" ${rawResourcesPath} ${resourcesPath}"

    return $newRuleData
}

function GenerateCsRuleClasses() {
    $csharpRulesFolder         = "${sonaranalyzerPath}\\src\\SonarAnalyzer.CSharp\\Rules"
    $csharpRuleTestsFolder     = "${sonaranalyzerPath}\\tests\\SonarAnalyzer.UnitTest\\Rules"
    $csharpRuleTestCasesFolder = "${sonaranalyzerPath}\\tests\\SonarAnalyzer.UnitTest\\TestCases"

    $filesMap = @{
        "CSharpRuleTemplate.cs"     = "${csharpRulesFolder}\\${className}.cs"
        "CSharpTestTemplate.cs"     = "${csharpRuleTestsFolder}\\${className}Test.cs"
        "CSharpTestCaseTemplate.cs" = "${csharpRuleTestCasesFolder}\\${className}.cs"
    }
    WriteClasses -filesMap $filesMap -classNameToUse $className
}

function GenerateVbRuleClasses($language) {
    $vbRulesFolder         = "${sonaranalyzerPath}\\src\\SonarAnalyzer.VisualBasic\\Rules"
    $csRulesFolder         = "${sonaranalyzerPath}\\src\\SonarAnalyzer.CSharp\\Rules"
    $commonRulesFolder     = "${sonaranalyzerPath}\\src\\SonarAnalyzer.Common\\Rules"
    $ruleTestsFolder       = "${sonaranalyzerPath}\\tests\\SonarAnalyzer.UnitTest\\Rules"
    $vbRuleTestCasesFolder = "${sonaranalyzerPath}\\tests\\SonarAnalyzer.UnitTest\\TestCases"

    $csClassName = FindCsName -csRulesPath $csRulesFolder

    $filesMap = @{}
    if ($csClassName) {
        $className = $csClassName
        AppendVbTestCase -ruleTestsFolder $ruleTestsFolder
    }
    else {
        $filesMap["VbNetTestTemplate.cs"] = "${ruleTestsFolder}\\${className}Test.cs"
    }

    $filesMap["CommonBaseClassTemplate.cs"] = "${commonRulesFolder}\\${className}Base.cs"
    $filesMap["VbNetRuleTemplate.cs"] = "${vbRulesFolder}\\${className}.cs"
    $filesMap["VbNetTestCaseTemplate.vb"] = "${vbRuleTestCasesFolder}\\${className}.vb"

    WriteClasses -filesMap $filesMap
}

function AppendVbTestCase($ruleTestsFolder) {
    $existingClassText = Get-Content -Path "${ruleTestsFolder}\\${csClassName}Test.cs" -Raw
    $snippetText = Get-Content -Path "${RuleTemplateFolder}\\VbNetTestSnippet.cs" -Raw

    $token = "        }"
    $idx = $existingClassText.LastIndexOf($token)

    $newText = ""
    if ($idx -gt -1) {
        $newText = $existingClassText.Remove($idx, $token.Length).Insert($idx, "${token}`r`n`r`n${snippetText}")
    }
    else {
        $newText = "${existingClassText}`r`n${snippetText}"
    }

    $replaced = ReplaceTokens -text $newText

    Set-Content -Path "${ruleTestsFolder}\\${csClassName}Test.cs" `
                -Value $replaced
}

function FindCsName($csRulesPath) {
    $csRuleFiles = Get-ChildItem -Path $csRulesPath -Filter "*.cs"

    $quotedRuleKey = "`"$ruleKey`";"
    foreach ($csRuleFile in $csRuleFiles) {
        $content = $null = Get-Content -Path $csRuleFile.FullName -Raw
        if ($content -match $quotedRuleKey) {
            return $csRuleFile.BaseName
        }
    }
    return $null
}

function WriteClasses($filesMap) {
    $filesMap.GetEnumerator() | ForEach-Object {
        $fileContent = Get-Content "${RuleTemplateFolder}\\$($_.Key)" -Raw
        $replaced = ReplaceTokens -text $fileContent

        Set-Content -Path $_.Value -Value $replaced
    }
}

function ReplaceTokens($text) {
    return $text `
        -replace '\$DiagnosticClassName\$', $className `
        -replace '\$DiagnosticId\$', $ruleKey
}


### SCRIPT START ###

$sonarpediaFolder = $sonarpediaMap.Get_Item($language)
Write-Host "Will change directory to $sonarpediaFolder to run rule-api"
pushd $sonarpediaFolder

if ($ruleKey) {
    java -jar $rule_api_jar generate -rule $ruleKey
}
else {
    java -jar $rule_api_jar update
}

Write-Host "Ran rule-api, will move back to root"
popd

$csRules = GetRules "cs"
$vbRules = GetRules "vbnet"

CopyResources "cs" $csRules $vbRules
CopyResources "vbnet" $vbRules $csRules
$csRuleData = CreateStringResources "cs" $csRules
$vbRuleData = CreateStringResources "vbnet" $vbRules

if ($className -And $ruleKey) {
    if ($language -eq "cs") {
       GenerateCsRuleClasses
       UpdateTestEntry $csRuleData
    }
    elseif ($language -eq "vbnet") {
       GenerateVbRuleClasses
       UpdateTestEntry $vbRuleData
    }

    $vsTempFolder = "${sonaranalyzerPath}\\.vs"
    if (Test-Path $vsTempFolder) {
        Remove-Item -Recurse -Force $vsTempFolder
    }
}
