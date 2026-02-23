using System.Collections.Generic;
using System.IO;

namespace Tests.Diagnostics
{
    public class StringConcatenationInLoop
    {
        private string field = "";
        private string Property { get; set; }

        public StringConcatenationInLoop(IList<MyObject> objects, string p, StringConcatenationInLoop sample)
        {
            string s = "";
            Dictionary<string, string> dict = new Dictionary<string, string>();
            int t = 0;

            for (int i = 0; i < 50; i++)
            {
                var sLoop = "";

                s = s + "a" + "b";  // Noncompliant {{Use a StringBuilder instead.}}
//              ^^^^^^^^^^^^^^^^^
                s += "a";     // Noncompliant
//              ^^^^^^^^

                s = s + i.ToString(); // Noncompliant
                s += i.ToString(); // Noncompliant
                s += "a" + s; // Noncompliant
                s += string.Format("{0} world;", "Hello"); // Noncompliant
                dict["a"] = dict["a"] + "a"; // Compliant: Technically an FN, but its an uncommon use case. It is much more likely to use a loop variable as the index on a dict/array.
                dict[i.ToString()] = dict[i.ToString()] + "a"; // Compliant
                s = "a" + (s == null ? "Empty" : s); // FN

                i = i + 1;
                i += 1;
                t = t + 1;
                t = t + 1 - 1 + 1;
                t += 1;
                sLoop = sLoop + "a";
                sLoop += "a";

                // https://github.com/SonarSource/sonar-dotnet/issues/7722
                p = p + "a";                        // Noncompliant NET-1259
                p += "a";                           // Noncompliant

                field = field + "a";                // Noncompliant
                field += "a";                       // Noncompliant

                Property = Property + "a";          // Noncompliant
                Property += "a";                    // Noncompliant

                sample.field = sample.field + "a";  // Noncompliant
            }

            while (true)
            {
                var sLoop = "";

                s = s + "a"; // Noncompliant
                s += "a"; // Noncompliant
                sLoop = s + "a"; // Compliant
                sLoop += s + "a"; // Compliant

                // See https://github.com/SonarSource/sonar-dotnet/issues/1138
                s = s ?? "b";
            }

            foreach (var o in objects)
            {
                var sLoop = "";

                s = s + "a"; // Noncompliant
                s += "a"; // Noncompliant
                sLoop = s + "a"; // Compliant
                sLoop += s + "a"; // Compliant
            }

            do
            {
                var sLoop = "";

                s = s + "a"; // Noncompliant
                s += "a"; // Noncompliant
                sLoop = s + "a"; // Compliant
                sLoop += s + "a"; // Compliant
            }
            while (true);

            s = s + "a"; // Compliant
            s += "a"; // Compliant

            p = p + "a"; // Compliant
            p += "a"; // Compliant

            var l = "";
            l = l + "a"; // Compliant
            l += "a"; // Compliant
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/5521
        void Repro_5521(IList<MyObject> objects)
        {
            foreach (var obj in objects)
            {
                obj.Name += "a"; // Compliant
                obj.Name = obj.Name + "a"; // Compliant
            }
        }

        // https://github.com/SonarSource/sonar-dotnet/issues/7713
        void Repro_7713()
        {
            var s = "";
            var t = "";

            while (true)
            {
                s = "a" + "b" + "c" + s; // Noncompliant
                s = "a" + "b" + s; // Noncompliant
                s = "a" + s; // Noncompliant
                s = "a" + s + "b"; // Noncompliant

                s = "a" + "b" + t; // Compliant
            }
        }
    }

    public class MyObject
    {
        public string Name { get; set; }
    }

    class Sample
    {
        public string S { get; set; }
        void M(List<Sample> others)
        {
            foreach (var sample in others)
            {
                sample.S = sample.S + "- disabled"; // Compliant
            }
        }
    }
}
