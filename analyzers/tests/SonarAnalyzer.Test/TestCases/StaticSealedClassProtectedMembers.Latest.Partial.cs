namespace CSharp13
{
    sealed partial class PartialPropertyClass
    {
        protected partial int PartialProperty // Noncompliant
        {
            get;
            set;
        }
        public partial int OtherPartialProperty
        {
            get;
            set;
        }
        public partial int this[int index] { get { return 42; } }
        protected partial int this[string index] { get { return 42; } } // Noncompliant
    }
}
