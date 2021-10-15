namespace Net6
{
    internal class StructuresAsLeftHandOperandsInWithExpressions
    {
        public void Example()
        {
            RecordStruct rs = new RecordStruct(null);

            // Structures as left-hand operands in with expressions
            RecordStruct rs2 = rs with { Property = null };
        }
    }
}
