// https://github.com/SonarSource/sonar-dotnet/issues/3408
namespace Repro_3408
{
    namespace OuterVarConsumer
    {
        using System; // Compliant, it's needed for tuple deconstruction

        public class Repro3408
        {
            public void Consumer()
            {
                var (_, y) = Provider.ServiceReturningTuples.GetPair();
            }
        }
    }

    namespace InnerVarConsumer
    {
        using System; // Compliant, it's needed for tuple deconstruction

        public class Repro3408
        {
            public void Consumer()
            {
                (var a, var b) = Provider.ServiceReturningTuples.GetPair();
            }
        }
    }

    namespace Provider
    {
        using System;

        public static class ServiceReturningTuples
        {
            public static Tuple<string, int> GetPair() => Tuple.Create("a", 1);
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/3631
namespace Repro_3631
{
    using System.Collections.Generic;

    namespace Tuple_OuterVarConsumer
    {
        using Extensions;   //Noncompliant FP, it's needed for tuple deconstruction

        public class Repro
        {
            public void Go(System.Tuple<string, int> pair)
            {
                var (key, value) = pair;
            }
        }
    }

    namespace Tuple_InnerVarConsumer
    {
        using Extensions;   //Noncompliant FP, it's needed for tuple deconstruction

        public class Repro
        {
            public void Go(System.Tuple<string, int> pair)
            {
                (var key, var value) = pair;
            }
        }
    }

    namespace KVP_OuterVarConsumer
    {
        using Extensions;   //Noncompliant FP, it's needed for tuple deconstruction under .NET Framework

        public class Repro
        {
            public void Go(KeyValuePair<string, int> pair)
            {
                var (key, value) = pair;
            }
        }
    }

    namespace KVP_InnerVarConsumer
    {
        using Extensions;   //Noncompliant FP, it's needed for tuple deconstruction under .NET Framework

        public class Repro
        {
            public void Go(KeyValuePair<string, int> pair)
            {
                (var key, var value) = pair;
            }
        }
    }

    namespace KVP_ForEach
    {
        using Extensions; //Noncompliant FP, it's needed for tuple deconstruction under .NET Framework

        public class Repro
        {
            public void Go(Dictionary<string, int> dict)
            {
                foreach (var (key, value) in dict)
                {
                }
            }
        }
    }

    namespace Extensions
    {
        public static class DeconstructExtensions
        {
            public static void Deconstruct<TKey, TValue>(this System.Tuple<TKey, TValue> kvp, out TKey key, out TValue value)
            {
                key = kvp.Item1;
                value = kvp.Item2;
            }

            public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
            {
                key = kvp.Key;
                value = kvp.Value;
            }
        }
    }
}
