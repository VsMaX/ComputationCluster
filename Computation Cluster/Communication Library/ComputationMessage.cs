using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication_Library
{
    /// <summary>
    /// Abstract class of messages tranferred by TCP/IP. All child classes might be serialized.
    /// </summary>
    public abstract class ComputationMessage
    {
        public int id;
        public string title;
        public string data;

        public ComputationMessage() { }

    }
}
