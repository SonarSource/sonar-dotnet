class Fixes
{
    int SingleLine() { return 42; }

    int SingleLineWithSpacing() { return 42; }

    /*  Multi lines with a mix like
     *  return 42;
     *  are not removed.
     */

    int SeperateLine() { return 42; }

    int WithinLine() { return 42; }

    int MultipleLines() { return 42; }

    int MultipleLinesWithSpace()
    {
        return 42;
    }
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
