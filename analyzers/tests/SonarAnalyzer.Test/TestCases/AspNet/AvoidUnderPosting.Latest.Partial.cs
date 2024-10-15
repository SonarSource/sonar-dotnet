using System.Text.Json.Serialization;

namespace CSharp13
{
    public partial class PartialPropertyClass
    {
        public partial int PartialProperty  // Noncompliant
        {
            get;
            set;
        }

        public partial int HasAttributePartialProperty // Compliant
        {
            get;
            set;
        }

        [JsonRequired]
        public partial int HasAttributePartialPropertyOther // Compliant
        {
            get;
            set;
        }
    }
}
