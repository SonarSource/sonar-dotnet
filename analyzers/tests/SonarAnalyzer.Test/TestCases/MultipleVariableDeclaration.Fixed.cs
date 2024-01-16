namespace Tests.Diagnostics
{
    public class MultipleVariableDeclaration
    {
        private int /*aaa1*/ a /*aaa2*/;
        private int
            /*bbb1*/ b /*bbb2*/;

        private int c;
        public MultipleVariableDeclaration(int n)
        {
            int /*aaa1*/a = 0 /*aaa2*/;
            int /*bbb1*/ b = 0 /*bbb2*/;
            int c = 0;
            for (int i = 0, j = 2; i < 3; i++)
            {

            }
        }
    }
}
