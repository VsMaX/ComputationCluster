using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Communication_Library
{
    [Serializable]
    [XmlRoot(ElementName = "SolveRequestResponse")]
    public class SolveRequestResponseMessage : ComputationMessage
    {
        [XmlElement]
        public ulong Id { get; set; }
    }
}
