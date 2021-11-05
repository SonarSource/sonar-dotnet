namespace Tests.Diagnostics
{
    public interface IFace
    {
        void Method2(int[,] a); //Noncompliant
//           ^^^^^^^
    }

    public class PublicMethodWithMultidimensionalArray :  IFace
    {
        public void Method1(int[][] a) //Noncompliant
        {
        }

        public void Method2(int[,] a) //Compliant, implements interface
        {
        }
    }
}
