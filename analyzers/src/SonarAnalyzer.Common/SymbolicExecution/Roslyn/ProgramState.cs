/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Text;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    public sealed record ProgramState : IEquatable<ProgramState>
    {
        public static readonly ProgramState Empty = new();
        private readonly ImmutableDictionary<IOperation, SymbolicValue> operationValue;
        private readonly ImmutableDictionary<ISymbol, SymbolicValue> symbolValue;
        private readonly ImmutableDictionary<CaptureId, IOperation> captureOperation;
        private readonly ImmutableHashSet<ISymbol> preservedSymbols;
        private readonly ImmutableStack<ExceptionState> exceptions;

        private ProgramState[] toArray;
        private int? operationValueHashCode;
        private int? symbolValueHashCode;
        private int? captureOperationHashCode;
        private int? preservedSymbolsHashCode;
        private int? exceptionsHashCode;
        private int? hashCode;

        // Current SymbolicValue result of a given operation
        private ImmutableDictionary<IOperation, SymbolicValue> OperationValue { get => operationValue; init => SetCachedHashCodeField(value, ref operationValue, ref operationValueHashCode); }
        private ImmutableDictionary<ISymbol, SymbolicValue> SymbolValue { get => symbolValue; init => SetCachedHashCodeField(value, ref symbolValue, ref symbolValueHashCode); }
        private ImmutableDictionary<int, int> VisitCount { get; init; }
        private ImmutableDictionary<CaptureId, IOperation> CaptureOperation { get => captureOperation; init => SetCachedHashCodeField(value, ref captureOperation, ref captureOperationHashCode); }
        private ImmutableHashSet<ISymbol> PreservedSymbols { get => preservedSymbols; init => SetCachedHashCodeField(value, ref preservedSymbols, ref preservedSymbolsHashCode); }
        private ImmutableStack<ExceptionState> Exceptions { get => exceptions; init => SetCachedHashCodeField(value, ref exceptions, ref exceptionsHashCode); }

        public ExceptionState Exception => Exceptions.IsEmpty ? null : Exceptions.Peek();
        public SymbolicValue this[IOperationWrapperSonar operation] => this[operation.Instance];
        public SymbolicValue this[IOperationWrapper operation] => this[operation.WrappedOperation];
        public SymbolicValue this[IOperation operation] => OperationValue.TryGetValue(ResolveCapture(operation), out var value) ? value : null;
        public SymbolicValue this[ISymbol symbol] => SymbolValue.TryGetValue(symbol, out var value) ? value : null;
        public IOperation this[CaptureId capture] => CaptureOperation.TryGetValue(capture, out var value) ? value : null;

        private ProgramState()
        {
            OperationValue = ImmutableDictionary<IOperation, SymbolicValue>.Empty;
            SymbolValue = ImmutableDictionary<ISymbol, SymbolicValue>.Empty;
            VisitCount = ImmutableDictionary<int, int>.Empty;
            CaptureOperation = ImmutableDictionary<CaptureId, IOperation>.Empty;
            PreservedSymbols = ImmutableHashSet<ISymbol>.Empty;
            Exceptions = ImmutableStack<ExceptionState>.Empty;
        }

        private ProgramState(ProgramState original)   // Custom record override constructor to reset "toArray"
        {
            operationValue = original.operationValue;
            symbolValue = original.symbolValue;
            VisitCount = original.VisitCount;
            captureOperation = original.captureOperation;
            preservedSymbols = original.preservedSymbols;
            exceptions = original.exceptions;
            operationValueHashCode = original.operationValueHashCode;
            symbolValueHashCode = original.symbolValueHashCode;
            captureOperationHashCode = original.captureOperationHashCode;
            preservedSymbolsHashCode = original.preservedSymbolsHashCode;
            exceptionsHashCode = original.exceptionsHashCode;
            hashCode = original.hashCode;
        }

        public ProgramState SetOperationValue(IOperationWrapper operation, SymbolicValue value) =>
            operation is null
                ? throw new ArgumentNullException(nameof(operation))
                : SetOperationValue(operation.WrappedOperation, value);

        public ProgramState SetOperationValue(IOperationWrapperSonar operation, SymbolicValue value) =>
            operation is null
                ? throw new ArgumentNullException(nameof(operation))
                : SetOperationValue(operation.Instance, value);

        public ProgramState SetOperationValue(IOperation operation, SymbolicValue value) =>
            (operation ?? throw new ArgumentNullException(nameof(operation))) is var _
            && value is null
                ? this with { OperationValue = OperationValue.Remove(ResolveCapture(operation)) }
                : this with { OperationValue = OperationValue.SetItem(ResolveCapture(operation), value) };

        public ProgramState SetOperationConstraint(IOperationWrapperSonar operation, SymbolicConstraint constraint) =>
            SetOperationConstraint(operation.Instance, constraint);

        public ProgramState SetOperationConstraint(IOperation operation, SymbolicConstraint constraint) =>
            SetOperationValue(operation, (this[operation] ?? SymbolicValue.Empty).WithConstraint(constraint));

        public ProgramState SetSymbolValue(ISymbol symbol, SymbolicValue value) =>
            value is null
                ? this with { SymbolValue = SymbolValue.Remove(symbol) }
                : this with { SymbolValue = SymbolValue.SetItem(symbol, value) };

        public ProgramState SetSymbolConstraint(ISymbol symbol, SymbolicConstraint constraint) =>
            SetSymbolValue(symbol, (this[symbol] ?? SymbolicValue.Empty).WithConstraint(constraint));

        public ProgramState SetCapture(CaptureId capture, IOperation operation) =>
            this with { CaptureOperation = CaptureOperation.SetItem(capture, operation) };

        public ProgramState SetException(ExceptionState exception) =>
            this with { Exceptions = ImmutableStack<ExceptionState>.Empty.Push(exception) };

        public ProgramState PushException(ExceptionState exception) =>
            this with { Exceptions = Exceptions.Push(exception) };

        public ProgramState PopException() =>
            this with { Exceptions = Exceptions.Pop() };

        public IEnumerable<ISymbol> SymbolsWith(SymbolicConstraint constraint) =>
            SymbolValue.Where(x => x.Value != null && x.Value.HasConstraint(constraint)).Select(x => x.Key);

        public ProgramState ResetOperations()
        {
            var captured = CaptureOperation.Values.ToHashSet(); // Preserve only captured
            return this with { OperationValue = OperationValue.Where(x => captured.Contains(x.Key)).ToImmutableDictionary() };
        }

        public ProgramState RemoveCapture(CaptureId capture) =>
            this with { CaptureOperation = CaptureOperation.Remove(capture) };

        public IOperation ResolveCapture(IOperation operation) =>
            operation?.Kind == OperationKindEx.FlowCaptureReference
            && this[IFlowCaptureReferenceOperationWrapper.FromOperation(operation).Id] is { } captured
                ? captured
                : operation;

        public ProgramState RemoveSymbols(Func<ISymbol, bool> remove) =>
            this with { SymbolValue = SymbolValue.Where(kv => PreservedSymbols.Contains(kv.Key) || !remove(kv.Key)).ToImmutableDictionary() };

        public ProgramState ResetFieldConstraints()
            => ResetFieldConstraints(field => true);

        public ProgramState ResetStaticFieldConstraints(INamedTypeSymbol containingType)
            => ResetFieldConstraints(field => field.IsStatic && containingType.DerivesFrom(field.ContainingType));

        public ProgramState AddVisit(int programPointHash) =>
            this with { VisitCount = VisitCount.SetItem(programPointHash, GetVisitCount(programPointHash) + 1) };

        public ProgramState Preserve(ISymbol symbol) =>
            this with { PreservedSymbols = PreservedSymbols.Add(symbol) };

        public int GetVisitCount(int programPointHash) =>
            VisitCount.TryGetValue(programPointHash, out var count) ? count : 0;

        public override int GetHashCode() =>
            // VisitCount is not included, it's not part of Equals
            hashCode ??= HashCode.Combine(
                operationValueHashCode ??= HashCode.DictionaryContentHash(OperationValue),
                symbolValueHashCode ??= HashCode.DictionaryContentHash(SymbolValue),
                captureOperationHashCode ??= HashCode.DictionaryContentHash(CaptureOperation),
                preservedSymbolsHashCode ??= HashCode.EnumerableUnorderedContentHash(PreservedSymbols),
                exceptionsHashCode ??= HashCode.EnumerableOrderedContentHash(Exceptions));

        public bool Equals(ProgramState other) =>
            // VisitCount is not compared, two ProgramState are equal if their current state is equal. No matter what historical path led to it.
            other is not null
            && other.OperationValue.DictionaryEquals(OperationValue)
            && other.SymbolValue.DictionaryEquals(SymbolValue)
            && other.CaptureOperation.DictionaryEquals(CaptureOperation)
            && other.PreservedSymbols.SetEquals(PreservedSymbols)
            && other.Exceptions.SequenceEqual(Exceptions);

        public ProgramState[] ToArray() =>
            toArray ??= new[] { this };

        public override string ToString() =>
            Equals(Empty) ? "Empty" + Environment.NewLine : SerializeExceptions() + SerializeSymbols() + SerializeOperations() + SerializeCaptures();

        private string SerializeExceptions() =>
            Exceptions.IsEmpty ? null : Exceptions.JoinStr(string.Empty, x => $"Exception: {x}{Environment.NewLine}");

        private string SerializeSymbols() =>
            Serialize(SymbolValue, "Symbols", x => x.Name, x => x.ToString());

        private string SerializeOperations() =>
            Serialize(OperationValue, "Operations", x => x.Serialize(), x => x.ToString());

        private string SerializeCaptures() =>
            Serialize(CaptureOperation, "Captures", x => "#" + x.GetHashCode(), x => x.Serialize());

        private static string Serialize<TKey, TValue>(ImmutableDictionary<TKey, TValue> dictionary, string title, Func<TKey, string> serializeKey, Func<TValue, string> serializeValue)
        {
            if (dictionary.IsEmpty)
            {
                return null;
            }
            else
            {
                var sb = new StringBuilder();
                sb.Append(title).AppendLine(":");
                foreach (var kvp in dictionary.Select(x => new KeyValuePair<string, string>(serializeKey(x.Key), serializeValue(x.Value))).OrderBy(x => x.Key))
                {
                    sb.Append(kvp.Key).Append(": ").AppendLine(kvp.Value);
                }
                return sb.ToString();
            }
        }

        private ProgramState ResetFieldConstraints(Func<IFieldSymbol, bool> predicate)
        {
            var state = this;
            foreach (var kvp in SymbolValue.Where(x => x.Key is IFieldSymbol field && predicate(field)))
            {
                var symbolValue = kvp.Value;
                if (symbolValue.AllConstraints.Where(x => !x.PreserveOnFieldReset).ToArray() is { Length: > 0 } resetConstraints)
                {
                    foreach (var constraint in resetConstraints)
                    {
                        symbolValue = symbolValue.WithoutConstraint(constraint);
                    }
                    state = state.SetSymbolValue(kvp.Key, symbolValue);
                }
            }
            return state;
        }

        private void SetCachedHashCodeField<T>(T value, ref T backingField, ref int? backingFieldHashCode) where T : class
        {
            if (backingField != value)
            {
                backingField = value;
                backingFieldHashCode = null;
                hashCode = null;
            }
        }
    }
}
