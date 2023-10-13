# How to run:
#   $ Invoke-Pester -Output Detailed
#
# Requires Pester 5.0.x
#   https://pester.dev/docs/quick-start
#   $ Install-Module -Name Pester -Force -SkipPublisherCheck
#   $ Import-Module Pester -Passthru
BeforeAll {
    . $PSScriptRoot/../CopyTestCasesFromRspec.ps1

    $InputFolder = "TestDrive:\input"
    $OutputFolder = "TestDrive:\output"
}

$FileExtension = @(".cs", ".vb", ".razor", ".cshtml")

Describe 'CopyTestCasesFromRspec - <_> files' -ForEach $FileExtension {
    BeforeEach {
        if (Test-Path $InputFolder)
        {
            Remove-Item -Force -Recurse $InputFolder
        }
        if (Test-Path $OutputFolder)
        {
            Remove-Item -Force -Recurse $OutputFolder
        }

        New-Item -Path "TestDrive:\" -Name "input" -ItemType "directory"
        New-Item -Path "TestDrive:\" -Name "output" -ItemType "directory"
    }


    It 'should copy <_> file' {
        New-Item -Path $InputFolder -Name "SomeTestCase$_" -ItemType "file"

        CopyTestCasesFromRspec "RuleName" $InputFolder $OutputFolder

        Get-ChildItem -Path $OutputFolder -File -name | Should -Contain "RuleName$_"
        "$OutputFolder\RuleName$_" | Should -Exist
    }

    It 'should take into account composed test case name' {
        New-Item -Path $InputFolder -Name "SomeTestCase.Scenario1$_" -ItemType "file"

        CopyTestCasesFromRspec "RuleName" $InputFolder $OutputFolder

        Get-ChildItem -Path $OutputFolder -File -name | Should -Contain "RuleName.Scenario1$_"
        "$OutputFolder\RuleName.Scenario1$_" | Should -Exist
    }

    It 'should include file if under a folder with filename used as a scenario' {
        New-Item -Path $InputFolder -Name "AFolder" -ItemType "directory"
        New-Item -Path "TestDrive:\input\AFolder" -Name "Scenario1$_" -ItemType "file"

        CopyTestCasesFromRspec "RuleName" $InputFolder $OutputFolder

        Get-ChildItem -Path $OutputFolder -File -name | Should -Contain "RuleName.Scenario1$_"
        "$OutputFolder\RuleName.Scenario1$_" | Should -Exist
    }
}
