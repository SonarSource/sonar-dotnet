using System.Collections.Frozen;
using System.Collections.Generic;

// https://github.com/SonarSource/sonar-dotnet/issues/8638
namespace Repro_8638
{
    public class FrozenCollections
    {
        public readonly FrozenDictionary<string, string> FrozenDictionaryTest = new Dictionary<string, string>().ToFrozenDictionary(); // Compilant
        public readonly FrozenSet<string> FrozenSetTest = new HashSet<string>().ToFrozenSet(); // Compilant
    }
}
