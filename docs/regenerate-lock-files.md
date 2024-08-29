# How to re-generate NuGet lock files

In order to correctly re-generate the lock files, please run the following:

```
nuget locals all -clear
git clean -xfd
git rm **/packages.lock.json -f
nuget restore -LockedMode -ConfigFile "private\analyzers\NuGet.Config" private\analyzers\SonarAnalyzer.sln
```
