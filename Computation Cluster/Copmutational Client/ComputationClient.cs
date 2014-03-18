using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Communication_Library;
using SuperSocket.SocketBase;

namespace Copmutational_Client
{
    public class ComputationClient
    {
        private Socket clientSocket;
        public ComputationClient()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork , SocketType.Stream, ProtocolType.Tcp);
        }

        public void SendProblemRequest(SolveRequestMessage problemRequestMessage)
        {

        }

        public void Connect(string ip)
        {
            clientSocket.Connect(ip, 5679);
        }

        public void SendSolveRequest(SolveRequestMessage problemRequest)
        {
            byte[] msg = Encoding.Unicode.GetBytes("Test message");
            int bytesSend = clientSocket.Send(msg);
            int bytesRec = 0;
            byte[] bytes = new byte[256];
            
            bytesRec = clientSocket.Receive(bytes);
            Debug.WriteLine(Encoding.UTF8.GetString(bytes));
            clientSocket.Close();
        }
    }
}
