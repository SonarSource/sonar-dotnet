using SomeName.WithHelper; // Error [CS0246]
using AliasHelper = SatchelPerks; // Noncompliant {{Do not use 'Helper' in type names.}}
//    ^^^^^^^^^^^
class SonarHelper // Noncompliant {{Do not use 'Helper' in type names.}}
//    ^^^^^^^^^^^
{
    public delegate int HelperDelegate(int x, int y);

    void HelperMethod()
    {

    }
}

class HelperSonarContext // Noncompliant
{
}

class SonarHelperContext // Noncompliant
{
}

interface ISonarHelper // Noncompliant
{
    void MethodHelper();
}

struct ValueTypeHelper // Noncompliant
{
    public bool HelperEnabled { get; set; }
}

enum EnumHelper // Noncompliant
{
}

record RecordHelper // Noncompliant
{
}

record struct RecordStructHelper // Noncompliant
{
}

class SatchelPerks
{

}

interface IMonsterWhelpEradicator
{

}

namespace Something.Helper { }              // Noncompliant {{Do not use 'Helper' in type names.}}
namespace Something.Helpers { }             // Noncompliant
namespace Something.SyntaxHelper { }        // Noncompliant
namespace Something.SyntaxHelpers { }       // Noncompliant
namespace Something.Helpers.Extensions { }  // Noncompliant
namespace Helper.Helping.Helpers.Work { }   // Noncompliant, just once
namespace { }   // Error [CS1001] Identifier expected
namespace;      // Error [CS1001] Identifier expected
                // Error@-1 [CS8956] File-scoped namespace must precede all other members in a file.
