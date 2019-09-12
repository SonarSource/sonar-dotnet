using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Method(int value)
        { // Compliant, parent is MethodDeclaration
            { // Noncompliant {{Extract this nested code block into a separate method.}}
//          ^
            }
            switch (value)
            {
                case 1:
                    break;
                case 2:
                    { // Compliant, parent is SwitchSection
                        int a = 1;
                        { // Noncompliant
//                      ^
                            int b = 2;
                        }
                    }
                    break;
                default:
                    { // Compliant, parent is SwitchSection

                    }
                    break;
            }
            if (value == 3)
            { // Compliant, parent is IfStatement
            }
            else
            { // Compliant, parent is ElseClause
            }
            for (int i = 0; i < value; i++)
            { // Compliant, parent is ForStatement
            }
            int j = 0;
            while (j < value)
            { // Compliant, parent is WhileStatement
                j++;
            }
        }

    }
}
