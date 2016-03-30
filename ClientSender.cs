using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TCPClient
{
    class ClientSender
    {
        static void Main(string[] args)
        {
            int MAXRECDATA = 2048;
            byte[] incomingData = new byte[MAXRECDATA];

            try {
                // Initialize a server connection through the socket
                IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 25000);
                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

                // Establish a server connection through the socket
                try {
                    sender.Connect(serverEP);
                    Console.WriteLine("Server IP is {0} in the port {1}", serverEP.Address, serverEP.Port);

                    // String to send
                    byte[] myMessageToRemoteHost = Encoding.Default.GetBytes("Kirjoitan tähän nyt mitä huvittaa ja hip hei hurraa \r\n");

                    // Send the string
                    sender.Send(myMessageToRemoteHost);

                    // Get the remote string
                    int bytesRec = sender.Receive(incomingData);
                    String someFunnyString = Encoding.Default.GetString(incomingData, 0, bytesRec);

                    // Handling with strings
                    String stringA = "";
                    String stringB = "";
                    int idxOfDelimiter = someFunnyString.IndexOf(";");
                    if (idxOfDelimiter > 0)
                    {
                        stringA = someFunnyString.Substring(0, idxOfDelimiter);
                        Console.WriteLine("You have got an answer from: " + stringA);
                    }
                    else {
                        Console.WriteLine("Server didn't like to reveal it's identity...");
                    }
                    
                    stringB = someFunnyString.Substring(someFunnyString.LastIndexOf(";")+1);
                    Console.WriteLine("And it echoes: " + stringB);

                    //Terminating the connection
                    Console.ReadKey();
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                
                } catch (SocketException se) {
                    Console.WriteLine("SocketException: {0}", se.ToString());
                } catch (Exception ue) {
                    Console.WriteLine("Some unknown exception: {0}", ue.ToString());
                }

            } catch (Exception e) {
                Console.WriteLine( e.ToString());
            }
        }
    }
}
