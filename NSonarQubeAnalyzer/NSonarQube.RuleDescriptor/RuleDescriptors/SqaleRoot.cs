using System.Collections.Generic;
using System.Xml.Serialization;

namespace NSonarQube.RuleDescriptor.RuleDescriptors
{
    [XmlRoot("sqale", Namespace = "")]
    public class SqaleRoot
    {
        public SqaleRoot()
        {
            Sqale = new List<SqaleDescriptor>();
        }
        [XmlArray("chc")]
        public List<SqaleDescriptor> Sqale { get; set; }
    }
}