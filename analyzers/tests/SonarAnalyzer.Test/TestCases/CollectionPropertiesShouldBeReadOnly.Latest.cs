using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.Serialization;

var x = 1;

public record TestRecord
{
    private List<string> list;

    public ArrayList NongenericList { get; set; } // Noncompliant {{Make the 'NongenericList' property read-only by removing the property setter or making it private.}}
//                   ^^^^^^^^^^^^^^
    protected ICollection<string> GenericCollectionProtected { get; set; } // Noncompliant

    public ICollection<string> GenericCollectionNoSetAuto { get; }
    public ICollection<string> GenericCollectionInit { get; init; }
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

    ICollection<string> CollectionInit { get; init; }
}

public record S4004InterfaceImplicit : IS4004
{
    public IDictionary<object, object> Items { get; set; }  // Compliant enforced by interface (https://github.com/SonarSource/sonar-dotnet/issues/2606)

    public ICollection<string> CollectionInit { get; init; }
}

namespace CSharp11
{
    public interface IS4004
    {
        static abstract IDictionary<object, object> Items { get; set; } // Noncompliant
        static abstract ICollection<string> CollectionInit { get; }
    }

    public class S4004InterfaceImplicit : IS4004
    {
        public static IDictionary<object, object> Items { get; set; }  // Compliant enforced by interface (https://github.com/SonarSource/sonar-dotnet/issues/2606)

        public static ICollection<string> CollectionInit { get; }
    }
}

namespace CSharp13
{
    public class NewCollectionTypes
    {
        public OrderedDictionary<string, string> OrderedDictionary { get; set; } // Noncompliant
        public ReadOnlySet<string> ReadonlySet { get; set; }                     // Noncompliant
    }

    public interface INewCollectionTypes
    {
        public OrderedDictionary<string, string> OrderedDictionary { get; set; } // Noncompliant
        public ReadOnlySet<string> ReadonlySet { get; set; }                     // Noncompliant
    }

    public partial class PartialProperties
    {
        public partial List<string> MyList { get; set; } // Noncompliant
    }

    public partial class PartialProperties
    {
        private List<string> _value = null;

        public partial List<string> MyList { get => _value; set => _value = value;  } // Noncompliant
    }
}
