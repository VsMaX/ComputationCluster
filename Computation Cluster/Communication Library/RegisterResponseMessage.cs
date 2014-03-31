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
    [XmlRoot(ElementName = "RegisterResponse")]
    public class RegisterResponseMessage : ComputationMessage
    {
        [XmlElement]
        public ulong Id { get; set; }
        [XmlElement]
        public string Timeout { get; set; }

        [XmlIgnore]
        public TimeSpan Time
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
