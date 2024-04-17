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
