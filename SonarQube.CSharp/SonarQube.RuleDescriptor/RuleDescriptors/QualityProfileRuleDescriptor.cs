using System.Xml.Serialization;

namespace SonarQube.RuleDescriptor.RuleDescriptors
{
    [XmlType("rule")]
    public class QualityProfileRuleDescriptor
    {
        public QualityProfileRuleDescriptor()
        {
            RepositoryKey = "csharpsquid";
        }
        [XmlElement("repositoryKey")]
        public string RepositoryKey { get; set; }
        [XmlElement("key")]
        public string Key { get; set; }
    }
}