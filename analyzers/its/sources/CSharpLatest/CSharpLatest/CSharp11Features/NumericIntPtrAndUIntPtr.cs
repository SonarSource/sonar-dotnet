namespace CSharpLatest.CSharp11
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
