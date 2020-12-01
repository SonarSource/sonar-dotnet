using System;
using System.IO;

namespace Tests.Diagnostics
{
    class Program
    {
        public int MeaningOfLife
        {
            get
            {
                { // Noncompliant {{Extract this nested code block into a separate method.}}
//              ^
                    return 42;
                }
            }
            set
            {
                { // Noncompliant
//              ^
                    throw new ArgumentOutOfRangeException("Value can only be 42.");
                }
            }
        }

        public event EventHandler eventHandler;

        struct Point
        {
            public int x, y;

            public Point(int x, int y)
            {
                { // Noncompliant
//              ^
                    this.x = x;
                    this.y = y;
                }
            }
        }

        public enum MyEnum
        {
            A,
            B
        }

        public Program()
        {
            { // Noncompliant
//          ^
            }
        }

        public void Method(int value)
        { // Compliant, parent is MethodDeclaration
            { // Noncompliant
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
                        { // Noncompliant
                        }
                    }
                    break;
            }
            if (value == 3)
            { // Compliant, parent is IfStatement
                { // Noncompliant
                }
            }
            else
            { // Compliant, parent is ElseClause
                { // Noncompliant
                }
            }
            for (int i = 0; i < value; i++)
            { // Compliant, parent is ForStatement
                { // Noncompliant
                }
            }

            foreach (int element in new int [] { 1, 2 })
            {
                { // Noncompliant
                }
            }

            int j = 0;
            while (j < value)
            { // Compliant, parent is WhileStatement
                { // Noncompliant
                }
                j++;
            }

            do
            {
                { // Noncompliant
                }
                j++;
            } while (j < value);

            using (var streamReader = new StreamReader("file.txt"))
            {
                { // Noncompliant
                }
            }

            try
            {
                { // Noncompliant
                }
            }
            catch
            {
                { // Noncompliant
                }
            }
            finally
            {
                { // Noncompliant
                }
            }

            unsafe
            {
                { // Noncompliant
                }
            }

            checked
            {
                { // Noncompliant
                }
            }

            unchecked
            {
                { // Noncompliant
                }
            }

            lock (this)
            {
                { // Noncompliant
                }
            }

            eventHandler += delegate
            {
                { // Noncompliant
                }
            };

            var x = LocalFunction();
            bool LocalFunction()
            {
                { // Noncompliant
                    return true;
                }
            }


        }

    }
}
