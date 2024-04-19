using System.Threading;

public class Sample
{
    public void SuggestedNames(SyntaxNode node, SyntaxTree tree, SemanticModel model, CancellationToken cancel) { }
    public void OtherNames(SyntaxNode song, SyntaxTree wood, SemanticModel sculpture, CancellationToken nuke) { }

    public void LongName1(SyntaxNode syntaxNode) { }     // Noncompliant {{Use short name 'node'.}}
    //                               ^^^^^^^^^^
    public void LongName2(SyntaxNode prefixedSyntaxNode) { }         // Noncompliant {{Use short name 'prefixedNode'.}}
    public void LongName3(SyntaxNode syntaxNodeCount) { }            // Noncompliant {{Use short name 'nodeCount'.}}
    public void LongName4(SyntaxTree syntaxTree) { }                 // Noncompliant {{Use short name 'tree'.}}
    public void LongName5(SyntaxTree firstSyntaxTreeCount) { }       // Noncompliant {{Use short name 'firstTreeCount'.}}
    public void LongName6(CancellationToken cancellationToken) { }   // Noncompliant {{Use short name 'cancel'.}}

    private SyntaxNode node;
    private SyntaxNode otherNode, syntaxNode;   // Noncompliant {{Use short name 'node'.}}
    //                            ^^^^^^^^^^
    private SyntaxTree syntaxTree;              // Noncompliant
    private SemanticModel semanticModel;        // Noncompliant

    public SyntaxNode SyntaxNode { get; }       // Noncompliant {{Use short name 'Node'.}}
    //                ^^^^^^^^^^

    public void TypedDeclarations()
    {
        SyntaxNode nodeNode;
        SyntaxNode otherNode;
        SyntaxNode node;
        SyntaxNode syntaxNode;  // Noncompliant {{Use short name 'node'.}} If there exist 'node' and 'syntaxNode' in the same scope, both need a rename.

        SyntaxTree syntaxTree;                  // Noncompliant
        SemanticModel semanticModel;            // Noncompliant
        CancellationToken cancellationToken;    // Noncompliant

        void SyntaxNode() { }   // Not in the scope (for now)
    }

    public void VarDeclarations()
    {
        var nodeNode = CreateNode();
        var otherNode = CreateNode();
        var node = CreateNode();
        var syntaxNode = CreateNode();  // Noncompliant {{Use short name 'node'.}} If there exist 'node' and 'syntaxNode' in the same scope, both need a rename.
        //  ^^^^^^^^^^

        var syntaxTree = CreateTree();          // Noncompliant
        var semanticModel = CreateModel();      // Noncompliant
        var cancellationToken = CreateCancel(); // Noncompliant

        void SyntaxNode() { }   // Not in the scope (for now)
    }

    public void UnexpectedType(SyntaxNode syntaxTree)   // Wrong, but compliant
    {
        var semanticModel = CreateNode();               // Wrong, but compliant
        SemanticModel syntaxNode = null;                // Wrong, but compliant
    }

    private SyntaxNode CreateNode() => null;
    private SyntaxTree CreateTree() => null;
    private SemanticModel CreateModel() => null;
    private CancellationToken CreateCancel() => default;

    private class NestedWithPublicFields
    {
        public SyntaxNode SyntaxNode;       // Noncompliant {{Use short name 'Node'.}}
        public SyntaxTree SyntaxTree;       // Noncompliant
        public SemanticModel SemanticModel; // Noncompliant
    }
}

public class ArrowProperty
{
    public SyntaxNode SyntaxNode => null;   // Noncompliant
}

public class BodyProperty
{
    public SyntaxNode SyntaxNode            // Noncompliant
    {
        get => null;
        set { }
    }
}

public class Methods
{
    // It does not appy to method names
    public void SyntaxNode() { }
    public void SyntaxTree() { }
    public void SemanticModel() { }
    public void CancellationToken() { }
}

public abstract class Base
{
    protected abstract void DoSomething(SyntaxNode syntaxNode); // Noncompliant {{Use short name 'node'.}}
}

public class Inherited : Base
{
    protected override void DoSomething(SyntaxNode syntaxNode) // Compliant so we don't contradict S927
    {
    }
}

public interface IInterface
{
    void DoSomething(SyntaxNode syntaxNode);    // Noncompliant
}

public class Implemented : IInterface
{
    public void DoSomething(SyntaxNode syntaxNode) // Compliant so we don't contradict S927
    {
    }
}

public partial class Partial
{
    public partial void DoSomething(SyntaxNode syntaxNode); // Noncompliant
}

public partial class Partial
{
    public partial void DoSomething(SyntaxNode syntaxNode) // Compliant so we don't contradict S927
    {
        // Implementation
    }
}

public class SyntaxNode { }
public class SyntaxTree { }
public class SemanticModel { }
