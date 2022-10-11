using System;
using System.Linq;
using NHibernate;
using NHibernate.Impl;

namespace Tests.Diagnostics
{
    class Program
    {
        public void ISessionMethods(ISession session, string query, string param)
        {
            session.CreateQuery(query + param); // Noncompliant
        }
    }
}
