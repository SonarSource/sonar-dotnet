// FIXME: This should raise S1134 once

namespace NamespaceDeclaration  // This should raise T0001 to use file-scoped namespace
{
    public enum Nothing { None }
}
