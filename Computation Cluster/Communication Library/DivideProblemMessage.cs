using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Communication_Library
{
    [Serializable]
    [XmlRoot(ElementName = "DivideProblem", Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public class DivideProblemMessage : ComputationMessage
    {
        [XmlElement]
        public string ProblemType { get; set; }
        [XmlElement]
        public ulong Id { get; set; }
        [XmlElement(DataType = "base64Binary")]
        public byte[] Data { get; set; }
        [XmlElement]
        public ulong ComputationalNodes { get; set; }
    }
}
