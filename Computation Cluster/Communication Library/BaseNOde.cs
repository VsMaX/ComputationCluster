using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using log4net;

namespace Communication_Library
{
    public class BaseNode
    {
        protected static readonly ILog _logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected string SerializeMessage<T>(T message) where T : ComputationMessage
        {
            var serializer = new ComputationSerializer<T>();
            try
            {
                return serializer.Serialize(message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                return String.Empty;
            }
        }

        protected T DeserializeMessage<T>(string message) where T : ComputationMessage
        {
            var serializer = new ComputationSerializer<T>();
            try
            {
                return serializer.Deserialize(message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
                return null;
            }
        }

        protected virtual string GetMessageName(string message)
        {
            var doc = new XmlDocument();
            try
            {
                doc.LoadXml(message);
            }
            catch (Exception ex)
            {
                _logger.Error("Error parsing xml document: " + message + "exception: " + ex.ToString());
                return String.Empty;
            }
            XmlElement root = doc.DocumentElement;
            return root.Name;
        }
    }
}
