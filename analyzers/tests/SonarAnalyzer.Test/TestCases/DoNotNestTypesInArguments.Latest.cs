using System.Collections.Generic;

namespace Partial
{
    public partial class A
    {
        private partial void M(IDictionary<IList<int>, IList<string>> args) { } // Noncompliant
    }
}

public class B
{
    public void Nullable(Dictionary<List<int>?, string>? args) { } // Noncompliant

    public void Ref(ref ICollection<ICollection<int>> arg) { } // Noncompliant

    public void Ref(ref ICollection<int> arg) { }

    public void In(in ICollection<ICollection<int>> arg) { } // Noncompliant

    public void In(in ICollection<int> arg) { }

    public void Functions()
    {
        static void Func1(ICollection<ICollection<int>> arg) { }; // Noncompliant

        static void Func2(ICollection<int> arg) { };
    }
}

public interface ISome
{
    void M(IList<IList<int>> arg); // Noncompliant
}

public abstract record Some
{
    public abstract void M2(IList<IList<int>> arg); // Noncompliant
} 

public record C : Some, ISome
{
    public void M(IList<IList<int>> arg) { }

    public override void M2(IList<IList<int>> arg) { }
}
