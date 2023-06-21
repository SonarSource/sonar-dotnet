using System;

namespace Net7.features
{
    internal class AvoidDateTimeNowForBenchmarking
    {
        public void S6561()
        {
            var start = DateTime.Now;
            MethodToBeBenchmarked();
            Console.WriteLine($"{(DateTime.Now - start).TotalMilliseconds} ms");
        }

        void MethodToBeBenchmarked() { }
    }
}
