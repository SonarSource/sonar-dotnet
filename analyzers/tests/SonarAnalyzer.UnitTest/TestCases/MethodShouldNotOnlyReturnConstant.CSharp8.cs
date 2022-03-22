namespace Tests.Diagnostics
{
    interface ISomeInterface {
        public bool IsCondition() => true; // Noncompliant - FP, see: https://github.com/SonarSource/sonar-dotnet/issues/5498
    }
}
