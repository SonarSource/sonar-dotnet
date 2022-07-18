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

$RspecSuffixMap = @{
    "cs" = "c#";
    "vbnet" = "vb.net";
}

$SonarpediaMap = @{
    "cs" = ".\analyzers\src\SonarAnalyzer.CSharp";
    "vbnet" = ".\analyzers\src\SonarAnalyzer.VisualBasic";
}

function UpdateTestEntry() {
    $Rule = CurrentRuleMetadata
    $RuleType = $Rule.Type
    if (!$RuleType) {
        return
    }

    $FileToEdit = if ($Language -eq "cs") {"RuleTypeMappingCS"} else {"RuleTypeMappingVB"}
    $TestFile = "${AnalyzersPath}\\tests\\SonarAnalyzer.UnitTest\\PackagingTests\\$fileToEdit.cs"
    (Get-Content "${TestFile}") -replace "//\s*\[`"$ruleKey`"\]", "[`"$ruleKey`"] = `"$ruleType`"" | Set-Content "${TestFile}" -Encoding UTF8
}

function CurrentRuleMetadata() {
    $Suffix = $RspecSuffixMap.Get_Item($Language)
    return Get-Content -Raw "${AnalyzersPath}\\rspec\\${Language}\\${RuleKey}_${suffix}.json" | ConvertFrom-Json
}

function GenerateCsRuleClasses() {
    $csharpRulesFolder         = "${AnalyzersPath}\\src\\SonarAnalyzer.CSharp\\Rules"
    $csharpRuleTestsFolder     = "${AnalyzersPath}\\tests\\SonarAnalyzer.UnitTest\\Rules"
    $csharpRuleTestCasesFolder = "${AnalyzersPath}\\tests\\SonarAnalyzer.UnitTest\\TestCases"

    $filesMap = @{
        "Rule.CS.cs"     = "${csharpRulesFolder}\\${className}.cs"
        "Test.CS.cs"     = "${csharpRuleTestsFolder}\\${className}Test.cs"
        "TestCase.CS.cs" = "${csharpRuleTestCasesFolder}\\${className}.cs"
    }
    WriteClasses -filesMap $filesMap -classNameToUse $className
}

function GenerateVbRuleClasses() {
    $vbRulesFolder         = "${AnalyzersPath}\\src\\SonarAnalyzer.VisualBasic\\Rules"
    $csRulesFolder         = "${AnalyzersPath}\\src\\SonarAnalyzer.CSharp\\Rules"
    $ruleTestsFolder       = "${AnalyzersPath}\\tests\\SonarAnalyzer.UnitTest\\Rules"
    $vbRuleTestCasesFolder = "${AnalyzersPath}\\tests\\SonarAnalyzer.UnitTest\\TestCases"

    $csClassName = FindCsName -rulesPath $csRulesFolder

    $filesMap = @{}
    if ($csClassName) {
        $className = $csClassName
        AppendVbTestCase -ruleTestsFolder $ruleTestsFolder
    }
    else {
        $filesMap["Test.VB.cs"] = "${ruleTestsFolder}\\${className}Test.cs"
    }

    $filesMap["Rule.VB.cs"] = "${vbRulesFolder}\\${className}.cs"
    $filesMap["TestCase.VB.vb"] = "${vbRuleTestCasesFolder}\\${className}.vb"

    WriteClasses -filesMap $filesMap
}

