$ErrorActionPreference = "Stop"

function testExitCode(){
    If($LASTEXITCODE -ne 0) {
        write-host -f green "lastexitcode: $LASTEXITCODE"
        exit $LASTEXITCODE
    }
}

function CreateRelease(
    [string] $productName, 
    [string] $version, 
    [string] $expectedSha1, 
    [string] $releaseVersion, 
    [string] $additionalNugetFileContent,
    [string] $nugetPath)
{
    # delete NuGet file if exists
    If (Test-Path "$productName.$version.nupkg"){
        Remove-Item "$productName.$version.nupkg"
    }

    # download nuget package
    $url = "https://repox.sonarsource.com/sonarsource-nuget-releases/$productName.$version.nupkg"
    Invoke-WebRequest -UseBasicParsing -Uri "$url" -OutFile "$productName.$version.nupkg"

    # create working folder
    $currentDir=(Get-Item -Path ".\" -Verbose).FullName
    $workDir=$currentDir + "\$productName"
    New-Item $workDir -type directory -force
    Remove-Item "$workDir\*" -recurse

    # unzip nuget package
    $zipName="$productName.$version.zip"
    Move-Item "$productName.$version.nupkg" $zipName -force
    $shell_app=new-object -com shell.application
    $destination = $shell_app.NameSpace($workDir)
    $zip_file = $shell_app.NameSpace("$currentDir\$zipName")
    Write-Host "Unzipping $workDir\$zipName"
    $destination.CopyHere($zip_file.Items(), 0x14) 

    # get sha1
    $productVersion = ""
    If (Test-Path "$workDir\analyzers"){
        $productversion=ls $workDir\analyzers\SonarAnalyzer.dll | % { $_.versioninfo.productversion }
    }
    ElseIf (Test-Path "$workDir\assembly"){
        $productversion=ls $workDir\assembly\SonarAnalyzer.dll | % { $_.versioninfo.productversion }
    }
    Else {
        If (-Not ($productName.StartsWith("SonarAnalyzer.RuleDocGenerator."))){
            Write-Error "Can't find Sha1"
            return
        }
    }

    If ($productVersion -ne "") {
        $sha1=$productversion.Substring($productversion.LastIndexOf('Sha1:')+5)
        if ($sha1 -ne $expectedSha1){
            Write-Error "SHA1 doesn't match expected ($sha1)"
            return
        }
    }

    # change content of nuspec file
    (Get-Content $workDir\$productName.nuspec) -replace "<version>$version</version>", "<version>$releaseVersion</version>" | Set-Content $workDir\$productName.nuspec

    $fileContent = (Get-Content $workDir\$productName.nuspec) -replace "</metadata>", "</metadata>$additionalNugetFileContent"
    $Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding($False)
    [System.IO.File]::WriteAllLines("$workDir\$productName.nuspec", $fileContent, $Utf8NoBomEncoding)

    # repack with the new name
    & $nugetPath pack $workDir\$productName.nuspec
    testExitCode
}

function ReleaseAll(
    [string] $buildVersion, 
    [string] $sha1, 
    [string] $releaseVersion, 
    [string] $nugetPath)
{
    # these should match the ones used to build the artifacts originally:

    $analyzerContent = "<files>
     <file src=""analyzers\*.dll"" target=""analyzers\"" />
     <file src=""tools\*.ps1"" target=""tools\"" />
    </files>"

    $descriptorContent =  "<files>
     <file src=""xml\*.xml"" target=""xml\"" />
    </files>"

    $scannerContent =  "<files>
     <file src=""assembly\*.*"" target=""assembly\"" />
     <file src=""protobuf\*.proto"" target=""protobuf\"" />
    </files>"


    # analyzers
    CreateRelease   -productName "SonarAnalyzer.VisualBasic" `
                    -version $buildVersion -expectedSha1 $sha1 -releaseVersion $releaseVersion -additionalNugetFileContent $analyzerContent -nugetPath $nugetPath
    CreateRelease   -productName "SonarAnalyzer.CSharp" `
                    -version $buildVersion -expectedSha1 $sha1 -releaseVersion $releaseVersion -additionalNugetFileContent $analyzerContent -nugetPath $nugetPath

    # descriptors
    CreateRelease   -productName "SonarAnalyzer.RuleDocGenerator.VisualBasic" `
                    -version $buildVersion -expectedSha1 $sha1 -releaseVersion $releaseVersion -additionalNugetFileContent $descriptorContent -nugetPath $nugetPath
    CreateRelease   -productName "SonarAnalyzer.RuleDocGenerator.CSharp" `
                    -version $buildVersion -expectedSha1 $sha1 -releaseVersion $releaseVersion -additionalNugetFileContent $descriptorContent -nugetPath $nugetPath

    # scanner
    CreateRelease   -productName "SonarAnalyzer.Scanner" `
                    -version $buildVersion -expectedSha1 $sha1  -releaseVersion $releaseVersion -additionalNugetFileContent $scannerContent -nugetPath $nugetPath
}

ReleaseAll -buildVersion $env:PROMOTED_VERSION -sha1 $env:SHA1 -releaseVersion $env:RELEASE_VERSION -nugetPath $env:NUGET_PATH

# for local testing:
# $releaseVersion = "1.19.0-alpha01"
# ReleaseAll -buildVersion "1.19.0-build00932" -sha1 "d5b415b237f694b166f6ece8b0404fedd58a21fe" -releaseVersion $releaseVersion -nugetPath "nuget"

function pushToRepox(
    [string] $productName,
    [string] $releaseVersion,
    [string] $nugetPath)
{
    & $nugetPath push "$productName.$releaseVersion.nupkg" -Source repox
    testExitCode

    #compute artifact name from filename
    $artifact=$productName
    $file = Get-ChildItem "$productName.$releaseVersion.nupkg"
    $filePath=$file.FullName    
    (Get-Content .\build\poms\$artifact\pom.xml) -replace "file-$artifact", "$filePath" | Set-Content .\build\poms\$artifact\pom.xml
}

# push to repox
# setup Nuget.config
del $env:APPDATA\NuGet\NuGet.Config
& $env:NUGET_PATH sources Add -Name repox -Source https://repox.sonarsource.com/api/nuget/sonarsource-nuget-qa/
testExitCode
$apikey = $env:ARTIFACTORY_DEPLOY_USERNAME+":"+$env:ARTIFACTORY_DEPLOY_PASSWORD
& $env:NUGET_PATH setapikey $apikey -Source repox
testExitCode

pushToRepox -productName "SonarAnalyzer.CSharp"                         -releaseVersion $env:RELEASE_VERSION -nugetPath $env:NUGET_PATH
pushToRepox -productName "SonarAnalyzer.VisualBasic"                    -releaseVersion $env:RELEASE_VERSION -nugetPath $env:NUGET_PATH
pushToRepox -productName "SonarAnalyzer.RuleDocGenerator.CSharp"        -releaseVersion $env:RELEASE_VERSION -nugetPath $env:NUGET_PATH
pushToRepox -productName "SonarAnalyzer.RuleDocGenerator.VisualBasic"   -releaseVersion $env:RELEASE_VERSION -nugetPath $env:NUGET_PATH
pushToRepox -productName "SonarAnalyzer.Scanner"                        -releaseVersion $env:RELEASE_VERSION -nugetPath $env:NUGET_PATH

        
#upload to maven repo
$version = $env:RELEASE_VERSION        
cd build\poms
write-host -f green  "set version $version in pom.xml"
$command = "mvn versions:set -DgenerateBackupPoms=false -DnewVersion='$version'"
iex $command
write-host -f green  "set version $version in env VAR PROJECT_VERSION for artifactory buildinfo metadata"
$env:PROJECT_VERSION=$version
write-host -f green  "set the buildnumber to this job build number"
$env:BUILD_ID=$version
write-host -f green  "Deploy to repox with $version"    
$command = 'mvn deploy -Pdeploy-sonarsource -B -e -V'
iex $command