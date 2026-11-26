namespace Tests.Diagnostics
{
    public static class ConditionalCompilation
    {
        public static void ElseBlockWithExcludedIf(bool useAlternative)
        {
#if NEW_API
            if (useAlternative)
            {
                Alternative();
            }
            else
#endif
            {   // Noncompliant FP NET-5282: the braces are the 'else' body when NEW_API is defined (https://github.com/SonarSource/sonar-dotnet/issues/9858)
                Original();
            }
        }

        public static void RealNestedBlock()
        {
            { // Noncompliant
//          ^
                Original();
            }
        }

        private static void Alternative() { }
        private static void Original() { }
    }
}
