using System;
using System.Collections.Generic;
using System.IO;

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
                default:
                    handleTheRest();
                    break;
                case 'a':
                    handleA();
                    break;
            }
        }
    }
}
