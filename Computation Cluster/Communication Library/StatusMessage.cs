using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Communication_Library
{
    [Serializable]
    [XmlRoot(ElementName = "Status", Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public class StatusMessage : ComputationMessage
    {
        public StatusMessage() { }

        public StatusMessage(ulong id)
        {
            this.Id = id;
            this.Threads = new StatusThread[0];
        }

        public StatusMessage(ulong id, StatusThread[] threads)
        {
            this.Id = id;
            this.Threads = threads;
        }

        [XmlElement]
        public ulong Id { get; set; }
        [XmlArray("Threads")]
        [XmlArrayItem("Thread")]
        public StatusThread[] Threads { get; set; }
    }

    public partial class StatusThread
    {

        public StatusThreadState State { get; set; }

        public ulong HowLong { get; set; }

        public ulong ProblemInstanceId { get; set; }

        public ulong TaskId { get; set; }

        public bool TaskIdSpecified { get; set; }

        public string ProblemType { get; set; }
    }

    public enum StatusThreadState
    {
        Idle = 0,
        Busy = 1,
    }
}
