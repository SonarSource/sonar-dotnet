var topLevel = true;

void LocalFunctionInTopLevelStatement(int a, int b, int c, int d, int e) // Noncompliant
{
}

public record R(int a, int b, int c, int d, int e); // Compliant, this ParameterList syntax defines fields, not parameters
