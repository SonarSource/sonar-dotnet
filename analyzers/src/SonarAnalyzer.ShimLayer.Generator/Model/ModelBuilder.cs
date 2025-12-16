/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
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
    public static StrategyModel Build(TypeDescriptor[] latest, TypeDescriptor[] baseline)
    {
        var baselineMap = baseline.ToDictionary(x => x.Type.FullName, x => x);
        return new(latest.ToDictionary(x => x.Type, x => CreateStrategy(x, baselineMap.TryGetValue(x.Type.FullName, out var baselineType) ? baselineType : null, baselineMap)));
    }

    private static Strategy CreateStrategy(TypeDescriptor latest, TypeDescriptor baseline, IReadOnlyDictionary<string, TypeDescriptor> baselineMap)
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
        else if (IsAssignableTo(latest.Type, "Microsoft.CodeAnalysis.SyntaxNode"))
        {
            var members = CreateMembers(latest, baseline);
            return baseline is null
                ? new SyntaxNodeWrapStrategy(latest.Type, FindCommonBaseType(latest, baselineMap), members)
                : new SyntaxNodeExtendStrategy(latest.Type, members);
        }
        else if (IsAssignableTo(latest.Type, "Microsoft.CodeAnalysis.IOperation"))
        {
            return new IOperationStrategy(latest.Type, CreateMembers(latest, baseline));
        }
        // ToDo: TypeStrategy, or ClassStrategy / StructStrategy / InterfaceStrategy?
        else
        {
            // ToDo: Throw NotSupportedException instead of skip, there should be nothing left after explicitly handling all known cases
            return baseline is null
                ? new SkipStrategy(latest.Type)
                : new NoChangeStrategy(latest.Type);
        }
    }

    private static Type FindCommonBaseType(TypeDescriptor latest, IReadOnlyDictionary<string, TypeDescriptor> baselineMap)
    {
        var current = latest.Type;
        while (current is not null)
        {
            if (baselineMap.ContainsKey(current.FullName))
            {
                return current;
            }
            current = current.BaseType;
        }
        return null;
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
        return latestType.Members.Where(IsValid).Select(x => new MemberDescriptor(x, baseline.Contains(x.ToString()))).ToArray();
    }

    private static FieldInfo[] CreateEnumFields(TypeDescriptor latestType, TypeDescriptor baselineType)
    {
        var baseline = new HashSet<string>(baselineType?.Members.OfType<FieldInfo>().Select(x => x.Name) ?? []);
        return latestType.Members.OfType<FieldInfo>().Where(x => !x.IsSpecialName && !baseline.Contains(x.Name)).ToArray();
    }

    private static bool IsSkipped(Type type) =>
        type.IsNested
        || type.IsGenericType
        || typeof(Delegate).IsAssignableFrom(type);

    private static bool IsValid(MemberInfo member) =>
        member switch
        {
            MethodInfo method => !method.IsSpecialName && !(method.Name is nameof(GetType) or nameof(Equals) or nameof(GetHashCode) or nameof(ToString)),   // Struct methods that would need override
            PropertyInfo => true,
            _ => false
        };
}
