namespace TestFsCheck
{
    using System;
    using FsCheck;
    using FsCheck.Fluent;
    using NUnit.Framework;

    using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;

    public class FsCheckTests
    {
        [Property]
        public bool NumberPlusNumberEqualsNumberTimesTwo(int someNumber) // Compliant - Property test
        {
            return someNumber + someNumber == someNumber * 2;
        }

        [Test]
        public void ModuloFiveReturnNaturalNumberBetweenMinus4AndPlus4()
        {
            Prop.ForAll<int>(x => x % 5 >= -4 && x % 5 <= 4)
                .QuickCheck();
        }
    }
}
