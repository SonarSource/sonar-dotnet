using System.Linq;

// https://github.com/dotnet/roslyn/issues/56644
public class RoslynIssue_56644
{
    private char[] invalidCharacters;

    private bool IsValidViewName(string viewName)   // Noncompliant FP, this works as expected under .NET build, but doesn't work under .NET Framework
    {
        return !this.invalidCharacters.Any(viewName.Contains);
    }
}
