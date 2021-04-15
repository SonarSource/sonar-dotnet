namespace Tests.Diagnostics
{
    public class SwitchCaseFallsThroughToDefault
    {
        private void handleA() { }
        private void handleB() { }
        private void handleTheRest() { }

        public SwitchCaseFallsThroughToDefault(char ch)
        {
            switch (ch)
            {
                case 'b':
                    handleB();
                    break;
                case 'c':  // Noncompliant
//              ^^^^^^^^^
                default:
                    handleTheRest();
                    break;
                case 'a':
                    handleA();
                    break;
            }

            switch (ch)
            {
                case 'b':
                    handleB();
                    break;
                case 'c':  // Noncompliant {{Remove this empty 'case' clause.}}
                case 'e':  // Noncompliant
                default:
                    handleTheRest();
                    break;
                case 'a':
                    handleA();
                    break;
            }

            switch (ch)
            {
                case 'b':
                    handleB();
                    break;
                case 'c':  // Noncompliant
                default:
                case 'e':  // Noncompliant
                    handleTheRest();
                    break;
                case 'a':
                    handleA();
                    break;
            }

            switch (ch)
            {
                case 'b': // Compliant - FN
                default:
                    break;
            }
        }
    }
}
