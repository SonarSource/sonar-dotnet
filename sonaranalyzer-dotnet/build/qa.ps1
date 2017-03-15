$ErrorActionPreference = "Stop"

function testExitCode(){
    If($LASTEXITCODE -ne 0) {
        write-host -f green "lastexitcode: $LASTEXITCODE"
        exit $LASTEXITCODE
    }
}

#cleanup
$strFileName="%USERPROFILE%\AppData\Local\Microsoft\MSBuild\14.0\Microsoft.Common.targets\ImportBefore\SonarAnalyzer.Testing.ImportBefore.targets" 
If (Test-Path $strFileName){
	Remove-Item $strFileName
}

$env:FILENAME="$env:ARTIFACT.$env:VERSION.nupkg"

#download nuget package
$ARTIFACTORY_SRC_REPO="sonarsource-nuget-qa"
$url = "$env:ARTIFACTORY_URL/$ARTIFACTORY_SRC_REPO/$env:FILENAME"
Write-Host "Downloading $url"
$pair = "$($env:REPOX_QAPUBLICADMIN_USERNAME):$($env:REPOX_QAPUBLICADMIN_PASSWORD)"
$encodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($pair))
$basicAuthValue = "Basic $encodedCreds"
$Headers = @{Authorization = $basicAuthValue}
Invoke-WebRequest -UseBasicParsing -Uri "$url" -Headers $Headers -OutFile $env:FILENAME

#unzip nuget package
$zipName=$env:FILENAME.Substring(0, $env:FILENAME.LastIndexOf('.'))+".zip"
Move-Item $env:FILENAME $zipName -force
$shell_app=new-object -com shell.application
$baseDir=(Get-Item -Path ".\" -Verbose).FullName
$destination = $shell_app.NameSpace($baseDir)
$zip_file = $shell_app.NameSpace("$baseDir\$zipName")
Write-Host "Unzipping $baseDir\$zipName"
$destination.CopyHere($zip_file.Items(), 0x14) 

#get sha1
$productversion="empty"
if (Test-Path .\analyzers\SonarAnalyzer.dll) {
  $productversion=ls .\analyzers\SonarAnalyzer.dll | % { $_.versioninfo.productversion }
}else{
  if (Test-Path .\assembly\SonarAnalyzer.dll) {
    $productversion=ls .\assembly\SonarAnalyzer.dll | % { $_.versioninfo.productversion }
  }   
}

if ($productversion -eq "empty") {
    Write-Host "Couldn't determine sha1"
    exit 1
} 

#find the sha1 
$sha1=$productversion.Substring($productversion.LastIndexOf('Sha1:')+5)
Write-Host "Checking out $sha1"
$s="SHA1=$sha1"
$s | out-file -encoding utf8 ".\sha1.properties"
#find the branch
$GITHUB_BRANCH=$productversion.split("{ }")[1].Substring(7)
Write-Host "GITHUB_BRANCH $GITHUB_BRANCH"
if ($GITHUB_BRANCH.StartsWith("refs/heads/")) {
    $GITHUB_BRANCH=$GITHUB_BRANCH.Substring(11)
}
$s="GITHUB_BRANCH=$GITHUB_BRANCH"
Write-Host "$s"
$s | out-file -encoding utf8 -append ".\sha1.properties"
#convert sha1 property file to unix for jenkins compatiblity
Get-ChildItem .\sha1.properties | ForEach-Object {
  $contents = [IO.File]::ReadAllText($_) -replace "`r`n?", "`n"
  $utf8 = New-Object System.Text.UTF8Encoding $false
  [IO.File]::WriteAllText($_, $contents, $utf8)
}

#checkout commit
git fetch --tags --progress git@github.com:$env:GITHUB_REPOSITORY_OWNER_NAME/$env:CI_BUILD_NAME.git +refs/heads/*:refs/remotes/origin/*
testExitCode
git checkout -f $sha1
testExitCode

#nuget restore
& $env:NUGET_PATH restore .\SonarAnalyzer.sln
testExitCode

#build tests
& $env:MSBUILD_PATH .\SonarAnalyzer.sln /p:configuration=Release /p:DeployExtension=false /p:ZipPackageCompressionLevel=normal /v:m /p:defineConstants=SignAssembly /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=$env:CERT_PATH
testExitCode

#move dlls to correct locations
Write-Host "Installing downloaded dlls"
$dllpath="empty"
if ($env:FILENAME -like '*CSharp*') {
    $dllpath="SonarAnalyzer.CSharp"
}
if ($env:FILENAME -like '*VisualBasic*') {
    $dllpath="SonarAnalyzer.VisualBasic"
}
if ($env:FILENAME -like '*Scanner*') {
    $dllpath="SonarAnalyzer.Scanner"
}
Write-Host "Copying analyzers"
Copy-Item .\analyzers\*.dll .\src\$dllpath\bin\Release -force
Copy-Item .\analyzers\*.dll .\its\binaries -force

dir .\src\$dllpath\bin\Release
dir .\its\binaries

#run tests
Write-Host "Start tests"
& $env:VSTEST_PATH .\src\Tests\SonarAnalyzer.Platform.Integration.UnitTest\bin\Release\SonarAnalyzer.Platform.Integration.UnitTest.dll
testExitCode
& $env:VSTEST_PATH .\src\Tests\SonarAnalyzer.UnitTest\bin\Release\SonarAnalyzer.UnitTest.dll
testExitCode
 
#run regression-test
Write-Host "Start regression tests"
cd .\its
git submodule update --init --recursive --depth 1
testExitCode
cmd /c .\regression-test.bat
testExitCode