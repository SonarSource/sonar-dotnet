$ErrorActionPreference = "Stop"

$PSVersionTable.PSVersion

function InstallAppveyorTools
{
	$travisUtilsVersion = "33"
	$localPath = "$env:USERPROFILE\.local"
	$tmp = [IO.Path]::GetTempFileName() | Rename-Item -NewName { [IO.Path]::ChangeExtension($_,".tmp.zip") } -PassThru
	wget "https://github.com/SonarSource/travis-utils/archive/v$travisUtilsVersion.zip" -OutFile $tmp
	
	Expand-Archive -Force -Path $tmp -DestinationPath $localPath
    
	$mavenLocal = "$env:USERPROFILE\.m2"
	if (-not(Test-Path $mavenLocal))
	{
		mkdir $mavenLocal | Out-Null
	}
	echo "Installating Travis Utils public Maven settings.xml into $mavenLocal"
	Copy-Item "$localPath\travis-utils-$travisUtilsVersion\m2\settings-public.xml" "$mavenLocal\settings.xml"
}

function CheckLastExitCode
{
    param ([int[]]$SuccessCodes = @(0))

    if ($SuccessCodes -notcontains $LastExitCode)
	{
        $msg = @"
EXE RETURNED EXIT CODE $LastExitCode
CALLSTACK:$(Get-PSCallStack | Out-String)
"@
        throw $msg
    }
}

InstallAppveyorTools

function maven_expression
{
  param ([string]$exp)
  
  $out = mvn help:evaluate -B -Dexpression="$exp"
  CheckLastExitCode
  $m = ($out | Select-String -NotMatch -Pattern '^\[|Download\w\+\:')
  $m.Line
}

function set_maven_build_version
{
  param ([string]$build_id)
  $CURRENT_VERSION=maven_expression "project.version"
  $RELEASE_VERSION=$CURRENT_VERSION.Split("-").Get(0)

# In case of 2 digits, we need to add the 3rd digit (0 obviously)
# Mandatory in order to compare versions (patch VS non patch)
  $DIGIT_COUNT=$RELEASE_VERSION.Split(".").Count
  if ( $DIGIT_COUNT -lt 3 )
  {
    $RELEASE_VERSION="$RELEASE_VERSION.0"
  }
  $NEW_VERSION="$RELEASE_VERSION.$build_id"

  echo "Replacing version $CURRENT_VERSION with $NEW_VERSION"

  mvn org.codehaus.mojo:versions-maven-plugin:2.2:set "-DnewVersion=$NEW_VERSION" -DgenerateBackupPoms=false -B -e
  CheckLastExitCode

  $env:PROJECT_VERSION=$NEW_VERSION
}

$env:DEPLOY_PULL_REQUEST="true"

# Waiting for a fix in parent pom:
$env:BUILD_ID=$env:APPVEYOR_BUILD_NUMBER
$env:CI_BUILD_URL="https://ci.appveyor.com/project/SonarSource/sonar-csharp/build/$env:APPVEYOR_BUILD_VERSION"
$env:GIT_BRANCH=$env:APPVEYOR_REPO_BRANCH
$env:GIT_COMMIT=$env:APPVEYOR_REPO_COMMIT

if ($env:APPVEYOR_REPO_BRANCH -eq 'master' -and $env:APPVEYOR_PULL_REQUEST_NUMBER -eq $null)
{
  echo "======= Build, deploy and analyze master"
  
  $CURRENT_VERSION=maven_expression "project.version"
  set_maven_build_version $env:APPVEYOR_BUILD_NUMBER
  
  $env:MAVEN_OPTS="-Xmx1536m -Xms128m"
  
  mvn org.jacoco:jacoco-maven-plugin:prepare-agent deploy sonar:sonar `
      "-Pcoverage-per-test,deploy-sonarsource,release" `
      "-Dmaven.test.redirectTestOutputToFile=false" `
      "-Dsonar.host.url=$env:SONAR_HOST_URL" `
      "-Dsonar.login=$env:SONAR_TOKEN" `
      "-Dsonar.projectVersion=$CURRENT_VERSION" `
      -B -e -V
  CheckLastExitCode
}
elseif ($env:APPVEYOR_PULL_REQUEST_NUMBER -ne $null -and $env:GITHUB_TOKEN -ne $null)
{
  echo '======= Build and analyze pull request'
  
  # Do not deploy a SNAPSHOT version but the release version related to this build and PR
  set_maven_build_version $env:APPVEYOR_BUILD_NUMBER

  # No need for Maven phase "install" as the generated JAR files do not need to be installed
  # in Maven local repository. Phase "verify" is enough.

  $env:MAVEN_OPTS="-Xmx1G -Xms128m"
  if ($env:DEPLOY_PULL_REQUEST -eq "true")
  {
    echo '======= with deploy'
    mvn org.jacoco:jacoco-maven-plugin:prepare-agent deploy sonar:sonar `
      -Pdeploy-sonarsource `
      "-Dmaven.test.redirectTestOutputToFile=false" `
      "-Dsonar.analysis.mode=issues" `
      "-Dsonar.github.pullRequest=$env:APPVEYOR_PULL_REQUEST_NUMBER" `
      "-Dsonar.github.repository=$env:APPVEYOR_REPO_NAME" `
      "-Dsonar.github.oauth=$env:GITHUB_TOKEN" `
      "-Dsonar.host.url=$env:SONAR_HOST_URL" `
      "-Dsonar.login=$env:SONAR_TOKEN" `
      -B -e -V
    CheckLastExitCode
  }
  else
  {
    echo '======= no deploy'
    mvn org.jacoco:jacoco-maven-plugin:prepare-agent verify sonar:sonar `
      "-Dmaven.test.redirectTestOutputToFile=false" `
      "-Dsonar.analysis.mode=issues" `
      "-Dsonar.github.pullRequest=$env:APPVEYOR_PULL_REQUEST_NUMBER" `
      "-Dsonar.github.repository=$env:APPVEYOR_REPO_NAME" `
      "-Dsonar.github.oauth=$env:GITHUB_TOKEN" `
      "-Dsonar.host.url=$env:SONAR_HOST_URL" `
      "-Dsonar.login=$env:SONAR_TOKEN" `
      -B -e -V
    CheckLastExitCode
  }
}
else
{
  echo '======= Build, no analysis, no deploy'

  # No need for Maven phase "install" as the generated JAR files do not need to be installed
  # in Maven local repository. Phase "verify" is enough.

  mvn verify `
      "-Dmaven.test.redirectTestOutputToFile=false" `
      -B -e -V
  CheckLastExitCode
}
