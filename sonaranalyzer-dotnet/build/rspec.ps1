# Usage:
# rspec.ps1 cs|vbnet
#
# Will download only the updated RSPEC files for the corresponding language
#
# or
# rspec.ps1 cs|vbnet <rule-key>
#
# Add will download the specified rule json and html files. If you want to add a
# rule to both c# and vb.net execute the operation for each language.
#
# NOTES:
# - All operations recreate the projects' resources, do not edit manually.
param (
    [Parameter(Mandatory = $true, HelpMessage = "language: cs or vbnet", Position = 0)]
    [ValidateSet("cs", "vbnet")]
    [string]
    $language,
    [Parameter(HelpMessage = "The key of the rule to add/update, e.g. S1234. If omitted will update all existing rules.", Position = 1)]
    [string]
    $ruleKey,
    [Parameter(HelpMessage = "The name of the rule class.", Position = 2)]
    [string]
    $className
)

if ((-Not $env:rule_api_path) -Or (-Not (Test-Path $env:rule_api_path))) {
    throw "Download the latest version of rule-api jar from repox and set the %rule_api_path% environment variable with the full path of the jar."
}

$resgenPath = "${Env:ProgramFiles(x86)}\\Microsoft SDKs\\Windows\\v10.0A\\bin\\NETFX 4.6.1 Tools\\ResGen.exe"
if (-Not (Test-Path $resgenPath)) {
    throw "You need to install the Windows SDK before using this script."
}

$categoriesMap =
@{
    "BUG" = "Sonar Bug";
    "CODE_SMELL" = "Sonar Code Smell";
    "VULNERABILITY" = "Sonar Vulnerability";
}

$severitiesMap =
@{
    "Critical" = "Critical";
    "Major" = "Major";
    "Minor" = "Minor";
    "Info" = "Info";
    "Blocker" = "Blocker";
}

$remediationsMap =
@{
    "" = "";
    "Constant/Issue" = "Constant/Issue";
}

$projectsMap = 
@{
    "cs" = "SonarAnalyzer.CSharp";
    "vbnet" = "SonarAnalyzer.VisualBasic";
}

$ruleapiLanguageMap = 
@{
    "cs" = "c#";
    "vbnet" = "vb.net";
}

$resourceLanguageMap = 
@{
    "cs" = "cs";
    "vbnet" = "vb";
}

# Returns the path to the folder where the RSPEC html and json files for the specified language will be downloaded.
function GetRspecDownloadPath() {
    param ($lang)

    $rspecFolder = "${PSScriptRoot}\\..\\rspec\\${lang}"
    if (-Not (Test-Path $rspecFolder)) {
        New-Item $rspecFolder | Out-Null
    }

    return $rspecFolder
}

# Returns a string array with rule keys for the specified language.
function GetRules() {
    param ($lang)

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
function CopyResources() {
    param ($lang, $rules, $otherLanguageRules)

    $descriptionsFolder = "${PSScriptRoot}\\..\\src\\SonarAnalyzer.Utilities\\Rules.Description"
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

function UpdateTestEntry()
{
    param ($rule)

    $ruleType = $rule.type
    if (!$ruleType)
    {
        return
    }

    $csharpRuleTypeTestCase = "${PSScriptRoot}\\..\\src\\Tests\\SonarAnalyzer.UnitTest\\PackagingTests\\RuleTypeTests.cs"
    $ruleId = $ruleKey.Substring(1)
    (Get-Content "${csharpRuleTypeTestCase}") -replace "//\[`"$ruleId`"\]", "[`"$ruleId`"] = `"$ruleType`"" |
        Set-Content "${csharpRuleTypeTestCase}"
}

function CreateStringResources() {
    param ($lang, $rules)

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
        }
        else {
            throw "The downloaded HTML for rule '${rule}' does not contain any paragraphs."
        }

        [void]$resources.Add("${rule}_Description=${description}")
        [void]$resources.Add("${rule}_Type=$(${json}.type)")
        [void]$resources.Add("${rule}_Title=$(${json}.title)")
        [void]$resources.Add("${rule}_Category=$($categoriesMap.Get_Item(${json}.type))")
        [void]$resources.Add("${rule}_IsActivatedByDefault=$(${sonarWayRules}.ruleKeys -Contains ${rule})")
        [void]$resources.Add("${rule}_Severity=$($severitiesMap.Get_Item(${json}.defaultSeverity))") # TODO see how can we implement lowering the severity for certain rules
        [void]$resources.Add("${rule}_Tags=" + (${json}.tags -Join ","))

        if (${json}.remediation.func) {
            [void]$resources.Add("${rule}_Remediation=$($remediationsMap.Get_Item(${json}.remediation.func))")
            [void]$resources.Add("${rule}_RemediationCost=$(${json}.remediation.constantCost)") # TODO see if we have remediations other than constantConst and fix them
        }
        
        if ($rule -eq $ruleKey)
        {
            $newRuleData = $json
    }
    }

    # improve readability of the generated file
    [void]$resources.Sort()

    $rawResourcesPath = "${PSScriptRoot}\\${lang}_strings.restext"
    $resourcesPath = "${PSScriptRoot}\\..\\src\\$($projectsMap.Get_Item($lang))\\RspecStrings.resx"

    Set-Content $rawResourcesPath $resources

    # generate resx file
    Invoke-Expression "& `"${resgenPath}`" ${rawResourcesPath} ${resourcesPath}"

    return $newRuleData
}

function GenerateRuleClasses() {
    $ruleTemplateFolder = "${PSScriptRoot}\\rspec-templates"
    $csharpRulesFolder = "${PSScriptRoot}\\..\\src\\SonarAnalyzer.CSharp\\Rules"
    $csharpRuleTestsFolder = "${PSScriptRoot}\\..\\src\\Tests\\SonarAnalyzer.UnitTest\\Rules"
    $csharpRuleTestCasesFolder = "${PSScriptRoot}\\..\\src\\Tests\\SonarAnalyzer.UnitTest\\TestCases"

    (Get-Content "${ruleTemplateFolder}\\CSharpRuleTemplate.cs") -replace '\$DiagnosticClassName\$', $className -replace '\$DiagnosticId\$', $ruleKey |
        Set-Content "${csharpRulesFolder}\\${className}.cs"

    (Get-Content "${ruleTemplateFolder}\\CSharpTestTemplate.cs") -replace '\$DiagnosticClassName\$', $className |
        Set-Content "${csharpRuleTestsFolder}\\${className}Test.cs"

    (Get-Content "${ruleTemplateFolder}\\CSharpTestCaseTemplate.cs") -replace '\$DiagnosticClassName\$', $className |
        Set-Content "${csharpRuleTestCasesFolder}\\${className}.cs"
}

if ($ruleKey) {
    java -jar $env:rule_api_path generate -directory $(GetRspecDownloadPath $language) -language $($ruleapiLanguageMap.Get_Item($language)) -rule $ruleKey
}
else {
    java -jar $env:rule_api_path update -directory $(GetRspecDownloadPath $language) -language $($ruleapiLanguageMap.Get_Item($language))
}

$csRules = GetRules "cs"
$vbRules = GetRules "vbnet"

CopyResources "cs" $csRules $vbRules
CopyResources "vbnet" $vbRules $csRules
$csRuleData = CreateStringResources "cs" $csRules
$vbRuleData = CreateStringResources "vbnet" $vbRules

if ($className -And $ruleKey) {
    GenerateRuleClasses

    if ($language -eq "cs")
    {
       UpdateTestEntry $csRuleData
    }
}