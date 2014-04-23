using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication_Library
{
    public class BaseNode
    {
        protected string SerializeMessage<T>(T message) where T : ComputationMessage
        {
            var serializer = new ComputationSerializer<T>();
            try
            {
                return serializer.Serialize(message);
            }
            catch (Exception ex)
            {
                LogError(ex.ToString());
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
                LogError(ex.ToString());
                return null;
            }
        }

        protected void LogError(string message)
        {
            
        }

        protected void LogError(Exception ex)
        {
            
        }
    }
}
