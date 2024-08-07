# Roslyn Shim Layer

## Purpose

Enables the new Roslyn API usage while maintaining compatibility with old versions.

## Structure

The project is divided into the following parts:
- At the root level is the `Lightup` layer introduced by the stylecop analyzers. The code was copied from
  https://github.com/DotNetAnalyzers/StyleCopAnalyzers/tree/master/StyleCop.Analyzers/StyleCop.Analyzers/Lightup excepting `Syntax.xml` and `OperationInterfaces.xml`.
- [Syntax.xml](https://github.com/dotnet/roslyn/blob/main/src/Compilers/CSharp/Portable/Syntax/Syntax.xml) and [OperationInterfaces.xml](https://github.com/dotnet/roslyn/blob/main/src/Compilers/Core/Portable/Operations/OperationInterfaces.xml) are copied from the Roslyn repository (ideally, from the latest release version branch).
They are used to generate the `Lightup` layer. We copy them from Roslyn and not from StyleCopAnalyzers to ensure that we have the most recent version to be able to support the latest features.
- All the additions made by SonarSource are in the `Sonar` folder.

## Conventions

- Keep our changes and all logic in a dedicated directory `Sonar`, using partial classes, extension methods, and external handlers.
- Keep the namespace. Have different license headers.
- Inject ourselves to the StyleCop code with minimal changes that are annotated with `// Sonar` comment everywhere.
