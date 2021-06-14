# How to re-generate NuGet lock files

In order to correctly re-generate the lock files, please run the following:

```
nuget locals all -clear
git clean -xfd
git rm **/packages.lock.json -f
nuget restore -LockedMode -ConfigFile "analyzers\NuGet.Config" analyzers\SonarAnalyzer.sln
```
