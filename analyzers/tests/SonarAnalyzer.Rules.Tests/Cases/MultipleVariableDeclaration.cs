namespace Tests.Diagnostics
{
    public class MultipleVariableDeclaration
    {
        private int /*aaa1*/ a /*aaa2*/,
            /*bbb1*/ b /*bbb2*/; // Noncompliant
//                   ^
        private int c;
        public MultipleVariableDeclaration(int n)
        {
            int /*aaa1*/a = 0 /*aaa2*/, /*bbb1*/ b = 0 /*bbb2*/; // Noncompliant {{Declare 'b' in a separate statement.}}
//                                               ^
            int c = 0;
            for (int i = 0, j = 2; i < 3; i++)
            {

            }
        }
    }
}
