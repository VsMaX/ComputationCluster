using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Communication_Library
{
    public class TestSerializeDeserialize
    {
        private XmlSerializer serializer;

        public TestSerializeDeserialize()
        {
            serializer = new XmlSerializer(typeof(SolveRequest));
        }

        public string Serialize(SolveRequest objToString)
        {
            var stringWriter = new Utf8StringWriter();
            using (var writer = XmlWriter.Create(stringWriter))
            {
                serializer.Serialize(writer, objToString);
            }
            return stringWriter.ToString();
        }

        public SolveRequest Deserialize(string computationObjectString)
        {

            //var stringReader = new StringReader(computationObjectString);
            
            //using (var reader = XmlReader.Create(stringReader))
            //{
            //    instance = (SolveRequest)serializer.Deserialize(reader, "UTF8");
            //}
            

            //return instance;
            MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(computationObjectString));
            SolveRequest resultingMessage = (SolveRequest)serializer.Deserialize(memStream);

            return resultingMessage;
        }

    }

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
