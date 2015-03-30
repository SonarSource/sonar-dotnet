using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace NSonarQube.RuleDescriptor.RuleDescriptors
{
    public class RuleDescriptor
    {
        public RuleDescriptor()
        {
            Tags = new List<string>();
            Parameters = new List<RuleParameter>();
        }

        [XmlElement("key")]
        public string Key { get; set; }
        [XmlElement("name")]
        public string Title { get; set; }
        [XmlElement("severity")]
        public string Severity { get; set; }
        [XmlElement("cardinality")]
        public string Cardinality { get; set; }

        [XmlIgnore]
        public string Description { get; set; }
        [XmlElement("description")]
        public XmlCDataSection DescriptionCDataSection
        {
            get
            {
                return new XmlDocument().CreateCDataSection(Description);
            }
            set { Description = value == null ? "" : value.Value; }
        }

        [XmlElement("tag")]
        public List<string> Tags { get; set; }

        [XmlElement("param")]
        public List<RuleParameter> Parameters { get; set; }

        [XmlIgnore]
        public bool IsActivatedByDefault { get; set; }
    }
}