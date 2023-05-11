// Fixed
class Compliant
{
    int SingleLine() { return 42; } // Single line comment.
    int BlockComment() { return 42; } /* Single line comment. */
}

class Noncompliant
{
    // Fixed
    int SingleLine() { return 42; }

    // Fixed
    int SingleLineWithSpacing() { return 42; }

    // Fixed
    int SeperateLine() { return 42; }

    // Fixed
    int MultipleLines() { return 42; }

    // Fixed
    int SingleLineBlock() { return 42; }

    // Fixed
    int SeperateLineBlock() { return 42; }
    /*
     * return 42;
     */
}
