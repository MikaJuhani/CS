using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace SMTPClient
{
    class SMTPClient
    {
        static void Main(string[] args)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                s.Connect("localhost", 25000);
            }
            catch (Exception ex)
            {
                Console.Write("Tapahtui virhe: {0}", ex.Message);
                Console.ReadKey();
                return;
            }

            NetworkStream ns = new NetworkStream(s);
            StreamReader sr = new StreamReader(ns);
            StreamWriter sw = new StreamWriter(ns);

            String eMailMessage = "Kukkuu ja pöö";
            String host = "jyu.fi";
            String mailer = "mika.j.ahonen@student.jyu.fi";
            String testimeiliosoite = "hehheh@jotain.fi";
            String messageFromStream = "";
            Boolean on = true;

            while (on)
            {
                messageFromStream = sr.ReadLine();
                String[] status = messageFromStream.Split(' ');
                Console.WriteLine(messageFromStream);
                switch (status[0])
                { 
                    case "220":
                        Console.WriteLine("Connection establihed and sends a HELO");
                        sw.WriteLine("HELO {0}", host);
                        break;
                    case "500":
                        Console.WriteLine("An error occured and the program will be closed. Press enter");
                        sw.WriteLine("QUIT");
                        on = false;
                        break;
                    case "250":
                        switch (status[1])
                        {
                            case "2.0.0":
                                Console.WriteLine("The message has been sent and it is time to close the session.");
                                sw.WriteLine("QUIT");
                                on = false;
                                break;
                            case "2.1.0":
                                Console.WriteLine("Submits a recipient.");
                                sw.WriteLine("RCPT TO: {0}", testimeiliosoite);
                                break;
                            case "2.1.5":
                                Console.WriteLine("Waiting a message body.");
                                sw.WriteLine("DATA");
                                break;
                            default:
                                Console.WriteLine("Submits a sender. ");
                                sw.WriteLine("MAIL FROM: {0}", mailer);
                                break;
                        }
                        break;
                    case "354":
                        Console.WriteLine("Kirjoita viesti:");
                        eMailMessage = Console.ReadLine();
                        sw.WriteLine(eMailMessage);
                        sw.WriteLine("\r\n.\r\n");
                        break;
                    case "221":
                        Console.WriteLine("Istunto on päättynyt. Lähetä meiliä käynnistämällä ohjelma uudelleen.");
                        on = false;
                        break;

                    default:
                        sw.WriteLine("QUIT");
                        break;
                } //switch
                sw.Flush();
            } //while

            Console.WriteLine("Paina mitä tahansa näppäintä sulkeaksesi ohjelman.");
            Console.ReadKey();

            sw.Close();
            sr.Close();
            ns.Close();
        }
    }
}
