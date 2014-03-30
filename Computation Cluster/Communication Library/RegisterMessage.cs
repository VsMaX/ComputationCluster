using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Communication_Library
{
    [Serializable]
    [XmlRoot(ElementName = "Register")]
    public class RegisterMessage : ComputationMessage
    {
        [XmlElement]
        public RegisterType Type { get; set; }
        [XmlArray("SolvableProblems")]
        [XmlArrayItem("ProblemName")]
        public string[] SolvableProblems { get; set; }
        [XmlElement]
        public byte ParallelThreads { get; set; }
    }

    public enum RegisterType
    {
        TaskManager,
        ComputationalNode,
    }
}
