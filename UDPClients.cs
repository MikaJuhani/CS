using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace UDPClient
{
    class UDPClient
    {
        static void Main(string[] args)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            int port = 9999;

            IPEndPoint iep = new IPEndPoint(IPAddress.Loopback, port);
            byte[] rec = new byte[256];

            EndPoint ep = (EndPoint)iep;
            s.ReceiveTimeout = 1000;
            String msg;
            Boolean on = true;

            do
            {
                Console.Write(">");
                msg = Console.ReadLine();
                if (msg.Equals("q"))
                {
                    on = false;
                }
                else
                {
                    //s.SendTo(System.Text.Encoding.Default.GetString((byte[])msg), ep);
                    s.SendTo(Encoding.ASCII.GetBytes(msg), ep);

                    while (!Console.KeyAvailable)
                    {
                        IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                        EndPoint palvelinep = (EndPoint)remote;
                        int paljon = 0;

                        try
                        {
                            paljon = s.ReceiveFrom(rec, ref palvelinep);
                            String rec_string = System.Text.Encoding.Default.GetString(rec, 0, paljon);
                            char[] delim = { ';' };
                            String[] palat = rec_string.Split(delim, 2);
                            if (palat.Length < 2)
                            {
                                return;
                            }
                            else
                            {
                                Console.WriteLine("{0}: {1}", palat[0], palat[1]);
                            }
                         }
                        catch (Exception ex)
                        {
                            //timeout
                            //Console.WriteLine("Virhe: {0}", ex.Message);
                        }
                    }
                }   
            } while (on);
            Console.ReadKey();
            s.Close();
        }
    }
}
