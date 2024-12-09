namespace TestMethodImport.Tests;

public abstract class TestBase
{
    [TestMethod]
    public void TestMethodInBaseClass() =>
        Assert.AreEqual(1, 1);
}
