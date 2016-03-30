using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Gamer
{
    class GameClient
    {
        public static String[] Receiver(Socket server, EndPoint ep)
        {
            try
            {
                int numberFromServer = 0;
                byte[] rec_bytes = new byte[256];
                try
                {
                    numberFromServer = server.ReceiveFrom(rec_bytes, ref ep);
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("Out of data. The game will be terminated");
                    Console.WriteLine(ane);
                    Sender(server, ep, "ACK 500");
                }

                String receivedString = Encoding.ASCII.GetString(rec_bytes, 0, numberFromServer);
                char[] delimiter = {' '};
                String[] pieces = receivedString.Split(delimiter, 10);
                return pieces;
            }
            catch (SocketException se)
            {
                Console.WriteLine("Socket error. The game will be terminated");
                Console.WriteLine(se);
                Sender(server, ep, "ACK 500");
                return null;
            }
        }


        public static void Sender(Socket server, EndPoint ep, String sendableData)
        {
            server.SendTo(Encoding.ASCII.GetBytes(sendableData), ep);
        }

        static void Main(string[] args)
        {
            Boolean on = true;
            String status = "JOIN";
            String userNumber = " ";
            int numberFromServer = 0;
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipEp = new IPEndPoint(IPAddress.Loopback, 9999);
            EndPoint ep = (IPEndPoint)ipEp;

            Console.Write("Who are you? ");
            String infoCarrier = Console.ReadLine();
            Sender(server, ep, "JOIN " + infoCarrier);

            while (on)
            {
                String[] stringArray = Receiver(server, ep);
                if (stringArray == null)
                {
                    Console.WriteLine("An error occured in receiving the data from server. The game will be terminated");
                    Sender(server, ep, "ACK 500");
                }
                
                switch (status)
                {
                    case "JOIN":
                        if (stringArray[0] != null)
                        {
                            switch (stringArray[0])
                            {
                                case "ACK":

                                    if (stringArray[1] != null)
                                    {
                                        switch (stringArray[1])
                                        {
                                            case "404":
                                                Console.Write("Use JOIN, ACK or DATA and then a mumerical value if necessary. Try again: ");
                                                infoCarrier = Console.ReadLine();
                                                Sender(server, ep, "DATA " + infoCarrier);
                                                break;

                                            case "407":
                                                Console.Write("In the numerical game you should use numbers ;-)");
                                                break;

                                            case "201":
                                                Console.WriteLine("Wait until an opponent joins the game...");
                                                break;

                                            case "202":
                                                Console.WriteLine("Your turn against player " + stringArray[2]);
                                                Console.Write("Enter your nummber (0-10): ");
                                                userNumber = Console.ReadLine();
                                                Sender(server, ep, "DATA " + userNumber);
                                                status = "GAME";
                                                break;

                                            case "203":
                                                Console.WriteLine("It is your opponent's turn, please wait...");
                                                status = "GAME";
                                                break;

                                            default:
                                                Console.WriteLine("ACK " + stringArray[1]);
                                                break;
                                        }
                                    }

                                    break;

                                case "DATA":
                                    try
                                    {
                                        numberFromServer = int.Parse(stringArray[stringArray.Length - 1]);
                                    }
                                    catch (ArgumentNullException ane)
                                    {
                                        Console.WriteLine("The program didn't get a value from server. Program will be terminated with following error:");
                                        Console.WriteLine("********** Error from C# **********");
                                        Console.WriteLine(ane);
                                        Sender(server, ep, "ACK 500");
                                    }
                                    break;

                                case "QUIT":
                                    on = false;
                                    break;

                                default:
                                    break;
                            }
                        }
                        break;

                    case "GAME":
                        if (stringArray[0] != null)
                        {
                            switch (stringArray[0])
                            {
                                case "DATA":
                                    Console.WriteLine("Your opponent's number is " + stringArray[1]);
                                    if (int.Parse(stringArray[stringArray.Length - 1]) != numberFromServer)
                                    {
                                        Sender(server, ep, "ACK 300");
                                        Console.WriteLine("Your turn, please enter a number: ");
                                        userNumber = Console.ReadLine();
                                        Sender(server, ep, "DATA " + userNumber);
                                    }
                                    break;

                                case "ACK":
                                    if (stringArray[1] != null)
                                    {
                                        switch (stringArray[1])
                                        {
                                            case "300":
                                                if (int.Parse(userNumber) != numberFromServer)
                                                {
                                                    Console.WriteLine("Wait! You opponent is thinking...");
                                                }
                                                break;

                                            case "407":
                                                Console.Write("Numbers numbers numbers, try again:  ");
                                                userNumber = Console.ReadLine();
                                                Sender(server, ep, "DATA " + userNumber);
                                                break;

                                            case "400":
                                                Console.WriteLine("Something went wrong. The game will be terminated.");
                                                on = false;
                                                Sender(server, ep, "ACK 500");
                                                break;

                                            case "402":
                                                Console.WriteLine("It is your opponent's turn right now...");
                                                break;

                                            case "404":
                                                Console.WriteLine("In the beginning of your input should be used DATA JOIN or ACK. Try again! ");
                                                infoCarrier = Console.ReadLine();
                                                Sender(server, ep, "DATA " + infoCarrier);
                                                break;

                                            default:
                                                Console.WriteLine("ACK " + stringArray[1]);
                                                break;
                                        }
                                    }
                                    break;

                                case "QUIT":
                                    if (stringArray[1] != null)
                                    {
                                        switch (stringArray[1])
                                        {
                                            case "501":
                                                Console.WriteLine("The winner takes it all (You)!");
                                                on = false;
                                                break;

                                            case "502":
                                                Console.WriteLine("You loose! I hope you will get better luck in next time... A right number is: " + stringArray[stringArray.Length - 1]);
                                                on = false;
                                                break;

                                            default:
                                                Console.WriteLine("QUIT " + stringArray[1] + "Terminating...");
                                                on = false;
                                                break;
                                        }
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }

                        break;

                    default:
                        break;
                }
            }
            on = false;

            if (!on)
            {
                Sender(server, ep, "ACK 500");
                Console.WriteLine("Press any key to terminate.");
                Console.ReadKey();
            }
        }
    }
}
