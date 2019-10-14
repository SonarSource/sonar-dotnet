using System;

namespace Tests.Diagnostics
{
    class Program
    {
        void Simple()
        {
            try { }
            catch (Exception)
            {
            }
            finally { }

            try { } // Noncompliant {{Combine this 'try' with the one starting on line 9.}}
            catch (Exception)
            {
            }
            finally { }

            try { }
            finally { }

            try { } // Noncompliant {{Combine this 'try' with the one starting on line 15.}}
            catch (Exception)
            {
            }
            finally { }

            try { } // Noncompliant {{Combine this 'try' with the one starting on line 21.}}
            finally { }
        }

        void DifferentCatches1()
        {
            try { }
            catch (Exception)
            {
            }

            // exception type different
            try { }
            catch (ApplicationException)
            {
            }

            // catch clauses count different
            try { }
            catch (ApplicationException)
            {
            }
            catch (Exception)
            {
            }

            // catch content different
            try { }
            catch (Exception)
            {
                Console.WriteLine();
                Console.Write("");
            }

            // has finally
            try { }
            catch (Exception)
            {
            }
            finally { }

            // differs than previous by finally content
            try { }
            catch (Exception)
            {
            }
            finally
            {
                Console.WriteLine();
                Console.Write("");
            }

            // False negative - the catch clause has a name for the exception, while the try on #36 does not have a name
            try { }
            catch (Exception e)
            {
            }
            finally { }

            // exception filter
            try { }
            catch (Exception e) when (string.IsNullOrEmpty(e.Message))
            {
            }
            finally { }
        }

        void DifferentCatches2()
        {
            try { }
            catch (ApplicationException)
            {
            }
            catch (ArgumentException)
            {
            }

            try { } // Noncompliant, same catches, different order
            catch (ArgumentException)
            {
            }
            catch (ApplicationException)
            {
            }

            try { }
            catch (Exception e) when (e != null)
            {
            }

            try { } // Noncompliant, same exception filter
            catch (Exception e) when (e != null)
            {
            }
        }

        void TryStatementsDifferentNesting()
        {
            try
            {
                // Child, not on the same level
                try { }
                catch (ApplicationException)
                {
                }
                catch (Exception)
                {
                }
            }
            catch (ApplicationException)
            {
            }
            catch (Exception)
            {
            }

            if (true)
            {
                // Not on the same level
                try { }
                catch (ApplicationException)
                {
                }
                catch (Exception)
                {
                }
            }

            try { } // Noncompliant {{Combine this 'try' with the one starting on line 128.}}
            catch (ApplicationException)
            {
            }
            catch (Exception)
            {
            }
        }

        string Property
        {
            get
            {
                try { }
                finally { }

                try { } // Noncompliant {{Combine this 'try' with the one starting on line 171.}}
                finally { }

                return "";
            }
            set
            {
                try { }
                finally { }

                try { } // Noncompliant {{Combine this 'try' with the one starting on line 181.}}
                finally { }
            }
        }

        public Program() // ctor
        {
            try { }
            finally { }

            try { } // Noncompliant {{Combine this 'try' with the one starting on line 191.}}
            finally { }
        }

        public void Lambdas()
        {
            Action a = () =>
            {
                try { }
                finally { }

                try { } // Noncompliant {{Combine this 'try' with the one starting on line 202.}}
                finally { }
            };
        }

        void CodeBetweenTryBlocks(int x, int y)
        {
            try { }
            catch (Exception) { }

            x++;

            try { }    // Noncompliant FP
            catch (Exception) { }
        }
    }
}
