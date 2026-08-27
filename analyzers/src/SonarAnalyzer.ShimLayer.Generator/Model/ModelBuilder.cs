/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.ShimLayer.Generator.Model;

public static class ModelBuilder
{
    private static readonly TypeDescriptor ObjectTypeDescriptor = new(typeof(object), typeof(object).GetMembers());

    public static StrategyModel Build(TypeDescriptor[] latest, TypeDescriptor[] baseline)
    {
        var baselineMap = baseline.ToDictionary(x => x.Type.FullName, x => x);
        var fallbackMap = CreateFallbackBaseTypeMap(latest, baselineMap);
        return new(latest.ToDictionary(x => x.Type, x => CreateStrategy(
                                                            x,
                                                            baselineMap.TryGetValue(x.Type.FullName, out var baselineType) ? baselineType : null,
                                                            fallbackMap.TryGetValue(x.Type.FullName, out var fallbackBaseType) ? fallbackBaseType : null,
                                                            baselineMap)));
    }

    private static Strategy CreateStrategy(TypeDescriptor latest, TypeDescriptor baseline, Type fallbackBaseType, IReadOnlyDictionary<string, TypeDescriptor> baselineMap)
    {
        if (IsSkipped(latest.Type))
        {
            return new SkipStrategy(latest.Type);
        }
        else if (baseline is not null && latest.Members.Select(x => x.ToString()).OrderBy(x => x).SequenceEqual(baseline.Members.Select(x => x.ToString()).OrderBy(x => x)))
        {
            return new NoChangeStrategy(latest.Type);
        }
        else if (latest.Type.IsEnum)
        {
            var fields = CreateEnumFields(latest, baseline);
            return baseline is null
                ? new NewEnumStrategy(latest.Type, fields)
                : new PartialEnumStrategy(latest.Type, fields);
        }
        else if (latest.Type.FullName == "Microsoft.CodeAnalysis.IOperation")
        {
            return new IOperationStrategy(latest.Type, CreateMembers(latest, baseline));
        }
        else if (IsAssignableTo(latest.Type, "Microsoft.CodeAnalysis.SyntaxNode"))
        {
            if (baseline is null)
            {
                var commonBase = FindCommonBaseType(latest, baselineMap);
                return new SyntaxNodeWrapStrategy(latest.Type, commonBase.Type, fallbackBaseType, CreateMembers(latest, commonBase));
            }
            else
            {
                return new ExtendStrategy(latest.Type, CreateMembers(latest, baseline));
            }
        }
        else if (IsAssignableTo(latest.Type, "Microsoft.CodeAnalysis.IOperation"))
        {
            return baseline is null
                ? new OperationWrapStrategy(latest.Type, CreateMembers(latest, baselineMap[typeof(IOperation).FullName]))
                : new ExtendStrategy(latest.Type, CreateMembers(latest, baseline));
        }
        else if (latest.Type.IsInterface)
        {
            return baseline is null
                ? new InterfaceWrapStrategy(latest.Type, typeof(object), CreateMembers(latest, null))
                : new ExtendStrategy(latest.Type, CreateMembers(latest, baseline));
        }
        else if (latest.Type.Name == nameof(Microsoft.CodeAnalysis.FlowAnalysis.CaptureId)) // ToDo: Remove once StructStrategy exists
        {
            return new NoChangeStrategy(latest.Type);
        }
        else if (IsNonStaticClass(latest.Type) && latest.Type.Name is not "SymbolStartAnalysisContext")
        {
            if (baseline is null)
            {
                var commonBase = FindCommonBaseType(latest, baselineMap);
                return new ClassWrapStrategy(latest.Type, commonBase.Type, CreateMembers(latest, commonBase));
            }
            else
            {
                return new ExtendStrategy(latest.Type, CreateMembers(latest, baseline));
            }
        }
        else
        {
            // ToDo: Throw NotSupportedException instead of skip, there should be nothing left after explicitly handling all known cases
            return baseline is null
                ? new SkipStrategy(latest.Type)
                : new NoChangeStrategy(latest.Type);
        }
    }

    private static bool IsNonStaticClass(Type type) =>
        type.IsClass && !(type.IsAbstract && type.IsSealed);

    private static TypeDescriptor FindCommonBaseType(TypeDescriptor latest, IReadOnlyDictionary<string, TypeDescriptor> baselineMap)
    {
        var current = latest.Type;
        while (current is not null)
        {
            if (baselineMap.TryGetValue(current.FullName, out var baselineType))
            {
                return baselineType;
            }
            current = current.BaseType;
        }
        return ObjectTypeDescriptor;
    }

