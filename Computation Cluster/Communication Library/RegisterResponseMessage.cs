using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Communication_Library
{
    [Serializable]
    [XmlRoot(ElementName = "RegisterResponse", Namespace = "http://www.mini.pw.edu.pl/ucc/")]
    public class RegisterResponseMessage : ComputationMessage
    {
        [XmlElement]
        public ulong Id { get; set; }
        [XmlElement]
        public string Timeout { get; set; }

        /// <summary>
        /// Time parsed from timeout, use this to pass data to Timeout property
        /// </summary>
        [XmlIgnore]
        public TimeSpan TimeoutTimeSpan
        {
            get
            {
                //CultureInfo culture = new CultureInfo("HH:mm:ss");
                TimeSpan ts = new TimeSpan();
                TimeSpan.TryParse(Timeout, out ts);
                
                //........
                return ts;
            }
            set 
            {
                Timeout = value.ToString(@"hh\:mm\:ss");
            }
        }
    }
}
