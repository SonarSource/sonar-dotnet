using System.Xml.Serialization;

namespace NSonarQube.RuleDescriptor.RuleDescriptors
{
    [XmlType("chc")]
    public class SqaleDescriptor
    {
        public SqaleDescriptor()
        {
            Remediation = new SqaleRemediation();
        }

        [XmlElement("key")]
        public string SubCharacteristic { get; set; }

        [XmlElement("chc")]
        public SqaleRemediation Remediation { get; set; }
    }
}