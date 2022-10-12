using System;
using System.Linq;
using PetaPoco;

class Program
{
    public void IExecuteMethods(IExecute execute, string query, string param)
    {
        execute.Execute(query);         // Compliant
        execute.Execute(query + param); // Noncompliant

        execute.ExecuteScalar<Entity>(query);         // Compliant
        execute.ExecuteScalar<Entity>(query + param); // Noncompliant
    }
}

class Entity
{
    public int Id { get; set; }
}
