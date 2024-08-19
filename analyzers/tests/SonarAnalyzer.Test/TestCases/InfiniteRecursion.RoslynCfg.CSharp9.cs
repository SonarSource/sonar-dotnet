// https://github.com/SonarSource/sonar-dotnet/issues/6646
namespace Repro_6646
{
    public class Repro
    {
        public string Name
        {
            init // Noncompliant
            {
                Name = value;
            }
        }

        public string Arrow
        {
            init => Arrow = value;   // Noncompliant
        }
    }
}
