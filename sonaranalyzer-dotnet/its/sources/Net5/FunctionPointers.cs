namespace Net5
{
    public unsafe class FunctionPointers
    {
        public static void Log() { }

        public void Method()
        {
            delegate*<void> ptr1 = &FunctionPointers.Log;

            // This should work but it does not
            // void* v = &FunctionPointers.Log;
        }
    }
}
