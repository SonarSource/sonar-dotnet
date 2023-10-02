class PrimaryConstructorParameterNotUsed(int someInt) { } // Compliant

class PrimaryConstructorParameterUsedInMethod(int someInt) // Compliant
{
    int Method => someInt;
}
