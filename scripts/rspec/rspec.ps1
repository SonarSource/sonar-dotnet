<#

.SYNOPSIS
This script allows to download metadata for the given language from RSPEC repository.

.DESCRIPTION
This script allows to download metadata for the given language from RSPEC repository.
If you want to add a rule to both c# and vb.net execute the operation for each language.

Usage: rspec.ps1
    [cs|vbnet]        The language to synchronize
    <rulekey>         The specific rule to synchronize
    <className>       The name to use for the automatically generated classes
#>
param (
    [Parameter(Mandatory = $true, HelpMessage = "language: cs or vbnet", Position = 0)]
    [ValidateSet("cs", "vbnet")]
    [string]
    $Language,
    [Parameter(HelpMessage = "The key of the rule to add/update, e.g. S1234. If omitted will update all existing rules.", Position = 1)]
    [ValidatePattern("^S[0-9]+$")]
    [string]
    $RuleKey,
    [Parameter(HelpMessage = "The name of the rule class.", Position = 2)]
    [string]
    $ClassName,
    [Parameter(HelpMessage = "The branch in the rspec github repo.", Position = 3)]
    [string]
    $RspecBranch
)

Set-StrictMode -version 1.0
$ErrorActionPreference = "Stop"
$RuleTemplateFolder = "${PSScriptRoot}\\rspec-templates"
# Based on RuleApiCache.computeDefaultCachePath
# https://github.com/SonarSource/sonar-rule-api/blob/2323b7313c76e7adc2b7df037e96510b90660292/src/main/java/com/sonarsource/ruleapi/utilities/RuleApiCache.java#L20-L25
$RspecRepositoryPath = "${Env:USERPROFILE}\\.sonar\\rule-api\\rspec"
if (! [string]::IsNullOrEmpty($Env:SONAR_USER_HOME))
{
    $RspecRepositoryPath = "${Env:SONAR_USER_HOME}\\rule-api\\rspec"
}

. ${PSScriptRoot}\CopyTestCasesFromRspec.ps1

$RuleApiError = "Could not find the Rule API Jar locally. Please download the latest rule-api from " + `
    "'https://repox.jfrog.io/repox/sonarsource-private-releases/com/sonarsource/rule-api/rule-api/' " +`
    "to a folder and set the %rule_api_path% environment variable with the full path of that folder. For example 'c:\\work\\tools'."
if (-Not $Env:rule_api_path) {
    throw $RuleApiError
}
$RuleApiJars = Get-ChildItem "${Env:rule_api_path}\\rule-api-*.jar" | Sort -desc
if ($RuleApiJars.Length -eq 0) {
    throw $RuleApiError
}
$RuleApiJar = $RuleApiJars[0].FullName
if (-Not (Test-Path $RuleApiJar)) {
    throw $RuleApiError
}

$AnalyzersPath = "${PSScriptRoot}\\..\\..\\analyzers"
$RulesFolderCommon = "${AnalyzersPath}\\src\\SonarAnalyzer.Core\\Rules"
$RulesFolderCS     = "${AnalyzersPath}\\src\\SonarAnalyzer.CSharp\\Rules"
$RulesFolderVB     = "${AnalyzersPath}\\src\\SonarAnalyzer.VisualBasic\\Rules"
$RulesFolderTests  = "${AnalyzersPath}\\tests\\SonarAnalyzer.Test\\Rules"
$TestCasesFolder   = "${AnalyzersPath}\\tests\\SonarAnalyzer.Test\\TestCases"

$SonarpediaMap = @{
    "cs" = ".\analyzers\src\SonarAnalyzer.CSharp";
    "vbnet" = ".\analyzers\src\SonarAnalyzer.VisualBasic";
}

