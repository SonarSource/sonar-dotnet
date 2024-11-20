The files were created by the `importLogMessagesFromMultipleFile()` test.
The `telemetry.pb` files can be inspected by [ProtoBufViewer](https://martin-strecker-sonarsource.github.io/ProtoBufViewer/)
with the definition file [AnalyzerReport.proto](https://github.com/SonarSource/sonar-dotnet-enterprise/blob/master/analyzers/src/SonarAnalyzer.Core/Protobuf/AnalyzerReport.proto).

The pb files contain this content:

### 0\telemetry.pb

``` 
projectFullPath: A.csproj
targetFramework: TFM1
targetFramework: TFM2
languageVersion: CS12 
```

### 1\telemetry.pb

```
projectFullPath: B.csproj
targetFramework: TFM1
targetFramework: TFM2
targetFramework: TFM3
languageVersion: CS12
```
