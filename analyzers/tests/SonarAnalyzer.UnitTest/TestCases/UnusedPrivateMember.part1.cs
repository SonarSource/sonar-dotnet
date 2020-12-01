namespace SonarCheck
{
    public partial class Partial
    {
        void DoWork() // Noncompliant
        {
            Initialize();
        }
        partial void Initialize();
    }

}
