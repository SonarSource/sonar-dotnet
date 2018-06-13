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
                Console.Write();
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
                Console.Write();
            }

            // FN - the catch clause has a name for the exception, while the try on #36 does not have a name
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

            try { } // Noncompliant {{Combine this 'try' with the one starting on line 118.}}
            catch (Exception)
            {
            }
            catch (ApplicationException)
            {
            }
        }

        string Property
        {
            get
            {
                try { }
                finally { }

                try { } // Noncompliant {{Combine this 'try' with the one starting on line 161.}}
                finally { }

                return 0;
            }
            set
            {
                try { }
                finally { }

                try { } // Noncompliant {{Combine this 'try' with the one starting on line 171.}}
                finally { }
            }
        }

        public Program() // ctor
        {
            try { }
            finally { }

            try { } // Noncompliant {{Combine this 'try' with the one starting on line 181.}}
            finally { }
        }

        public void Lambdas()
        {
            Action a = () =>
            {
                try { }
                finally { }

                try { } // Noncompliant {{Combine this 'try' with the one starting on line 192.}}
                finally { }
            };
        }
    }
}
