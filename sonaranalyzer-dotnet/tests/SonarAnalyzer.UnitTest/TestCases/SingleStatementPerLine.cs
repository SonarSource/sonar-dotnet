using System;
namespace Tests.Diagnostics
{
    class SingleStatementPerLine
    {
        public SingleStatementPerLine()
        {
            if (someCondition) doSomething(); //Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            if (someCondition) { doSomething(); } //Noncompliant
            else
            {
                doSomething();
            }

            var i = 5;
            i = 6; i = 7; //Noncompliant {{Reformat the code to have only one statement per line.}}

            if (someCollection.Any(x=>true))
            {

            }

            if(_sent < _repeat)
            {
                _actor.Tell(message);
                _sent++;
            }
            else if (_received >= _repeat)
            {
                Console.WriteLine("done {0}", Self.Path);
                _latch.SetResult(true);
            }

            Func<NancyContext, Response> item1 = r => { return null; }; //Compliant
            item1 = (r) => null; //Compliant
            Func<NancyContext, Response> item2 = r => { return null; return null; }; // Noncompliant
            TestDelegate testDelB = delegate (string s) { Console.WriteLine(s); };//Compliant
            TestDelegate testDelB = delegate (string s) { Console.WriteLine(s); Console.WriteLine(s); };//Noncompliant

            Receive<ClusterEvent.ReachableMember>(member =>
            {
                if (member.Member.Status == MemberStatus.Up) AddMember(member.Member); //Noncompliant
            });

            switch (x)
            {
            }; // Compliant, because blocks are ignored

            if (false) {
                ; } // Compliant, because blocks are ignored

            ; ; // Noncompliant
        }
    }
}
