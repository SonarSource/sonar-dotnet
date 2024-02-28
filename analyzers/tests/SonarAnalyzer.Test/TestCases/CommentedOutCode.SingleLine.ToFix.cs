// return "Some code at the start of a file".Trim();
// Leading single line comment.
class Fixes
{
    int SingleLine() { return 42; }// return 42;

    int SingleLineWithSpacing() { return 42; }              // return 42;

    int SeperateLine() { return 42; }
    // return 42;

    int MultipleLines() { return 42; }
    // int OldImplementation()
    // {
    //     return 42;
    // }
    // Trailing single line comment.
}
// Console.WriteLine("End Of file");

// https://github.com/SonarSource/sonar-dotnet/issues/8819
class Repro_8819
{
    void IncorrectRemovalOfNewLine()
    {
        // Sentence before
        // var x = 42;
        // Sentence after
    }

    void CorrectRemovalOfCommentedOutLine()
    {
        var aStatement = "Hello, World!";
        // var x = 42;
        // Sentence after
    }
}
