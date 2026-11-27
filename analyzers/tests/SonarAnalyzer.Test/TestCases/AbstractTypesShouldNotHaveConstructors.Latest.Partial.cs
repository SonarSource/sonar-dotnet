namespace CSharp14
{
    public abstract partial class PartialConstructor
    {
        public partial PartialConstructor() { } // Noncompliant
    }

    public abstract partial class NonPartialConstructor
    {

    }
}

namespace CSharp15
{
    public partial class ClosedPartialAcrossFiles
    {
        public ClosedPartialAcrossFiles() { } // Noncompliant
        protected ClosedPartialAcrossFiles(int i) { } // Compliant
    }
}

namespace AbstractPartial
{
    public partial class AbstractPartialAcrossFiles
    {
        public AbstractPartialAcrossFiles() { } // Noncompliant
        protected AbstractPartialAcrossFiles(int i) { } // Compliant
    }
}
