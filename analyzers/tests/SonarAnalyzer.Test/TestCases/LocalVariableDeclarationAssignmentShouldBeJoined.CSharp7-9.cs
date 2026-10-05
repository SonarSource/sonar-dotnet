class LocalVariableDeclarationAssignmentShouldBeJoined
{
    void MixedDeconstruction()
    {
        int y = 0;
        int x;                  // Compliant - joining into "(int x, y) = ..." requires C# 10
        (x, y) = (1, 2);
    }

    void MixedDeconstructionWithDiscard()
    {
        int y = 0;
        int x;                  // Compliant - joining into "(int x, y, _) = ..." requires C# 10
        (x, y, _) = (1, 2, 3);
    }

    void DeconstructionWithDiscardOnly()
    {
        int x;                  // Noncompliant
        (x, _) = (1, 2);
    }
}
