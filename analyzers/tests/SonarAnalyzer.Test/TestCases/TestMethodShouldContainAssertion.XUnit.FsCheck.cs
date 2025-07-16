namespace TestFsCheck
{
    using System;
    using FsCheck;
    using FsCheck.Fluent;
    using FsCheck.Xunit;
    using Xunit;

    public class FsCheckTests
    {
        [Property]
        public bool NumberPlusNumberEqualsNumberTimesTwo(int someNumber) // Compliant - Property test
        {
            return someNumber + someNumber == someNumber * 2;
        }

        [Fact]
        public void ModuloFiveReturnNaturalNumberBetweenMinus4AndPlus4()
        {
            Prop.ForAll<int>(x => x % 5 >= -4 && x % 5 <= 4)
                .QuickCheck();
        }
    }
}
