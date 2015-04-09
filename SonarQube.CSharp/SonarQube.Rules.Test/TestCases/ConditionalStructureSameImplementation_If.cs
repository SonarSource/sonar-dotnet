using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    class ConditionalStructureSameCondition_If
    {
        public bool someCondition1 { get; set; }
        public bool someCondition2 { get; set; }
        public bool someCondition3 { get; set; }

        public void DoSomething1() { }
        public void DoSomething2() { }

        public void Test()
        {
            if (someCondition1)
            {
                DoSomething1();
            }
            else
            { // Noncompliant
                DoSomething1();
            }

            if (someCondition1)
            {
                DoSomething1();
            }
            else if (someCondition2)
            { // Noncompliant     
                DoSomething1();
            }
            else if (someCondition3)
            {
                DoSomething2();
            }
            else
            { // Noncompliant
                DoSomething1();
            }

            if (someCondition1)
            {
                DoSomething1();
            }
            else if (someCondition2)
            { // Noncompliant     
                DoSomething1();
            }
            else
            {// Noncompliant 
                DoSomething1();
            }
        }
    }
}
