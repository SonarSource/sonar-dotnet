using System;
using System.IO;
using NUnit.Framework;

namespace NUnit.Core.Tests
{
    [TestFixture]
    public class AssemblyHelperTests
    {
        [Test]
        public void GetPathForAssembly()
        {
            string path = AssemblyHelper.GetAssemblyPath(this.GetType().Assembly);
            Assert.That(Path.GetFileName(path), Is.EqualTo("nunit.core.tests.dll").IgnoreCase);
            Assert.That(File.Exists(path));
        }

        [Test]
        public void GetPathForType()
        {
            string path = AssemblyHelper.GetAssemblyPath(this.GetType());
            Assert.That(Path.GetFileName(path), Is.EqualTo("nunit.core.tests.dll").IgnoreCase);
            Assert.That(File.Exists(path));
        }
    }
}
