namespace Tests.Diagnostics
{
    public class ExpressionBodyTest
    {
        private const bool TRUE = true;
        private bool myBool;

        public bool Method(bool b) => TRUE || b; // Noncompliant
                                                 // Secondary@-1

        public bool Prop => TRUE || myBool; // Noncompliant
                                            // Secondary@-1

        public bool OtherProp
        {
            get => TRUE || myBool; // Noncompliant
                                   // Secondary@-1
        }
    }
}
