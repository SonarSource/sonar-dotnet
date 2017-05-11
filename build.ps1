$ErrorActionPreference = "Stop"

$PSVersionTable.PSVersion

function testExitCode() {
    If ($LASTEXITCODE -ne 0) {
        write-host -f green "lastexitcode: $LASTEXITCODE"
        exit $LASTEXITCODE
    }
}

function maven_expression {
    param ([string]$exp)

    $out = mvn help:evaluate -B -Dexpression="$exp"
    testExitCode
    $m = ($out | Select-String -NotMatch -Pattern '^\[|Download\w\+\:')
    $m.Line
}

function set_maven_build_version {
    param ([string]$build_id)
    $CURRENT_VERSION = maven_expression "project.version"
    $RELEASE_VERSION = $CURRENT_VERSION.Split("-").Get(0)

    # In case of 2 digits, we need to add the 3rd digit (0 obviously)
    # Mandatory in order to compare versions (patch VS non patch)
    $DIGIT_COUNT = $RELEASE_VERSION.Split(".").Count
    if ( $DIGIT_COUNT -lt 3 ) {
        $RELEASE_VERSION = "$RELEASE_VERSION.0"
    }
    $NEW_VERSION = "$RELEASE_VERSION.$build_id"

    Write-Output "Replacing version $CURRENT_VERSION with $NEW_VERSION"

    mvn org.codehaus.mojo:versions-maven-plugin:2.2:set "-DnewVersion=$NEW_VERSION" -DgenerateBackupPoms=false -B -e
    testExitCode

    $env:PROJECT_VERSION = $NEW_VERSION
}

$env:DEPLOY_PULL_REQUEST = "true"

#build sonaranalyzer
Set-Location sonaranalyzer-dotnet
& .\build\Build.ps1 `
  -analyze `
  -test `
  -package `
  -githubRepo $env:GITHUB_REPO `
  -githubToken $env:GITHUB_TOKEN `
  -githubPullRequest $env:PULL_REQUEST `
  -isPullRequest $env:IS_PULLREQUEST `
  -sonarQubeUrl $env:SONAR_HOST_URL `
  -sonarQubeToken $env:SONAR_TOKEN `
  -certificatePath $env:CERT_PATH
testExitCode
Set-Location ..

# remove env variables so qgate is not displayed for java (we only want qgate for sonaranalyzer as long as only one qgate can be shown in burgr)
if (test-path Env:\CI_BUILD_NUMBER) { Remove-Item Env:\CI_BUILD_NUMBER }
if (test-path Env:\CI_PRODUCT) { Remove-Item Env:\CI_PRODUCT }

if ($env:GITHUB_BRANCH -eq 'master' -and $env:IS_PULLREQUEST -eq "false") {
    Write-Output "======= Build, deploy and analyze master"

    $CURRENT_VERSION = maven_expression "project.version"
    set_maven_build_version $env:BUILD_NUMBER

    $env:MAVEN_OPTS = "-Xmx1536m -Xms128m"

    mvn org.jacoco:jacoco-maven-plugin:prepare-agent deploy sonar:sonar `
      "-Pcoverage-per-test,deploy-sonarsource,release,sonaranalyzer" `
      "-Dmaven.test.redirectTestOutputToFile=false" `
      "-Dsonar.host.url=$env:SONAR_HOST_URL" `
      "-Dsonar.login=$env:SONAR_TOKEN" `
      "-Dsonar.projectVersion=$CURRENT_VERSION" `
      -B -e -V
    testExitCode
}
elseif ($env:IS_PULLREQUEST -eq "true" -and $env:GITHUB_TOKEN -ne $null) {
    Write-Output '======= Build and analyze pull request'

    # Do not deploy a SNAPSHOT version but the release version related to this build and PR
    set_maven_build_version $env:BUILD_NUMBER

    # No need for Maven phase "install" as the generated JAR files do not need to be installed
    # in Maven local repository. Phase "verify" is enough.

    $env:MAVEN_OPTS = "-Xmx1G -Xms128m"
    if ($env:DEPLOY_PULL_REQUEST -eq "true") {
        Write-Output '======= with deploy'
        mvn org.jacoco:jacoco-maven-plugin:prepare-agent deploy sonar:sonar `
      "-Pdeploy-sonarsource,sonaranalyzer" `
      "-Dmaven.test.redirectTestOutputToFile=false" `
      "-Dsonar.analysis.mode=issues" `
      "-Dsonar.github.pullRequest=$env:PULL_REQUEST" `
      "-Dsonar.github.repository=$env:GITHUB_REPO" `
      "-Dsonar.github.oauth=$env:GITHUB_TOKEN" `
      "-Dsonar.host.url=$env:SONAR_HOST_URL" `
      "-Dsonar.login=$env:SONAR_TOKEN" `
      -B -e -V
        testExitCode
    }
    else {
        Write-Output '======= no deploy'
        mvn org.jacoco:jacoco-maven-plugin:prepare-agent verify sonar:sonar `
      "-Dmaven.test.redirectTestOutputToFile=false" `
      "-Dsonar.analysis.mode=issues" `
      "-Dsonar.github.pullRequest=$env:PULL_REQUEST" `
      "-Dsonar.github.repository=$env:GITHUB_REPO" `
      "-Dsonar.github.oauth=$env:GITHUB_TOKEN" `
      "-Dsonar.host.url=$env:SONAR_HOST_URL" `
      "-Dsonar.login=$env:SONAR_TOKEN" `
      -B -e -V
        testExitCode
    }
}
else {
    Write-Output '======= Build, no analysis, no deploy'

    set_maven_build_version $env:BUILD_NUMBER

    # No need for Maven phase "install" as the generated JAR files do not need to be installed
    # in Maven local repository. Phase "verify" is enough.

    mvn verify `
      "-Dmaven.test.redirectTestOutputToFile=false" `
      -B -e -V
    testExitCode
}