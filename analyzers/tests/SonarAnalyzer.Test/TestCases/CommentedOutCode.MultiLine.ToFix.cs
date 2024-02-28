/* return "Some code at the start of a file".Trim(); */
/*
{
    int SingleLine() { return 42; }
}
*/
class Fixes
{
    int SingleLine() { return 42; }/* return 42; */

    int SingleLineWithSpacing() { return 42; }              /* return 42; */

    /*  Multi lines with a mix like
     *  return 42;
     *  are not removed.
     */

    int SeperateLine() { return 42; }
    /* return 42;*/

    int /* return 17; */ WithinLine() { return 42; }

    int MultipleLines() { return 42; }
    /*
     {
         return 42;
     }
     */

    int MultipleLinesWithSpace()
    {
        /*
         *
         * return 17;
         *
         */
        return 42;
    }
}
/* Console.WriteLine("End Of file"); */

// https://github.com/SonarSource/sonar-dotnet/issues/8819
class Repro_8819
{
    void IncorrectRemovalOfNewLine()
    {
        // Sentence before
        /* var x = 42;
         * var y = 42;
         */
        // Sentence after
    }

    void CorrectRemovalOfCommentedOutLine()
    {
        var aStatement = "Hello, World!";
        /* var x = 42;
         * var y = 42;
         */
        // Sentence after
    }
}
