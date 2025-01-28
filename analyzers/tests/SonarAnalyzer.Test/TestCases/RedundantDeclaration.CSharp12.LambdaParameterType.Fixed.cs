using System;
using System.Collections.Generic;

namespace AliasAnyType
{
    using Int = int;
    using IntArray = int[];
    using IntNullable = int?;
    using Point = (int x, int y);
    using ThreeInts = int[3];                   // Error [CS0270]: Array size cannot be specified in a variable declaration
    using IntToIntFunc = Func<int, int>;

    class AClass
    {
        void AliasWithArraySize()
        {
            IntArray a1 = new Int[3] { 1, 2, 3 };  // Fixed
        }

        void AliasWithUnnecessaryType()
        {
            IntToIntFunc f = (i) => i;      // Fixed
        }

        void AliasWithInitializer()
        {
            IntArray a1 = new IntArray { };     // Error [CS8386]: Invalid object creation
            var a2 = new IntArray { };          // Error [CS8386]: Invalid object creation
            int[] a3 = new IntArray();          // Error [CS8386]: Invalid object creation
        }

        void AliasWithEmptyParamsList()
        {
            IntArray a1 = new IntArray();       // Error [CS8386]: Invalid object creation
            var a2 = new IntArray();            // Error [CS8386]: Invalid object creation
            int[] a3 = new IntArray();          // Error [CS8386]: Invalid object creation
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8115
namespace Repro_8115
{
    class CollectionExpressions
    {
        void ExplicitTypeDeclaration()
        {
            int[] a1 = [1, 2, 3];         // Compliant
            a1 = [1, 2, 3];               // Compliant, reassignment
            int[] a2 = new[] { 1, 2, 3 }; // FN, can be written as [1, 2, 3]
            a2 = new[] { 1, 2, 3 };       // FN, can be written as [1, 2, 3], reassignment
        }

        void VarDeclarationWithInlineAssignment()
        {
            var invalid = [1, 2, 3];            // Error [CS9176] There is no target type for the collection expression.
        }

        void VarDeclarationWithReassignment()
        {
            var typeInferredAndReassigned = new[] { 1, 2, 3 }; // Compliant, cannot be written as [1, 2, 3]
            typeInferredAndReassigned = new[] { 1, 2, 3 };     // FN, can be written as [1, 2, 3], reassignment of a type-inferred variable
            typeInferredAndReassigned = new int[] { 1, 2, 3 }; // Fixed
            typeInferredAndReassigned = [];                    // Compliant
            typeInferredAndReassigned = new int[] { };         // FN, can be written as []
        }

        void VarDeclarationWithReassignmentToEmptyCollection()
        {
            var typeInferredAndReassigned = new[] { 1, 2, 3 };
            typeInferredAndReassigned = new[] { };             // Error [CS0826] No best type found for implicitly-typed array
        }
    }
}

// https://sonarsource.atlassian.net/browse/NET-882
namespace ReproNET882
{
    public class ReproNet882
    {
        public void Method()
        {
            // In this example Action<object, RoutedEventArgs> is down-casted by AddHandler to EventHandler  (EventHandler has the exact same definition as the Action but they are different).
            AddHandler(OnErrorEvent);
            AddHandler(new EventHandler(OnErrorEvent)); // Fixed
        }
        private void OnErrorEvent(object sender, EventArgs e) { }

        public void AddHandler(Delegate handler)
        {
        }
    }
}