    private static IReadOnlyDictionary<string, Type> CreateFallbackBaseTypeMap(TypeDescriptor[] latestTypes, IReadOnlyDictionary<string, TypeDescriptor> baselineMap)
    {
        var candidates = new Dictionary<string, HashSet<Type>>();
        var syntaxNodeType = latestTypes.Single(x => x.Type.FullName == typeof(SyntaxNode).FullName).Type;
        foreach (var fallback in latestTypes.Select(x => x.Type).Where(x => syntaxNodeType.IsAssignableFrom(x) && baselineMap.ContainsKey(x.FullName)))   // Fallback itself has a basetype
        {
            var current = fallback.BaseType;
            while (current is not null && current != syntaxNodeType)
            {
                if (!baselineMap.ContainsKey(current.FullName))
                {
                    Add(current.FullName, fallback);
                }
                current = current.BaseType;
            }
        }
        return candidates.Where(x => x.Value.Count == 1).ToDictionary(x => x.Key, x => x.Value.Single());

        void Add(string fullName, Type fallback)
        {
            if (!candidates.TryGetValue(fullName, out var candidateTypes))
            {
                candidateTypes = [];
                candidates.Add(fullName, candidateTypes);
            }
            candidateTypes.Add(fallback);
        }
    }

    private static bool IsAssignableTo(Type type, string fullName)   // We can't use typeof(Xxx).IsAssignableFrom(type) because it's loaded into a different metadata context
    {
        if (type.GetInterface(fullName) is not null)
        {
            return true;
        }
        while (type is not null)
        {
            if (type.FullName == fullName)
            {
                return true;
            }
            type = type.BaseType;
        }
        return false;
    }

    private static MemberDescriptor[] CreateMembers(TypeDescriptor latestType, TypeDescriptor baselineType)
    {
        var baseline = new HashSet<string>(baselineType?.Members.Select(x => x.ToString()) ?? []);
        var nonShadowedMembers = latestType.Members.GroupBy(MemberKey).Select(x => x.OrderByDescending(x => InheritanceDepth(x.DeclaringType)).First());
        var names = new Dictionary<string, int>();
        var result = new List<MemberDescriptor>();
        foreach (var member in nonShadowedMembers.Where(IsValid).OrderBy(x => x.Name, StringComparer.Ordinal).ThenBy(x => x.ToString(), StringComparer.Ordinal))
        {
            var nameCount = names.TryGetValue(member.Name, out var oldCount) ? oldCount + 1 : 1;
            names[member.Name] = nameCount;
            var accessorSuffix = nameCount == 1 ? null : $"_Overload{nameCount}";
            result.Add(new MemberDescriptor(member, baseline.Contains(member.ToString()), $"{member.Name}Accessor{accessorSuffix}"));
        }
        return result.ToArray();

        static string MemberKey(MemberInfo member) =>
            member is MethodInfo method
                ? member.Name + ": " + method.GetParameters().JoinStr(", ", x => x.ParameterType.FullName)
                : member.Name;

        static int InheritanceDepth(Type type) =>
            type is null ? 0 : 1 + InheritanceDepth(type.BaseType);
    }

    private static FieldInfo[] CreateEnumFields(TypeDescriptor latestType, TypeDescriptor baselineType)
    {
        // IOperation has changed significantly compared to Roslyn 1.3.2, including changes of values => we need to (re)generate everything
        var baseline = latestType.Type.Name == nameof(OperationKind) ? [] : new HashSet<string>(baselineType?.Members.OfType<FieldInfo>().Select(x => x.Name) ?? []);
        return latestType.Members.OfType<FieldInfo>().Where(x => !x.IsSpecialName && !baseline.Contains(x.Name)).ToArray();
    }

    private static bool IsSkipped(Type type) =>
        type.IsNested
        || type.IsGenericType
        || typeof(Delegate).IsAssignableFrom(type);

    private static bool IsValid(MemberInfo member) =>
        !IsExcluded(member)
        && member switch
        {
            MethodInfo method => !method.IsSpecialName && !(method.Name is nameof(GetType) or nameof(Equals) or nameof(GetHashCode) or nameof(ToString)),   // Struct methods that would need override
            PropertyInfo => true,
            _ => false
        };

    private static bool IsExcluded(MemberInfo member) =>
        member.DeclaringType.Name == nameof(SemanticModel)
        && member.Name is "NullableAnalysisIsDisabled";  // this would have the wrong default (nullable enabled) and is fully covered by GetNullableContext anyway.
}