function UpdateRuleTypeMapping() {
    $Rule = Get-Content -Raw "${AnalyzersPath}\\rspec\\${Language}\\${RuleKey}.json" | ConvertFrom-Json
    $RuleType = $Rule.Type
    if (!$RuleType) {
        return
    }

    $FileToEdit = if ($Language -eq "cs") {"RuleTypeMappingCS"} else {"RuleTypeMappingVB"}
    $TestFile = "${AnalyzersPath}\\tests\\SonarAnalyzer.TestFramework\\Packaging\\$fileToEdit.cs"
    (Get-Content "${TestFile}") -replace "//\s*\[`"$ruleKey`"\]", "[`"$ruleKey`"] = `"$ruleType`"" | Set-Content "${TestFile}" -Encoding UTF8
}

function GenerateRuleClassesCS() {
    $FilesMap = @{
        "Rule.CS.cs"     = "${RulesFolderCS}\\${className}.cs"
        "Test.CS.cs"     = "${RulesFolderTests}\\${className}Test.cs"
    }

    if (-Not (Test-Path -Path "${TestCasesFolder}\\${className}.cs" -PathType Leaf))
    {
        $FilesMap["TestCase.CS.cs"] = "${TestCasesFolder}\\${className}.cs"
    }

    WriteClasses $FilesMap
}

function GenerateRuleClassesVB() {
    $ExistingClassName = FindClassName $RulesFolderCS

    $FilesMap = @{}
    if ($ExistingClassName) {
        $ClassName = $ExistingClassName
        AppendTestCaseVB
    }
    else {
        $FilesMap["Test.VB.cs"] = "${RulesFolderTests}\\${ClassName}Test.cs"
    }

    $FilesMap["Rule.VB.cs"] = "${RulesFolderVB}\\${ClassName}.cs"

    if (-Not (Test-Path -Path "${TestCasesFolder}\\${ClassName}.vb" -PathType Leaf))
    {
        $FilesMap["TestCase.VB.vb"] = "${TestCasesFolder}\\${ClassName}.vb"
    }

    WriteClasses $FilesMap
}

function GenerateBaseClassIfSecondLanguage()
{
    $ClassNameCS = FindClassName $RulesFolderCS
    $ClassNameVB = FindClassName $RulesFolderVB

    if ($ClassNameCS -And $ClassNameVB) {
        $ContentCS = Get-Content -Path "${RulesFolderCS}\\${ClassNameCS}.cs" -Raw
        $ContentVB = Get-Content -Path "${RulesFolderVB}\\${ClassNameVB}.cs" -Raw

        $ContentCS = $ContentCS.Replace("public sealed class ${ClassNameCS} : SonarDiagnosticAnalyzer", `
                                        "public sealed class ${ClassNameCS} : ${ClassName}Base<SyntaxKind>")
        $ContentVB = $ContentVB.Replace("public sealed class ${ClassNameVB} : SonarDiagnosticAnalyzer", `
                                        "public sealed class ${ClassNameVB} : ${ClassName}Base<SyntaxKind>")

        $Line = "    private const string DiagnosticId = ""${ruleKey}"";`n"
        $ContentCS = $ContentCS.Replace($Line, "")
        $ContentVB = $ContentVB.Replace($Line, "")

        $Line = "    private const string MessageFormat = ""FIXME"";`n"
        $ContentCS = $ContentCS.Replace($Line, "")
        $ContentVB = $ContentVB.Replace($Line, "")

        $Line = "    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);`n"
        $ContentCS = $ContentCS.Replace($Line, "")
        $ContentVB = $ContentVB.Replace($Line, "")

        $ContentCS = $ContentCS.Replace("`n`n`n", "`n`n").Replace("`n`n`n", "`n`n")
        $ContentVB = $ContentVB.Replace("`n`n`n", "`n`n").Replace("`n`n`n", "`n`n")

        $Line = "public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);"
        $ContentCS = $ContentCS.Replace($Line, "protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;")
        $ContentVB = $ContentVB.Replace($Line, "protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;")

        Set-Content -NoNewline -Path "${RulesFolderCS}\\${ClassNameCS}.cs" -Value $ContentCS -Encoding UTF8
        Set-Content -NoNewline -Path "${RulesFolderVB}\\${ClassNameVB}.cs" -Value $ContentVB -Encoding UTF8

        $FilesMap = @{}
        $FilesMap["Rule.Base.cs"] = "${RulesFolderCommon}\\${ClassName}Base.cs"
        WriteClasses $FilesMap
    }
}

