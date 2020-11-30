namespace Tests.Diagnostics
{
    public class SwitchCasesMinimumThree
    {
        public SwitchCasesMinimumThree(int n)
        {
            switch (n) // Noncompliant
//          ^^^^^^
            {
                case 0:
                    break;
                default:
                    break;
            }

            switch (n) // Noncompliant {{Replace this 'switch' statement with 'if' statements to increase readability.}}
            {
            }

            switch (n)
            {
                case 0:
                    break;
                case 1:
                default:
                    var x=5;
                    break;
            }
        }

        public int SwitchExpressionsWithNone(int n) =>
            n switch // Noncompliant {{Remove this 'switch' expression to increase readability.}}
//            ^^^^^^
            {

            };

        public int SwitchExpressionsWithOne(int n) =>
            n switch // Noncompliant {{Replace this 'switch' expression with a ternary conditional operator to increase readability.}}
            {
                1 => 1,
            };

        public int SwitchExpressionsWithTwo(int n) =>
            n switch //  Noncompliant {{Replace this 'switch' expression with a ternary conditional operator to increase readability.}}
            {
                1 => 1,
                _ => 2,
            };

        public int SwitchExpressions(string type)
        {
            var x = type switch // Noncompliant {{Remove this 'switch' expression to increase readability.}}
            {
                _ => 1,
            };

            var y = type switch // Noncompliant
            {
                _ => 0,
                _ => 1 // Error [CS8510]
            };


            return type switch // Noncompliant
            {
                _ => 1
            };
        }

        public int SwitchExpressionsWithTwoNonDefault(int n) =>
            n switch // Compliant
            {
                1 => 1,
                2 => 2,
            };

        public int SwitchExpressionsWithMany(int n) =>
            n switch // Compliant
            {
                1 => 1,
                2 => 2,
                3 => 3,
                4 => 4,
                5 => 5,
            };

    }
}
