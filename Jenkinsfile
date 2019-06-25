@Library('SonarSource@2.2') _
pipeline {
  agent {
    label 'linux'
  }
  parameters {
    string(name: 'GIT_SHA1', description: 'Git SHA1 (provided by travisci hook job)')
    string(name: 'CI_BUILD_NAME', defaultValue: 'sonar-dotnet', description: 'Build Name (provided by travisci hook job)')
    string(name: 'CI_BUILD_NUMBER', description: 'Build Number (provided by travisci hook job)')
    string(name: 'GITHUB_BRANCH', defaultValue: 'master', description: 'Git branch (provided by travisci hook job)')
    string(name: 'GITHUB_REPOSITORY_OWNER', defaultValue: 'SonarSource', description: 'Github repository owner(provided by travisci hook job)')
  }
  environment {
    SONARSOURCE_QA = 'true'
    MAVEN_TOOL = 'Maven 3.6.x'
    JDK_VERSION = 'Java 11'
  }
  stages {
    stage('Notify') {
      steps {
        sendAllNotificationQaStarted()
      }
    }
    stage('QA') {
      parallel {
        stage('DEV') {
          agent {
            label 'vs2017'
          }
          steps {
            runITs("DEV","")
          }
        }     
        stage('LTS') {
          agent {
            label 'linux'
          }
          steps {
            runITs("LTS")
          }
        }
        stage('SONARANALYZER') {
          agent {
            label 'linux'
          }
          steps {
            sh 'ci-qa.cmd'      
          }
        }                       
      }         
      post {
        always {
          sendAllNotificationQaResult()
        }
      }

    }
    stage('Promote') {
      steps {
        repoxPromoteBuild()
      }
      post {
        always {
          sendAllNotificationPromote()
        }
      }
    }
  }
}

def runITs(SQ_VERSION) {    
  withMaven(maven: MAVEN_TOOL) {
    sh 'echo "cleaning msbuild ImportBefore"'
    sh 'rm -rf $USERPROFILE/AppData/Local/Microsoft/MSBuild/15.0/Microsoft.Common.targets/ImportBefore/*'      
    mavenSetBuildVersion()   
    def CURRENT_VERSION=sh returnStdout: true, script: "mvn.cmd help:evaluate -Dexpression=project.version | grep -v '^\[\|Download\w\+\:'"
    echo CURRENT_VERSION
    runMaven(JDK_VERSION,"dependency:copy -Dartifact=org.sonarsource.dotnet:sonar-csharp-plugin:$CURRENT_VERSION -DoutputDirectory=sonar-csharp-plugin/target")
    dir("its/$TEST") {    
      runMavenOrch(JDK_VERSION,"clean verify -Dsonar.runtimeVersion=$SQ_VERSION -DcsharpVersion=$CURRENT_VERSION")
    }
  }
}