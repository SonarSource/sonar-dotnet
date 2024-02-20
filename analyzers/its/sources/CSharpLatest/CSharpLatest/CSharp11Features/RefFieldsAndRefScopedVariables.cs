namespace CSharpLatest.CSharp11Features;

internal class RefFieldsAndRefScopedVariables
{
    public ref struct CustomRef
    {
        public ref bool IsValid;
        public Span<int> Inputs;
        public Span<int> Outputs;
    }


    public readonly ref struct ConversionRequest
    {
        public ConversionRequest(scoped ref double rate, scoped ref ReadOnlySpan<double> values)
        {
            Rate = rate;
            Values = values;
        }

        public double Rate { get; }

        public ReadOnlySpan<double> Values { get; }
    }
}
