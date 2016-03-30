using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace UDPServer
{
    class UDPServer
    {
        static void Main(string[] args)
        {
            Socket s = null;
            int port = 9999;
            IPEndPoint iep = new IPEndPoint(IPAddress.Loopback, port);

            List<EndPoint> asiakkaat = new List<EndPoint>();

            try
            {
                s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                s.Bind(iep);
            }
            catch (Exception e)
            {
                Console.WriteLine("Virhe: {0}", e.Message);
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Odotetaan asiakasta");

            //Server

            while (!Console.KeyAvailable)
            {
                byte[] rec = new byte[256];
                IPEndPoint asiakas = new IPEndPoint(IPAddress.Any, 0);
                EndPoint remote = (EndPoint)asiakas;
                int received = s.ReceiveFrom(rec, ref remote);

                String rec_string = System.Text.Encoding.Default.GetString(rec, 0, received);
                char[] delim = { ';' };
                String[] palat = rec_string.Split(delim, 2);
                if (palat.Length < 2)
                {
                    s.SendTo(Encoding.ASCII.GetBytes("Virhe, viesti tule kirjoittaa seuraavasti: nimi;teksti"), remote);
                }
                else 
                {
                    if (!asiakkaat.Contains(remote))
                    {
                        asiakkaat.Add(remote);
                        Console.WriteLine("Uusi asiakas: [{0}:{1}]", ((IPEndPoint)remote).Address, ((IPEndPoint)remote).Port);
                    }
                    Console.WriteLine("{0}:{1}", palat[0], palat[1]);

                    foreach (EndPoint client in asiakkaat)
                    {
                        s.SendTo(Encoding.ASCII.GetBytes(rec_string), client);
                    }
                }
            }

            Console.ReadKey();
            s.Close();
        }
    }
}
