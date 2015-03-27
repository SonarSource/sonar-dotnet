namespace NSonarQubeAnalyzer.Diagnostics.SonarProperties.Sqale
{
    public class SqaleConstantRemediationAttribute : SqaleRemediationAttribute
    {
        public string Value { get; set; }

        public SqaleConstantRemediationAttribute(string value)
        {
            Value = value;
        }
    }
}