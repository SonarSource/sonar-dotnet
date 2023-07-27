// https://github.com/SonarSource/sonar-dotnet/issues/7694
using ReproS3928.Two;
using ReproS3928.One; // FN - This is not needed

namespace ReproS3928
{
    public static class S1128
    {
        static void Method()
        {
            var _ = new S1128Child()
            {
                Id = 42 // Issue raise if this line is commented.
            };
        }
    }
}
