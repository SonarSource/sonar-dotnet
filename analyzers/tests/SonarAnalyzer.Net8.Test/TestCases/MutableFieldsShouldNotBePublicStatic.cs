//https://github.com/SonarSource/sonar-dotnet/issues/8638
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Repro_8638
{
    public class FrozenCollections
    {
        public static FrozenDictionary<string, string> FrozenDictionaryTest = new Dictionary<string, string>().ToFrozenDictionary(); // Compilant
    }
}
