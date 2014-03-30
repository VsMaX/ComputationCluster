using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Communication_Library
{
    [Serializable]
    [XmlRoot(ElementName = "RegisterResponse")]
    public class RegisterResponseMessage : ComputationMessage
    {
        [XmlElement]
        public ulong Id { get; set; }
        [XmlElement]
        public DateTime Timeout { get; set; }
    }
}
