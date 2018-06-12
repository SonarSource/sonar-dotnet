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

        void DifferentCatches()
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
            catch (Exception)
            {
            }
            catch (ApplicationException)
            {
            }

            // catch content different
            try { }
            catch (Exception)
            {
                Console.WriteLine();
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
            }
        }

        void DifferentCatches()
        {
            try { }
            catch (ApplicationException)
            {
            }
            catch (Exception)
            {
            }

            try { } // Noncompliant, same catches, different order
            catch (Exception)
            {
            }
            catch (ApplicationException)
            {
            }
        }

        void TryStatementsDifferentNesting()
        {
            try
            {
                // Child, not on the same level
                try { }
                catch (Exception)
                {
                }
                catch (ApplicationException)
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
                catch (Exception)
                {
                }
                catch (ApplicationException)
                {
                }
            }

            try { } // Noncompliant {{Combine this 'try' with the one starting on line 102.}}
            catch (Exception)
            {
            }
            catch (ApplicationException)
            {
            }
        }
    }
}
