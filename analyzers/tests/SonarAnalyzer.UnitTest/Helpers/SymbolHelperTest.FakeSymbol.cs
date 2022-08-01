using System.Globalization;
using System.Reflection;
using System.Reflection.Metadata;
using System.Threading;

namespace SonarAnalyzer.UnitTest.Helpers
{
    public partial class SymbolHelperTest
    {
#pragma warning disable RS1009 // Only internal implementations of this interface are allowed
        private class FakeSymbol : ISymbol
#pragma warning restore RS1009 // Only internal implementations of this interface are allowed
        {
            public FakeSymbol(SymbolKind symbolKind)
            {
                Kind = symbolKind;
            }
            public SymbolKind Kind { get; private init; }

            #region Not implemented members

            public string Language => throw new NotImplementedException();

            public string Name => throw new NotImplementedException();

            public string MetadataName => throw new NotImplementedException();

            public int MetadataToken => throw new NotImplementedException();

            public ISymbol ContainingSymbol => throw new NotImplementedException();

            public IAssemblySymbol ContainingAssembly => throw new NotImplementedException();

            public IModuleSymbol ContainingModule => throw new NotImplementedException();

            public INamedTypeSymbol ContainingType => throw new NotImplementedException();

            public INamespaceSymbol ContainingNamespace => throw new NotImplementedException();

            public bool IsDefinition => throw new NotImplementedException();

            public bool IsStatic => throw new NotImplementedException();

            public bool IsVirtual => throw new NotImplementedException();

            public bool IsOverride => throw new NotImplementedException();

            public bool IsAbstract => throw new NotImplementedException();

            public bool IsSealed => throw new NotImplementedException();

            public bool IsExtern => throw new NotImplementedException();

            public bool IsImplicitlyDeclared => throw new NotImplementedException();

            public bool CanBeReferencedByName => throw new NotImplementedException();

            public ImmutableArray<Location> Locations => throw new NotImplementedException();

            public ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => throw new NotImplementedException();

            public Microsoft.CodeAnalysis.Accessibility DeclaredAccessibility => throw new NotImplementedException();

            public ISymbol OriginalDefinition => throw new NotImplementedException();

            public bool HasUnsupportedMetadata => throw new NotImplementedException();

            public void Accept(SymbolVisitor visitor) => throw new NotImplementedException();
            public TResult? Accept<TResult>(SymbolVisitor<TResult> visitor) => throw new NotImplementedException();
            public bool Equals(ISymbol other, SymbolEqualityComparer equalityComparer) => throw new NotImplementedException();
            public bool Equals(ISymbol other) => throw new NotImplementedException();
            public ImmutableArray<AttributeData> GetAttributes() => throw new NotImplementedException();
            public string GetDocumentationCommentId() => throw new NotImplementedException();
            public string GetDocumentationCommentXml(CultureInfo preferredCulture = null, bool expandIncludes = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public ImmutableArray<SymbolDisplayPart> ToDisplayParts(SymbolDisplayFormat format = null) => throw new NotImplementedException();
            public string ToDisplayString(SymbolDisplayFormat format = null) => throw new NotImplementedException();
            public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null) => throw new NotImplementedException();
            public string ToMinimalDisplayString(SemanticModel semanticModel, int position, SymbolDisplayFormat format = null) => throw new NotImplementedException();

            #endregion
        }

#pragma warning disable RS1009 // Only internal implementations of this interface are allowed
        private class FakeNamedTypeSymbol : FakeSymbol, INamedTypeSymbol
#pragma warning restore RS1009 // Only internal implementations of this interface are allowed
        {
            public FakeNamedTypeSymbol(TypeKind typeKind) : this(typeKind, isRecord: false)
            {
            }

            public FakeNamedTypeSymbol(TypeKind typeKind, bool isRecord) : base(SymbolKind.NamedType)
            {
                TypeKind = typeKind;
                IsRecord = isRecord;
            }

            public TypeKind TypeKind { get; private init; }
            public bool IsRecord { get; private init; }

            #region Not implemented members
            public int Arity => throw new NotImplementedException();

            public bool IsGenericType => throw new NotImplementedException();

            public bool IsUnboundGenericType => throw new NotImplementedException();

            public bool IsScriptClass => throw new NotImplementedException();

            public bool IsImplicitClass => throw new NotImplementedException();

            public bool IsComImport => throw new NotImplementedException();

            public IEnumerable<string> MemberNames => throw new NotImplementedException();

            public ImmutableArray<ITypeParameterSymbol> TypeParameters => throw new NotImplementedException();

            public ImmutableArray<ITypeSymbol> TypeArguments => throw new NotImplementedException();

            public ImmutableArray<NullableAnnotation> TypeArgumentNullableAnnotations => throw new NotImplementedException();

            public IMethodSymbol DelegateInvokeMethod => throw new NotImplementedException();

            public INamedTypeSymbol EnumUnderlyingType => throw new NotImplementedException();

            public INamedTypeSymbol ConstructedFrom => throw new NotImplementedException();

            public ImmutableArray<IMethodSymbol> InstanceConstructors => throw new NotImplementedException();

            public ImmutableArray<IMethodSymbol> StaticConstructors => throw new NotImplementedException();

            public ImmutableArray<IMethodSymbol> Constructors => throw new NotImplementedException();

            public ISymbol AssociatedSymbol => throw new NotImplementedException();

            public bool MightContainExtensionMethods => throw new NotImplementedException();

            public INamedTypeSymbol TupleUnderlyingType => throw new NotImplementedException();

            public ImmutableArray<IFieldSymbol> TupleElements => throw new NotImplementedException();

            public bool IsSerializable => throw new NotImplementedException();

            public INamedTypeSymbol NativeIntegerUnderlyingType => throw new NotImplementedException();


            public INamedTypeSymbol BaseType => throw new NotImplementedException();

            public ImmutableArray<INamedTypeSymbol> Interfaces => throw new NotImplementedException();

            public ImmutableArray<INamedTypeSymbol> AllInterfaces => throw new NotImplementedException();

            public bool IsReferenceType => throw new NotImplementedException();

            public bool IsValueType => throw new NotImplementedException();

            public bool IsAnonymousType => throw new NotImplementedException();

            public bool IsTupleType => throw new NotImplementedException();

            public bool IsNativeIntegerType => throw new NotImplementedException();

            public SpecialType SpecialType => throw new NotImplementedException();

            public bool IsRefLikeType => throw new NotImplementedException();

            public bool IsUnmanagedType => throw new NotImplementedException();

            public bool IsReadOnly => throw new NotImplementedException();

            public NullableAnnotation NullableAnnotation => throw new NotImplementedException();

            public bool IsNamespace => throw new NotImplementedException();

            public bool IsType => throw new NotImplementedException();

            INamedTypeSymbol INamedTypeSymbol.OriginalDefinition => throw new NotImplementedException();

            ITypeSymbol ITypeSymbol.OriginalDefinition => throw new NotImplementedException();

            public INamedTypeSymbol Construct(params ITypeSymbol[] typeArguments) => throw new NotImplementedException();
            public INamedTypeSymbol Construct(ImmutableArray<ITypeSymbol> typeArguments, ImmutableArray<NullableAnnotation> typeArgumentNullableAnnotations) => throw new NotImplementedException();
            public INamedTypeSymbol ConstructUnboundGenericType() => throw new NotImplementedException();
            public ISymbol FindImplementationForInterfaceMember(ISymbol interfaceMember) => throw new NotImplementedException();
            public ImmutableArray<ISymbol> GetMembers() => throw new NotImplementedException();
            public ImmutableArray<ISymbol> GetMembers(string name) => throw new NotImplementedException();
            public ImmutableArray<CustomModifier> GetTypeArgumentCustomModifiers(int ordinal) => throw new NotImplementedException();
            public ImmutableArray<INamedTypeSymbol> GetTypeMembers() => throw new NotImplementedException();
            public ImmutableArray<INamedTypeSymbol> GetTypeMembers(string name) => throw new NotImplementedException();
            public ImmutableArray<INamedTypeSymbol> GetTypeMembers(string name, int arity) => throw new NotImplementedException();
            public ImmutableArray<SymbolDisplayPart> ToDisplayParts(NullableFlowState topLevelNullability, SymbolDisplayFormat format = null) => throw new NotImplementedException();
            public string ToDisplayString(NullableFlowState topLevelNullability, SymbolDisplayFormat format = null) => throw new NotImplementedException();
            public ImmutableArray<SymbolDisplayPart> ToMinimalDisplayParts(SemanticModel semanticModel, NullableFlowState topLevelNullability, int position, SymbolDisplayFormat format = null) => throw new NotImplementedException();
            public string ToMinimalDisplayString(SemanticModel semanticModel, NullableFlowState topLevelNullability, int position, SymbolDisplayFormat format = null) => throw new NotImplementedException();
            public ITypeSymbol WithNullableAnnotation(NullableAnnotation nullableAnnotation) => throw new NotImplementedException();

            #endregion
        }

#pragma warning disable RS1009 // Only internal implementations of this interface are allowed
        private class FakeMethodSymbol : FakeSymbol, IMethodSymbol
#pragma warning restore RS1009 // Only internal implementations of this interface are allowed
        {
            public FakeMethodSymbol(MethodKind methodKind) : base(SymbolKind.Method)
            {
                MethodKind = methodKind;
            }

            public MethodKind MethodKind { get; private init; }

            #region Not implemented members
            public int Arity => throw new NotImplementedException();

            public bool IsGenericMethod => throw new NotImplementedException();

            public bool IsExtensionMethod => throw new NotImplementedException();

            public bool IsAsync => throw new NotImplementedException();

            public bool IsVararg => throw new NotImplementedException();

            public bool IsCheckedBuiltin => throw new NotImplementedException();

            public bool HidesBaseMethodsByName => throw new NotImplementedException();

            public bool ReturnsVoid => throw new NotImplementedException();

            public bool ReturnsByRef => throw new NotImplementedException();

            public bool ReturnsByRefReadonly => throw new NotImplementedException();

            public RefKind RefKind => throw new NotImplementedException();

            public ITypeSymbol ReturnType => throw new NotImplementedException();

            public NullableAnnotation ReturnNullableAnnotation => throw new NotImplementedException();

            public ImmutableArray<ITypeSymbol> TypeArguments => throw new NotImplementedException();

            public ImmutableArray<NullableAnnotation> TypeArgumentNullableAnnotations => throw new NotImplementedException();

            public ImmutableArray<ITypeParameterSymbol> TypeParameters => throw new NotImplementedException();

            public ImmutableArray<IParameterSymbol> Parameters => throw new NotImplementedException();

            public IMethodSymbol ConstructedFrom => throw new NotImplementedException();

            public bool IsReadOnly => throw new NotImplementedException();

            public bool IsInitOnly => throw new NotImplementedException();

            public IMethodSymbol OverriddenMethod => throw new NotImplementedException();

            public ITypeSymbol ReceiverType => throw new NotImplementedException();

            public NullableAnnotation ReceiverNullableAnnotation => throw new NotImplementedException();

            public IMethodSymbol ReducedFrom => throw new NotImplementedException();

            public ImmutableArray<IMethodSymbol> ExplicitInterfaceImplementations => throw new NotImplementedException();

            public ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => throw new NotImplementedException();

            public ImmutableArray<CustomModifier> RefCustomModifiers => throw new NotImplementedException();

            public SignatureCallingConvention CallingConvention => throw new NotImplementedException();

            public ImmutableArray<INamedTypeSymbol> UnmanagedCallingConventionTypes => throw new NotImplementedException();

            public ISymbol AssociatedSymbol => throw new NotImplementedException();

            public IMethodSymbol PartialDefinitionPart => throw new NotImplementedException();

            public IMethodSymbol PartialImplementationPart => throw new NotImplementedException();

            public MethodImplAttributes MethodImplementationFlags => throw new NotImplementedException();

            public bool IsPartialDefinition => throw new NotImplementedException();

            public INamedTypeSymbol AssociatedAnonymousDelegate => throw new NotImplementedException();

            public bool IsConditional => throw new NotImplementedException();

            IMethodSymbol IMethodSymbol.OriginalDefinition => throw new NotImplementedException();

            public IMethodSymbol Construct(params ITypeSymbol[] typeArguments) => throw new NotImplementedException();
            public IMethodSymbol Construct(ImmutableArray<ITypeSymbol> typeArguments, ImmutableArray<NullableAnnotation> typeArgumentNullableAnnotations) => throw new NotImplementedException();
            public DllImportData GetDllImportData() => throw new NotImplementedException();
            public ImmutableArray<AttributeData> GetReturnTypeAttributes() => throw new NotImplementedException();
            public ITypeSymbol GetTypeInferredDuringReduction(ITypeParameterSymbol reducedFromTypeParameter) => throw new NotImplementedException();
            public IMethodSymbol ReduceExtensionMethod(ITypeSymbol receiverType) => throw new NotImplementedException();

            #endregion
        }
    }
}
