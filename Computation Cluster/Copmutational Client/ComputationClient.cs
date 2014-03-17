using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communication_Library;
using SuperSocket.SocketBase;

namespace Copmutational_Client
{
    public class ComputationClient
    {

        public ComputationClient()
        {
            
        }

        public void SendProblemRequest(SolveRequestMessage problemRequestMessage)
        {

        }

        public void Connect(string ip)
        {
            AppSession appSession = new AppSession();
            throw new NotImplementedException();
        }

        public void SendSolveRequest(SolveRequestMessage problemRequest)
        {
            throw new NotImplementedException();
        }
    }
}
