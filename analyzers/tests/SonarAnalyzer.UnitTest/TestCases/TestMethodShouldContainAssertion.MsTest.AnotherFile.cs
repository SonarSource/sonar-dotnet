namespace TestMsTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class HelperFromAnotherSyntaxTree
    {
        public static void DoNothing() { }

        public static void Is42Nested(int i) =>
            Is42(i);

        public static void Is42(int i) =>
            Assert.AreEqual(42, i);
    }
}
