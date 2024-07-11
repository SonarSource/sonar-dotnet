using System;

// https://github.com/SonarSource/sonar-dotnet/issues/7522
public class UnityMonoBehaviour : UnityEngine.MonoBehaviour
{
    public int Field1; // Compliant
}

public class UnityScriptableObject : UnityEngine.ScriptableObject
{
    public int Field1; // Compliant
}

public class InvalidCustomSerializableClass1 : UnityEngine.Object
{
    public int Field1; // Noncompliant
}

[Serializable]
public class ValidCustomSerializableClass : UnityEngine.Object
{
    public int Field1; // Compliant
}

// Unity3D does not seem to be available as a nuget package and we cannot use the original classes
// Cannot run this test case in Concurrent mode because of the Concurrent namespace
namespace UnityEngine
{
    public class MonoBehaviour { }
    public class ScriptableObject { }
    public class Object { }
}
