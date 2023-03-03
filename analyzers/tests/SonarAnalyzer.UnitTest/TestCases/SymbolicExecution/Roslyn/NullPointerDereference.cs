namespace Tests.Diagnostics
{
    public class Test
    {
        public enum Helper
        {
            A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P
        }
        public static void UnsupportedSize()
        {
            var helper = Helper.A;
            object o = null;
            if (helper == Helper.A | helper == Helper.B | helper == Helper.C | helper == Helper.D
                | helper == Helper.E | helper == Helper.F)
            {
                o.ToString(); // Noncompliant (old engine: FN, the condition state generation is too big to explore all constraint combinations)
            }
        }
    }
}