function GenerateBaseClassIfSecondLanguage()
{
    $vbRulesFolder         = "${AnalyzersPath}\\src\\SonarAnalyzer.VisualBasic\\Rules"
    $csRulesFolder         = "${AnalyzersPath}\\src\\SonarAnalyzer.CSharp\\Rules"
    $commonRulesFolder     = "${AnalyzersPath}\\src\\SonarAnalyzer.Common\\Rules"

    $csClassName = FindCsName -rulesPath $csRulesFolder
    $vbClassName = FindCsName -rulesPath $vbRulesFolder

    $csFilePath = "${csRulesFolder}\\${csClassName}.cs"
    $vbFilePath = "${vbRulesFolder}\\${vbClassName}.cs"
    $filesMap = @{}

    if ($csClassName -And $vbClassName) {
        $existingCSClassText = Get-Content -Path "${csRulesFolder}\\${csClassName}.cs" -Raw
        $existingVBClassText = Get-Content -Path "${vbRulesFolder}\\${vbClassName}.cs" -Raw

        $oldCsClass ="public sealed class ${csClassName} : SonarDiagnosticAnalyzer"
        $newCsClass ="public sealed class ${csClassName} : ${className}Base<SyntaxKind>"
        $oldVbClass ="public sealed class ${vbClassName} : SonarDiagnosticAnalyzer"
        $newVbClass ="public sealed class ${vbClassName} : ${className}Base<SyntaxKind>"
        $existingCSClassText = ReplaceTextInString -oldText $oldCsClass -newText $newCsClass -modifiableString $existingCSClassText
        $existingVBClassText = ReplaceTextInString -oldText $oldVbClass -newText $newVbClass -modifiableString $existingVBClassText

        $diagnosticIdToken = "    private const string DiagnosticId = ""${ruleKey}"";"
        $existingCSClassText = RemoveText -textToRemove $diagnosticIdToken -modifiableString $existingCSClassText
        $existingVBClassText = RemoveText -textToRemove $diagnosticIdToken -modifiableString $existingVBClassText

        $messageFormatToken = "   private const string MessageFormat = ""FIXME"";"
        $existingCSClassText = RemoveText -textToRemove $messageFormatToken -modifiableString $existingCSClassText
        $existingVBClassText = RemoveText -textToRemove $messageFormatToken -modifiableString $existingVBClassText

        $ruleToken = "    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);"
        $existingCSClassText = RemoveText -textToRemove $ruleToken -modifiableString $existingCSClassText
        $existingVBClassText = RemoveText -textToRemove $ruleToken -modifiableString $existingVBClassText

        $supportedDiagToken = "    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);"
        $csLanguageFacadeToken = "    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;"
        $vbLanguageFacadeToken = "    protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;"
        $existingCSClassText = ReplaceTextInString -oldText $supportedDiagToken -newText $csLanguageFacadeToken -modifiableString $existingCSClassText
        $existingVBClassText = ReplaceTextInString -oldText $supportedDiagToken -newText $vbLanguageFacadeToken -modifiableString $existingVBClassText

        $filesMap["Rule.Base.cs"] = "${commonRulesFolder}\\${className}Base.cs"

        Set-Content -NoNewline `
                    -Path $csFilePath `
                    -Value $existingCSClassText `
                    -Encoding UTF8

        Set-Content -NoNewline `
                    -Path $vbFilePath `
                    -Value $existingVBClassText `
                    -Encoding UTF8

        WriteClasses -filesMap $filesMap
    }
}

function ReplaceTextInString($oldText, $newText, $modifiableString)
{
    $idx = $modifiableString.LastIndexOf($oldText)
    if ($idx -gt -1) {
        $modifiableString = $modifiableString.Remove($idx, $oldText.Length).Insert($idx, "$newText")
    }

    return $modifiableString
}

function RemoveText($textToRemove, $modifiableString)
{
    $idx = $modifiableString.LastIndexOf($textToRemove)
    if ($idx -gt -1) {
        $modifiableString = $modifiableString.Remove($idx, $textToRemove.Length + 2)
    }

    return $modifiableString
}

function AppendVbTestCase($ruleTestsFolder) {
    $usingToken = "using SonarAnalyzer.Rules.CSharp;"
    $usingTokenCS = "using CS = SonarAnalyzer.Rules.CSharp;"
    $usingTokenVB = "using VB = SonarAnalyzer.Rules.VisualBasic;"
    $namespaceToken = "namespace SonarAnalyzer.UnitTest.Rules"
    $oldToken = "    }" # For files using old namespaces
    $newToken = "}"     # For files using file scoped namespaces

    $text = Get-Content -Path "${ruleTestsFolder}\\${csClassName}Test.cs" -Raw
    $methodVB = Get-Content -Path "${RuleTemplateFolder}\\TestMethod.VB.cs" -Raw

    $usingTokenIdx = $text.IndexOf($usingToken)
    if ($usingTokenIdx -gt -1) {
        $text = $text.Remove($usingTokenIdx, $usingToken.Length + 1)
    }

    $token = if ($text.Contains("${namespaceToken};")) {$newToken} else {$oldToken}
    $idx = $text.LastIndexOf($token)
    if ($idx -gt -1) {
        $text = $text.Insert($idx, "`n${methodVB}")
    }
    else {
        $text = "${$text}`n${methodVB}"
    }

    $namespaceTokenIdx = $text.IndexOf($namespaceToken);
    if ($namespaceTokenIdx -gt -1) {
        $text = $text.Insert($namespaceTokenIdx - 1, "${usingTokenCS}`n${usingTokenVB}`n")
    }

    $text = $text.Replace("new ${csClassName}", "new CS.${csClassName}")
    $text = $text.Replace("<${csClassName}>", "<CS.${csClassName}>")
    $text = $text.Replace("builder =", "builderCS =")
    $text = $text.Replace("builder.", "builderCS.")

    $replaced = ReplaceTokens -text $text
    Set-Content -NoNewline -Path "${ruleTestsFolder}\\${csClassName}Test.cs" -Value $replaced -Encoding UTF8
}

function FindCsName($rulesPath) {
    $csRuleFiles = Get-ChildItem -Path $rulesPath -Filter "*.cs"

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

        Set-Content -NoNewline -Path $_.Value -Value $replaced -Encoding UTF8
    }
}

function ReplaceTokens($text) {
    return $text `
        -replace '\$DiagnosticClassName\$', $className `
        -replace '\$DiagnosticId\$', $ruleKey
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
    if ($Language -eq "cs") {
       GenerateCsRuleClasses
       UpdateTestEntry
       GenerateBaseClassIfSecondLanguage
    }
    elseif ($Language -eq "vbnet") {
       GenerateVbRuleClasses
       UpdateTestEntry
       GenerateBaseClassIfSecondLanguage
    }
}
