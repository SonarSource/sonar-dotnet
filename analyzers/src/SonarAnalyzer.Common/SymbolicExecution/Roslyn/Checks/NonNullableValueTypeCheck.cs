using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.Checks
{
    internal class NonNullableValueTypeCheck : SymbolicCheck
    {
        protected override ProgramState PreProcessSimple(SymbolicContext context)
        {
            var operation = context.Operation.Instance;
            if (IPropertyReferenceOperationWrapper.IsInstance(operation)
                && IPropertyReferenceOperationWrapper.FromOperation(operation) is var wrapper
                && wrapper.Type.IsNonNullableValueType())
            {
                return context.SetOperationConstraint(ObjectConstraint.NotNull);
            }
            return base.PreProcessSimple(context);
        }
    }
}
