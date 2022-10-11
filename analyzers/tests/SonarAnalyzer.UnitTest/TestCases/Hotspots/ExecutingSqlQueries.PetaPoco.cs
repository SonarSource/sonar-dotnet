using System;
using System.Linq;
using PetaPoco;

namespace Tests.Diagnostics
{
    class Program
    {
        public void IExecuteMethods(IExecute execute, string query, string param)
        {
            execute.Execute(query + param); // Noncompliant
        }
    }
}
