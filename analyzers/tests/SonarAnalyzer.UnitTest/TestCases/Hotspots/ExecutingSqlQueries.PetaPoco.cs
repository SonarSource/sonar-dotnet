﻿using System;
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

    public void IDatabaseMethods(IDatabase database, string query, string param, string otherParam)
    {
        database.Execute(query);         // Compliant
        database.Execute(query + param); // Noncompliant

        database.ExecuteScalar<Entity>(query);         // Compliant
        database.ExecuteScalar<Entity>(query + param); // Noncompliant

        database.Query<Entity>(query);                                          // Compliant
        database.Query<Entity>(query + param);                                  // Noncompliant
        database.Query<Entity>(query, param + otherParam);                      // Noncompliant FP. The second argument is params object[] args and is safe.
        database.Query<Entity>(new[] { typeof(Entity) }, null,  query + param); // FN. This overload is not supported (sql string in the third parameter)
        database.Query<Entity, Entity>(query + param);                          // Noncompliant
        database.Query<Entity, Entity, Entity>(query + param);                  // Noncompliant
        database.Query<Entity, Entity, Entity, Entity>(query + param);          // Noncompliant
        database.Query<Entity, Entity, Entity, Entity, Entity>(query + param);  // Noncompliant
    }
}

class Entity
{
    public int Id { get; set; }
}
