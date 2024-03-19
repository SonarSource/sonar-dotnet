using System;

namespace IntentionalFindings
{
    internal class S6561
    {
        public void Benchmark()
        {
            var start = DateTime.Now;
            MethodToBeBenchmarked();
            Console.WriteLine($"{(DateTime.Now - start).TotalMilliseconds} ms"); // Noncompliant (S6561)
        }

        void MethodToBeBenchmarked() { }
    }
}
