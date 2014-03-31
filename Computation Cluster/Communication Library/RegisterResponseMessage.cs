using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
        public TimeSpan Timeout { get; set; }
    }
}
