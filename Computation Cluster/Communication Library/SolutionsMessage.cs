using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Communication_Library
{
    [Serializable]
    [XmlRoot(ElementName = "Solutions")]
    public class SolutionsMessage : ComputationMessage
    {
        [XmlElement]
        public string ProblemType { get; set; }
        [XmlElement]
        public ulong Id { get; set; }
        [XmlElement(DataType = "base64Binary")]
        public byte[] CommonData { get; set; }
        [XmlArray("SolutionsSolution")]
        [XmlArrayItem("Solution")]
        public Solution[] Solutions { get; set; }
    }

    [Serializable]
    public class Solution
    {
        public ulong TaskId { get; set; }
        public bool TaskIdSpecified { get; set; }
        public bool TimeoutOccured { get; set; }
        public SolutionType Type { get; set; }
        public ulong ComputationsTime { get; set; }
        public byte[] Data { get; set; }
    }

    public enum SolutionType
    {
        Ongoing,
        Partial,
        Final,
    }
}
