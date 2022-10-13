namespace Net7.features
{
    internal class NumericIntPtrAndUIntPtr
    {
        public void Method()
        {
            nint intPtr = IntPtr.Zero;
            nuint uintPtr  = UIntPtr.Zero;

            IntPtr x = intPtr;
            UIntPtr y = uintPtr;
        }
    }   
}
