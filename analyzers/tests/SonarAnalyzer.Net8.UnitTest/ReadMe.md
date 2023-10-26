# Description

This project contains the analyzer unit tests that need the .Net 8 specific runtime.

Due to the following issue we cannot change the main test project to use .Net 8: https://github.com/dotnet/roslyn/issues/69578#issuecomment-1744931047

The current solution uses the main test project as a dependency, in order to have access to the `Verifier` and `VerifierBuilder`. A temporary decision that will allow us to add support on time for .net 8.

The final design is to extract these test utilities in their own project to be able to reuse them where necessary. See: https://github.com/SonarSource/sonar-dotnet/issues/8237