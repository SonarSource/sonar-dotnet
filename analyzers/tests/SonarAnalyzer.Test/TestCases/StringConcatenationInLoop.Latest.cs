using System.Threading.Tasks;
using System;

namespace CSharp11
{
    public class StringConcatenationInLoop
    {
        public StringConcatenationInLoop()
        {
            string s = "";
            for (int i = 0; i < 50; i++)
            {
                var sLoop = "";

                s = s + $"""{5}a""" + """b""";  // Noncompliant
                s += """a""";     // Noncompliant {{Use a StringBuilder instead.}}
                sLoop += """a"""; // Compliant

                i += 5;
            }
        }
    }
}

namespace CSharp13
{
    public partial class Partial
    {
        public partial string MyString { get; set; }
    }

    public partial class Partial {
        public partial string MyString
        {
            get => "a";
            set => MyString = value;
        }
    }

    public class MyClass
    {
        async Task TaskWhenEach()
        {
            string s = "";
            Task task1 = Task.Delay(100);
            Task task2 = Task.Delay(100);
            Task task3 = Task.Delay(100);

            await foreach (Task t in Task.WhenEach(task1, task2, task3))
            {
                s += "A"; // Noncompliant
            }
        }

        // https://sonarsource.atlassian.net/browse/NET-442
        void PartialProperties()
        {
            Partial partial = new Partial();
            for (int i = 0; i < 50; i++)
            {
                partial.MyString += "A";                   // FN
                partial.MyString = partial.MyString + "A"; // FN
            }
        }
    }
}