function AppendTestCaseVB() {
    $UsingTokenCS = "using CS = SonarAnalyzer.Rules.CSharp;`n"
    $UsingTokenVB = "using VB = SonarAnalyzer.Rules.VisualBasic;`n"
    $NamespaceToken = "namespace SonarAnalyzer.Test.Rules"
    $OldEndToken = "    }" # For files using old namespaces
    $NewEndToken = "}"     # For files using file scoped namespaces
    $MethodVB = Get-Content -Path "${RuleTemplateFolder}\\TestMethod.VB.cs" -Raw
    $Text = Get-Content -Path "${RulesFolderTests}\\${ClassName}Test.cs" -Raw

    $Text = $Text.Replace("using SonarAnalyzer.Rules.CSharp;`n", "")
    $Text = $Text.Replace($NamespaceToken, "${UsingTokenCS}${UsingTokenVB}`n${NamespaceToken}")
    $Text = $Text.Replace("new ${ClassName}", "new CS.${ClassName}")
    $Text = $Text.Replace("<${ClassName}>", "<CS.${ClassName}>")
    $Text = $Text.Replace("builder =", "builderCS =")
    $Text = $Text.Replace("builder.", "builderCS.")

    $EndToken = if ($Text.Contains("${namespaceToken};")) {$NewEndToken} else {$OldEndToken}
    $Index = $Text.LastIndexOf($EndToken)
    if ($Index -gt -1) {
        $Text = $Text.Insert($Index, "`n${MethodVB}")
    }
    else {
        $Text = "${Text}`n${MethodVB}"
    }

    $Text = ReplacePlaceHolders $Text
    Set-Content -NoNewline -Path "${RulesFolderTests}\\${ClassName}Test.cs" -Value $Text -Encoding UTF8
}

function FindClassName($RulesPath) {
    $Files = Get-ChildItem -Path $RulesPath -Filter "*.cs"

    $QuotedRuleKey = "`"$RuleKey`";"
    foreach ($File in $Files) {
        $Content = $null = Get-Content -Path $File.FullName -Raw
        if ($Content -match $QuotedRuleKey) {
            return $File.BaseName
        }
    }
    return $null
}

function WriteClasses($FilesMap) {
    $FilesMap.GetEnumerator() | ForEach-Object {
        $Content = Get-Content "${RuleTemplateFolder}\\$($_.Key)" -Raw
        $Replaced = ReplacePlaceHolders -text $Content

        Set-Content -NoNewline -Path $_.Value -Value $replaced -Encoding UTF8
    }
}

function ReplacePlaceHolders($Text) {
    return $Text.Replace('$DiagnosticClassName$', $ClassName).Replace('$DiagnosticId$', $RuleKey)
}

### SCRIPT START ###

$SonarpediaFolder = $SonarpediaMap.Get_Item($Language)
Write-Host "Will change directory to $SonarpediaFolder to run rule-api (from $RuleApiJar )"
pushd $SonarpediaFolder

if ($RuleKey) {
    if ($RspecBranch) {
        java "-Dline.separator=`n" -jar $RuleApiJar generate -rule $RuleKey -branch $RspecBranch
    }
    else {
        java "-Dline.separator=`n" -jar $RuleApiJar generate -rule $RuleKey
    }
}
else {
    java "-Dline.separator=`n" -jar $RuleApiJar update
}

Write-Host "Ran rule-api, will move back to root"
popd

if ($ClassName -And $RuleKey) {
    if ($RspecBranch) {
        $langFolder = If ($Language -eq "vbnet") { "vbnet" } Else { "csharp" }
        CopyTestCasesFromRspec $ClassName "${RspecRepositoryPath}\\rules\\${RuleKey}\\$langFolder" $TestCasesFolder
    }

    if ($Language -eq "cs") {
        GenerateRuleClassesCS
    }
    elseif ($Language -eq "vbnet") {
        GenerateRuleClassesVB
    }
    UpdateRuleTypeMapping
    GenerateBaseClassIfSecondLanguage
}
