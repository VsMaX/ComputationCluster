using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communication_Library
{
    public interface ICommunicationModule
    {
        void StartListening();
        string ReceiveData();
        void SendData(string message);
        void SetupListening();
        void SetupConnecting();
        void Connect();
        void CloseConnection();
    }
}
