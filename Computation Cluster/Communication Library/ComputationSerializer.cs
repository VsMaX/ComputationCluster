using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Communication_Library
{
    public class ComputationSerializer<T> where T : ComputationMessage
    {
        private XmlSerializer serializer;

        public ComputationSerializer()
        {
            serializer = new XmlSerializer(typeof(T));
        }

        public string Serialize(T computationMessage)
        {
            string serializedObject = "";
            using (var sw = new StreamWriter(serializedObject))
            {
                serializer.Serialize(sw, computationMessage);
            }
            return serializedObject;
        }

        public T Deserialize(string computationObjectString)
        {
            T deserializedObject = null;
            using (var sr = new StreamReader(computationObjectString))
            {
                deserializedObject = (T)serializer.Deserialize(sr);
            }
            return deserializedObject;
        }
    }
}
