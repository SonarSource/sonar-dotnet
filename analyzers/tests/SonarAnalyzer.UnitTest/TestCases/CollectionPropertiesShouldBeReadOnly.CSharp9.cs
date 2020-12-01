using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

var x = 1;

public record Program
{
    private List<string> list;

    public ArrayList NongenericList { get; set; } // Noncompliant {{Make the 'NongenericList' property read-only by removing the property setter or making it private.}}
//                   ^^^^^^^^^^^^^^
    protected ICollection<string> GenericCollectionProtected { get; set; } // Noncompliant

    public ICollection<string> GenericCollectionNoSetAuto { get; }
    public ICollection<string> GenericCollectionInit { get; init; } // Noncompliant FP
}

// Ignore collections marked with DataMember attribute: https://github.com/SonarSource/sonar-dotnet/issues/795
[DataContract]
public record Message
{
    [DataMember]
    public Dictionary<string, string> Properties { get; set; }
}

// Ignore collections marked with Serializable attribute: https://github.com/SonarSource/sonar-dotnet/issues/2762
[Serializable]
public record SerializableMessage
{
    public Dictionary<string, string> Properties { get; set; }
}

public abstract record S4004Base
{
    public abstract IDictionary<object, object> Items { get; set; } // Noncompliant
}

public record S4004Abstract : S4004Base
{
    public override IDictionary<object, object> Items { get; set; } // Compliant enforced by base (https://github.com/SonarSource/sonar-dotnet/issues/2606)
}

public interface IS4004
{
    IDictionary<object, object> Items { get; set; } // Noncompliant
}

public record S4004InterfaceImplicit : IS4004
{
    public IDictionary<object, object> Items { get; set; }  // Compliant enforced by interface (https://github.com/SonarSource/sonar-dotnet/issues/2606)
}
