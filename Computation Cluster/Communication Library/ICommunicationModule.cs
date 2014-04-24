using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Communication_Library
{
    public interface ICommunicationModule
    {
        Socket SetupServer();
        Socket SetupClient();
        void Connect(Socket socket);
        void CloseSocket(Socket socket);
        void SendData(string str, Socket socket);
        string ReceiveData(Socket socket);
        Socket Accept(Socket socket);
    }
}
