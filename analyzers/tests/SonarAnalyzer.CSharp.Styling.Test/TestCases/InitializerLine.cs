using System.Collections.Generic;

public class Sample
{
    private string nextLineString =
        "Ipsum"; // Noncompliant {{Move this initializer to the previous line.}}
    //  ^^^^^^^

    private string sameLine = "Lorem";

    private string nextLineWhereTheFinalLinewouldBeLongButWithin200Limit_Noncompliant =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus faucibus elit et phasellus, Length is 200.";    // Noncompliant, it fits

    private string nextLineWhereTheFinalLinewouldBeTooLongSoItMustBeOnTheNextLineAnyway_Length201_Compliant =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus. Final Length is 201.";

    private string nextLineWhereTheFinalLinewouldBeTooLongSoItMustBeOnTheNextLineAnyway_Length223_Compliant =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus. Final Length is definitely more than 200.";

    private int[] values =
        [
        ];
    public List<int> newOnNextLine =
        new()               // Noncompliant, multiline only in this case
//      ^^^^^
        {
        };

    public List<int> newOnNextLineWithArguments_Implicit =
        new(42)             // Noncompliant
//      ^^^^^^^
        {
        };

    public List<int> newOnNextLineWithArguments_Explicit =
        new List<int>(42)   // Noncompliant
//      ^^^^^^^^^^^^^^^^^
        {
        };

    public List<int> newOnNextLineWithArguments_Explicit_NoArgumentList =
        new List<int>       // Noncompliant
//      ^^^^^^^^^^^^^
        {
        };

    public List<int> newOnNextLineWithArgumentsWhereTheFinalLineWouldBeTooLongSoItMustBeOnTheNextLineAnyway_Implicit_Compliant =
        new(1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1)
        {
        };

    public List<int> newOnNextLineWithArgumentsWhereTheFinalLineWouldBeTooLongSoItMustBeOnTheNextLineAnyway_Explicit_Compliant =
        new List<int>(1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1 + 1)
        {
        };

    public List<int> newOnSameLine = new()
    {
    };

    private string multiLine = "Lorem" +
        "Ipsum";

    private int computedNextLine =
        Compute();              // Noncompliant
    private int computedSameLine = Compute();
    private int computedMultipleLines = Compute()
        + Compute();

    private static int Compute() => 42;

    private int multipleSameLineA = 1, multipleSameLineB = 2;
    private int multipleMixedLineA = 1, multipleMixedLineB =
        2;                      // Noncompliant
    private int multipleMultiLineA =
        1, multipleMultiLineB = // Noncompliant
        2;                      // Noncompliant
}
