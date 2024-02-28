// Leading single line comment.
class Fixes
{
    int SingleLine() { return 42; }

    int SingleLineWithSpacing() { return 42; }

    int SeperateLine() { return 42; }

    int MultipleLines() { return 42; }
    // Trailing single line comment.
}

// https://github.com/SonarSource/sonar-dotnet/issues/8819
class Repro_8819
{
    void IncorrectRemovalOfNewLine()
    {
        // Sentence before        // Sentence after
    }

    void CorrectRemovalOfCommentedOutLine()
    {
        var aStatement = "Hello, World!";
        // Sentence after
    }
}
