using System;
using System.Reflection;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Test()
        {
            // RSPEC: https://jira.sonarsource.com/browse/RSPEC-3011
            Type dynClass = Type.GetType("MyInternalClass");
            // Questionable. Using BindingFlags.NonPublic will return non-public members
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Static;
//                                     ^^^^^^^^^^^^^^^^^^^^^^   {{Make sure that this accessibility bypass is safe here.}}
            MethodInfo dynMethod = dynClass.GetMethod("mymethod", bindingAttr);
            object result = dynMethod.Invoke(dynClass, null);
        }

        public BindingFlags AdditionalChecks(System.Type t)
        {
            // Using other binding attributes should be ok
            var bindingAttr = BindingFlags.Static | BindingFlags.CreateInstance | BindingFlags.DeclaredOnly |
                 BindingFlags.ExactBinding | BindingFlags.GetField | BindingFlags.InvokeMethod; // et cetera...
            var dynMeth = t.GetMember("mymethod", bindingAttr);

            // We don't detect casts to the forbidden value
            var nonPublic = (BindingFlags)32;
            dynMeth = t.GetMember("mymethod", nonPublic);

            Enum.TryParse<BindingFlags>("NonPublic", out nonPublic);
            dynMeth = t.GetMember("mymethod", nonPublic);



            bindingAttr = (((BindingFlags.NonPublic)) | BindingFlags.Static);
//                           ^^^^^^^^^^^^^^^^^^^^^^

            dynMeth = t.GetMember("mymethod", (BindingFlags.NonPublic));
//                                             ^^^^^^^^^^^^^^^^^^^^^^

            const int val = (int)BindingFlags.NonPublic; // Noncompliant
            return BindingFlags.NonPublic;  // Noncompliant
        }

        public const BindingFlags DefaultAccess = BindingFlags.OptionalParamBinding | BindingFlags.NonPublic;
//                                                                                    ^^^^^^^^^^^^^^^^^^^^^^

        private readonly BindingFlags access1 = BindingFlags.NonPublic;     // Noncompliant
        public BindingFlags Access2 { get; } = BindingFlags.NonPublic;      // Noncompliant
        public BindingFlags GetBindingFlags() => BindingFlags.NonPublic;    // Noncompliant
    }

    public class BindingFlagsImposter
    {
        public int NonPublic;
    }

    public class Derived
    {
        [Obsolete(nameof(BindingFlags.NonPublic))] // Compliant
        public void DoWork(BindingFlagsImposter BindingFlags)
        {
            var a = System.Reflection.BindingFlags.NonPublic; // Noncompliant
        }
    }
}

