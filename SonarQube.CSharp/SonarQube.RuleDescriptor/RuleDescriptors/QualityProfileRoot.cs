using System.Collections.Generic;
using System.Xml.Serialization;

namespace SonarQube.RuleDescriptor.RuleDescriptors
{
    [XmlRoot("profile", Namespace = "")]
    public class QualityProfileRoot
    {
        public QualityProfileRoot()
        {
            Rules = new List<QualityProfileRuleDescriptor>();
            Language = "cs";
            Name = "Sonar way";
        }

        [XmlElement("language")]
        public string Language { get; set; }
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlArray("rules")]
        public List<QualityProfileRuleDescriptor> Rules { get; set; }
    }
}