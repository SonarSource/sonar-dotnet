// https://github.com/SonarSource/sonar-dotnet/issues/8101
namespace Repro_8101
{
    public class SomeClass(object y)
    {
        object x;

        public object Y
        {
            get { return x; }    // FN
            set { x ??= value; } // FN
        }
    }
}
