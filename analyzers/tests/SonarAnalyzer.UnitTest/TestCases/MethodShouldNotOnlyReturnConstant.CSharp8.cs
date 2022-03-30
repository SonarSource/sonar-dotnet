namespace Tests.Diagnostics
{
    interface ISomeInterface {
        public bool IsCondition() => true; // Compliant, see: https://github.com/SonarSource/sonar-dotnet/issues/5498
    }
}
