namespace SonarAnalyzer.Rules.Tests.Framework
{
    public enum LanguageVersions
    {
        None = 0,

        BeforeCSharp7,
        BeforeCSharp8,
        BeforeCSharp9,

        FromCSharp6,
        FromCSharp7,
        FromCSharp7_1,
        FromCSharp7_2,
        FromCSharp8,
        FromCSharp9,

        CSharp9,

        FromVisualBasic12,
        FromVisualBasic14,
        FromVisualBasic15,
    }
}
