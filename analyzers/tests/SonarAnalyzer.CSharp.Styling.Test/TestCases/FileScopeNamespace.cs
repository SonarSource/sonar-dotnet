
namespace Outer // Noncompliant {{Use file-scoped namespace.}}
//        ^^^^^
{
    public class NotRelevant
    {

    }

    namespace Inner // Noncompliant, nested namespace should not be used in general
    {

    }
}


namespace   // Error [CS1001] Identifier expected
{           // Noncompliant^1#0

}
