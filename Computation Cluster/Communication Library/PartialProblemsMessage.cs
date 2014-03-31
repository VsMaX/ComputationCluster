using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Communication_Library
{
    [Serializable]
    [XmlRoot(ElementName = "SolvePartialProblems")]
    public class PartialProblemsMessage : ComputationMessage
    {
        [XmlElement]
        public string ProblemType { get; set; }
        [XmlElement]
        public ulong Id { get; set; }
        [XmlElement(DataType = "base64Binary")]
        public byte[] CommonData { get; set; }
        [XmlElement]
        public ulong SolvingTimeout { get; set; }
        [XmlElement]
        public bool SolvingTimeoutSpecified { get; set; }
        [XmlArray("PartialProblems")]
        [XmlArrayItem("PartialProblem")]
        public SolvePartialProblemsPartialProblem[] PartialProblems { get; set; }
    }

    public partial class SolvePartialProblemsPartialProblem
    {
        public ulong TaskId { get; set; }
        public byte[] Data { get; set; }
    }
}
