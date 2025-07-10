namespace TestXunit
{
    using System;
    using Xunit;

    using static Xunit.Assert;

    public class XUnitV3Tests
    {
        public static bool SkipIndicator => true; // Used in [Fact(SkipUnless/SkipWhen)]

        [Fact]
        public void Fact1() // Noncompliant {{Add at least one assertion to this test case.}}
        //          ^^^^^
        {
            var x = 42;
        }

        [Fact(Skip = "Reason")]
        public void SkippedFact1()
        {
            var x = 42;
        }

        [Fact(SkipExceptions = new Type[] { typeof(NotSupportedException) })]
        public void SkippedFact2()
        {
            var x = 42;
        }

        [Fact(SkipType = typeof(XUnitV3Tests))]
        public void SkippedFact3()
        {
            var x = 42;
        }

        
        [Fact(SkipUnless = nameof(SkipIndicator))]
        public void SkippedFact4()
        {
            var x = 42;
        }

        [Fact(SkipWhen = nameof(SkipIndicator))]
        public void SkippedFact5()
        {
            var x = 42;
        }

        [Fact(Explicit = true)]
        public void SkippedFact6()
        {
            var x = 42;
        }

        [Fact(Explicit = false)]
        public void SkippedFact7() // Noncompliant
        {
            var x = 42;
        }

        [Fact]
        public void SkippedFact8()
        {
            Assert.Skip("Reason");
        }

        [Fact]
        public void SkippedFact9()
        {
            SkipUnless(true, "");
        }

        [Fact]
        public void SkippedFact10()
        {
            SkipWhen(true, "");
        }
    }
}
