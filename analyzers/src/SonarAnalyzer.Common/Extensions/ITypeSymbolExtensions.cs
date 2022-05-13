using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Extensions
{
    public static class ITypeSymbolExtensions
    {
        public static ImmutableArray<ISymbol> GetAccessibleMembersAndBaseMembers(this ITypeSymbol typeSymbol, SemanticModel model, int position)
        {
            var builder = ImmutableArray.CreateBuilder<ISymbol>();
            AddAccessibleMembers(typeSymbol);
            while ((typeSymbol = typeSymbol.BaseType) is not null)
            {
                AddAccessibleMembers(typeSymbol);
            }
            return builder.ToImmutable();

            void AddAccessibleMembers(ITypeSymbol symbol) =>
                builder.AddRange(symbol.GetMembers().Where(x => model.IsAccessible(position, x)));
        }
    }
}
