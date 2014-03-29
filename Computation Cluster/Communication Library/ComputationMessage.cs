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
        private int id;
        private string title;
        private string data;

        public ComputationMessage() { }

    }
}
