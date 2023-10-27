# For each test case file in the RspecRulePath, copy the file to the OutputFolder with the name $FileName + $Scenario + $Extension
#
# The scenario is determined by the following rules:
# 1. The test case file name contains a dot, e.g. "MyTestCase.Scenario.razor"
#    Then the scenario name is the second part of the file name, e.g. "Scenario"
# 2. The test case file name contain a dot, but the test case file is in a subfolder, e.g. "MyTestCase\MyTestCase.Scenario.razor"
#    Then the scenario name is the second part of the file name, e.g. "Scenario", ignoring the folder name
# 3. The test case file name does not contain a dot, but the test case file is in a subfolder, e.g. "MyTestCase\Scenario.razor"
#    Then the scenario name is the file name, e.g. "Scenario", ignoring the folder name
# 4. The test case file name does not contain a dot and the test case file is in the root folder, e.g. "MyTestCase.razor"
#    Then the scenario name is empty, e.g. ""
#
# Example:
#   1. MyTestCase.Scenario.razor            =>  RuleName.Scenario.razor
#   2. MyTestCase\MyTestCase.Scenario.razor =>  RuleName.Scenario.razor
#   3. MyTestCase\Scenario.razor            =>  RuleName.Scenario.razor
#   4. MyTestCase.razor                     =>  RuleName.razor
#
function CopyTestCasesFromRspec($FileName, $RspecRulePath, $OutputFolder) {
    $TestCaseFileExtension = @(
        "*.cs",
        "*.vb",
        "*.razor",
        "*.cshtml"
    )

    Get-ChildItem -Recurse -Path $RspecRulePath -Include $TestCaseFileExtension -File | ForEach-Object {

        $scenario = "";

        if ($_.BaseName.Contains("."))
        {
            $scenario = "." + $($_.BaseName -Split "\." | Select-Object -Last 1)
        }
        elseif ($_.Directory.FullName -ne $(Convert-Path $RspecRulePath))
        {
            $scenario = "." + $_.BaseName
        }

        $outputPath = "${OutputFolder}\$FileName$scenario$($_.Extension)"

        if (Test-Path -Path $outputPath -PathType Leaf)
        {
            $count = (Get-ChildItem -Path $OutputFolder -Name | Where-Object { $_.StartsWith("$FileName$scenario") }).Count
            $outputPath = "${OutputFolder}\$FileName$scenario.$count$($_.Extension)"
        }

        Set-Content -NoNewline -Path $outputPath -Value $(Get-Content $_ -Raw) -Encoding UTF8
    }
}
