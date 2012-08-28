//
// Unit Tests for AvoidMessageChains Rule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2008 Néstor Salceda
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.Smells;
using Mono.Cecil;

using Test.Rules.Fixtures;
using Test.Rules.Definitions;

using NUnit.Framework;

namespace Test.Rules.Smells {

	class One {
		public Two ReturnTwo (int x) {return null;}
		public Two Two {get; set;}
	}

	class Two {
		public Three ReturnThree (string x) {return null;}
		public Three Three {get; set;}
		public object Tag {get; set;}
	}

	class Three {
		public Four ReturnFour (object o, int f) {return null;}
		public Four Four {get; set;}
	}
	class Four {
		public Five ReturnFive (float f) {return null;}
	}
	class Five {
		public Six ReturnSix () {return null;}
	}
	class Six {
		public One ReturnOne (char c) {return null;}
	}

	[TestFixture]
	public class AvoidMessageChainsTest : MethodRuleTestFixture<AvoidMessageChainsRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		public void MethodWithSmallChain ()
		{
			// 1111111 2222222222222 333333333333333333333333333333333
			new One ().ReturnTwo (3).ReturnThree ("Avoid chaining me");
		}

		public void MethodWithArgumentsWithSmallChain (string s)
		{
			//          1111111111 222222222222222 33333333333333333333333333333
			Four four = new Two ().ReturnThree (s).ReturnFour (new object (), 3);
		}

		[Test]
		public void MethodWithSmallChainTest ()
		{
			// default is 5 so small chains (like 3) are ok
			AssertRuleSuccess<AvoidMessageChainsTest> ("MethodWithSmallChain");
			AssertRuleSuccess<AvoidMessageChainsTest> ("MethodWithArgumentsWithSmallChain");
		}

		[Test]
		public void MethodWithSmallChainLowerLimitTest ()
		{
			int chain = Rule.MaxChainLength;
			try {
				// unless we lower the limit
				Rule.MaxChainLength = 2;
				AssertRuleFailure<AvoidMessageChainsTest> ("MethodWithSmallChain", 1);
				AssertRuleFailure<AvoidMessageChainsTest> ("MethodWithArgumentsWithSmallChain", 1);
			}
			finally {
				Rule.MaxChainLength = chain;
			}
		}

		public void MethodWithChain ()
		{
			object obj = new object ();
			// 1111111 2222222222222 333333333333333333333333333333 4444444444444444444 55555555555555 666666666666 777777777777777
			new One ().ReturnTwo (3).ReturnThree ("Ha ha! Chained").ReturnFour (obj, 5).ReturnFive (3).ReturnSix ().ReturnOne ('a');
		}

		public void MethodWithChain_Array ()
		{
			// 11111111111111  222222222222 33333333 44444444444 5555555555555555555 6666666
			Console.WriteLine (new byte [5].Clone ().ToString ().ToUpperInvariant ().Trim ());
		}

		[Test]
		public void MethodWithChainTest ()
		{
			AssertRuleFailure<AvoidMessageChainsTest> ("MethodWithChain", 1);
			AssertRuleFailure<AvoidMessageChainsTest> ("MethodWithChain_Array", 1);
		}

		public void MethodWithVariousChains ()
		{
			object obj = new object ();
			new One ().ReturnTwo (3).ReturnThree ("Ha ha! Chained").ReturnFour (obj, 5).ReturnFive (3).ReturnSix ().ReturnOne ('a');
			Two two= new Three ().ReturnFour (obj, 3).ReturnFive (4 + 4).ReturnSix ().ReturnOne ('2').ReturnTwo (8);
		}

		[Test]
		public void MethodWithVariousChainsTest ()
		{
			AssertRuleFailure<AvoidMessageChainsTest> ("MethodWithVariousChains", 2);

		}

		public void MethodWithArgumentsChained (int x, float f)
		{
			//11111111 2222222222222 333333333333333333333333333333 44444444444444444444444444444 55555555555555 666666666666 777777777777777
			new One ().ReturnTwo (x).ReturnThree ("Ha ha! Chained").ReturnFour (new object (), x).ReturnFive (f).ReturnSix ().ReturnOne ('a');
		}

		[Test]
		public void MethodWithArgumentsChainedTest ()
		{
			AssertRuleFailure<AvoidMessageChainsTest> ("MethodWithArgumentsChained", 1);
		}

		public void SmallChainWithTemporaryVariables ()
		{
			//                11111111111111111111111111111111 2222222222 3333333
			Version version = Assembly.GetExecutingAssembly ().GetName ().Version;
			int major = version.Major;
		}

		public void NoChainWithTemporaryVariables ()
		{
			int x;
			char c;
			Console.WriteLine ("More tests");
		}

		[Test]
		public void NoChainWithTemporaryVariablesTest ()
		{
			AssertRuleSuccess<AvoidMessageChainsTest> ("SmallChainWithTemporaryVariables");
			AssertRuleSuccess<AvoidMessageChainsTest> ("NoChainWithTemporaryVariables");
		}

		public bool Compare (int x, long y)
		{
			//        11111111111 2222222222222222222 3333333      11111111111 2222222 3333333333333333333
			return (x.ToString ().ToUpperInvariant ().Trim () == y.ToString ().Trim ().ToLowerInvariant ());
		}

		[Test]
		public void CompareTest ()
		{
			AssertRuleSuccess<AvoidMessageChainsTest> ("Compare");
		}
		
		private static One Start {get; set;}

		public void StaticProperty ()
		{
			One beta = new One();

			// 1 222 333                 11111 222 333
			beta.Two.Tag = ((PlatformID) Start.Two.Tag);	// because Start is static there's no load instruction between these two lines
			
			//                11111 222 333
			Console.WriteLine(Start.Two.Tag);
		}

		[Test]
		public void StaticPropertyTest ()
		{
			AssertRuleSuccess<AvoidMessageChainsTest> ("StaticProperty");
		}
		
		public object Linq ()
		{
			var query = from n in Runner.Defects
				    group n by n.Rule into a
				    orderby a.Key.Name
				    select new {
					    Rule = a.Key,
					    Value = from o in a
						    group o by o.Target into r
						    orderby r.ToString()
						    select new {
							    Target = r.Key,
							    Value = r
						    }
				    };
				
			return query;
		}

		[Test]
		public void LinqTest ()
		{
			AssertRuleSuccess<AvoidMessageChainsTest> ("Linq");
		}
	}
}
