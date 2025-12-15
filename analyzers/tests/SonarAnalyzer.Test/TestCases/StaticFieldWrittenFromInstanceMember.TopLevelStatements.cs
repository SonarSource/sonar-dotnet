Record.count++;             // Compliant - static field set from static method
Structure.count++;          // Compliant - static field set from static method
RecordStructure.count++;    // Compliant - static field set from static method

record Record
{
    internal static int count = 0;
}

struct Structure
{
    internal static int count = 0;
}

record struct RecordStructure
{
    internal static int count = 0;
}
