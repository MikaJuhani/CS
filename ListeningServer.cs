using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace TCPServer
{
    class ListeningServer
    {
        static void Main(string[] args)
        {
            //Establishing of server and start listening
            Socket Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Loopback, 25000);
            Server.Bind(iep);
            Server.Listen(5);

            Socket Client = Server.Accept();

            IPEndPoint iap = (IPEndPoint)Client.RemoteEndPoint;

            //Checking client parameters
            Console.WriteLine("Remote address is {0} in the port {1}", iap.Address, iap.Port);

            //Handling of streams
            NetworkStream ns = new NetworkStream(Client);
            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns);

            String incomingString = sr.ReadLine();
            sw.WriteLine("Mika's server;" + incomingString);
            sw.Flush();
            
            //Final routines
            Console.ReadKey();

            Client.Close();

            Console.ReadKey();

            Server.Close();
        }
    }
}
