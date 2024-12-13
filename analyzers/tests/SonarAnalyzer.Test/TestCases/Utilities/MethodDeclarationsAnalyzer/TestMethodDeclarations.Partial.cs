namespace Samples.CSharp;

using Microsoft.VisualStudio.TestTools.UnitTesting;

public partial class PartialClass : BaseClass
{
    [TestMethod]
    public void InSecondFile() { }

    [TestMethod]
    public partial void PartialMethod() { }
}
