using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Communication_Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ComputationTests
{
    [TestClass]
    public class TestSerializeDeserialize
    {
        private XmlSerializer serializer;

        public TestSerializeDeserialize()
        {
            serializer = new XmlSerializer(typeof(SolveRequest));
        }

        [TestMethod]
        public void Serialize(SolveRequest objToString)
        {
            var stringWriter = new Utf8StringWriter();
            using (var writer = XmlWriter.Create(stringWriter))
            {
                serializer.Serialize(writer, objToString);
            }
            string result = stringWriter.ToString();
            Assert.Fail();
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
