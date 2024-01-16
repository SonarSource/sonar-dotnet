using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class CatchEmpty
    {
        public void Test()
        {
            try
            {

            }
            catch (Exception exc) // Compliant because of the comment
            {
                /*some comment here*/
            }

            try
            {

            }
            catch (Exception exc) // Compliant because of the comment
            {
                // comment
            }

            try
            {

            }
            catch (ArgumentException)
            {
                Console.WriteLine("log");
            }

            try
            {

            }
            catch (ArgumentException)
            {
            }
            catch (Exception) { } //Noncompliant {{Handle the exception or explain in a comment why it can be ignored.}}
//          ^^^^^^^^^^^^^^^^^^^^^

            try
            {

            }
            catch (ArgumentException)
            {
            }
            catch //Noncompliant
            {
            }

            try
            {

            }
            catch (ArgumentException)
            {
            }
            catch (Exception ex) when (ex is ArgumentException)  // Compliant
            {
            }
        }
    }
}
