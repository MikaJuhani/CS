using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace GameServer
{
    class GameServer
    {
        //Swaps between numbers
        public static int UpSideDown(int i)
        {
            return 1 - i;
        }

        //Sends data to endpoints
        public static void Send(Socket server, EndPoint ep, String streamer)
        {
            Console.WriteLine(ep.ToString() + " " + streamer);
            server.SendTo(Encoding.ASCII.GetBytes(streamer), ep);
        }

        //Receives data from endpoints
        public static String[] Receive(Socket server, ref EndPoint ep)
        {
            int amount = 0;
            byte[] rec_bytes = new byte[256];
            amount = server.ReceiveFrom(rec_bytes, ref ep);
            String rec_string = Encoding.ASCII.GetString(rec_bytes, 0, amount);
            Console.WriteLine(ep.ToString() + " " + rec_string);
            char[] delim = {' '};
            String[] pieces = rec_string.Split(delim, 10);
            return pieces;
        }

        //Offers randomized integers according to values given by arguments
        public static int Randomizer(int firstArg, int secondArg)
        {
            Random randValue = new Random();
            int selectedValue = randValue.Next(firstArg, secondArg);
            return selectedValue;
        }

        //Checks if the given string is numerical
        public static Boolean FormatChecker(String candidate)
        {
            int testValue;
            try
            {
                //formatCheck = false;
                testValue = int.Parse(candidate);
                return true;
            }
            catch (FormatException fe)
            {
                //Writes an error log in server console
                Console.WriteLine("********** Error Stack from C# System **********");
                Console.WriteLine(fe);
                Console.WriteLine("********** ***** ***** **** ** ****** **********");
                return false;
            }
        }

        public static void ThrowErrorStack(Socket srv, EndPoint ep, String strArray)
        {
            try
            {
                //Retrives value and checks if null
                String nullCheckerString = strArray;
                if (strArray != "DATA" || strArray != "JOIN" || strArray != "ACK")
                {
                    Send(srv, ep, "ACK 404 Frame error. Use DATA, JOIN or ACK");
                }
            }
            catch (NullReferenceException nre)
            {
                //Writes error log in server console
                Send(srv, ep, "ACK 400 Unexpected error");
                Console.WriteLine("********** Error Stack from C# System **********");
                Console.WriteLine(nre);
                Console.WriteLine("********** ***** ***** **** ** ****** **********");
            }        
        }

        //Main program
        static void Main(string[] args)
        {
            Socket server;
            IPEndPoint iep = new IPEndPoint(IPAddress.Loopback, 9999);
            try
            {
                server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                server.Bind(iep);
            }
            catch (SocketException se)
            {
                Console.WriteLine("Socket error. The game will be terminated");
                Console.WriteLine(se);
                server = null;
                Console.WriteLine("Connection problem. Server will be terminated. Press any key");
                Console.ReadKey();
            }

            EndPoint[] player = new EndPoint[2];
            String[] name = new String[2];
            
            String status = "WAIT";
            Boolean on = true;

            int quess;
            int turn = -1;
            int players = 0; //Should be two of them
            int number = -1;

            //The loop begins
            while (on)
            {
                Console.WriteLine(status);
                IPEndPoint ipEp = new IPEndPoint(IPAddress.Any, 0);
                EndPoint remote = (EndPoint)(ipEp);
                String[] pieces = Receive(server, ref remote);

                switch (status)
                {
                    case "WAIT":
                        switch (pieces[0])
                        {
                            case "JOIN":
                                player[players] = remote;
                                name[players] = pieces[1];
                                players++;
                                if (players == 1)
                                {
                                    Send(server, player[0], "ACK 201 JOIN OK");
                                }
                                if (players == 2)
                                {
                                    int firstPlayer = Randomizer(0, 1);
                                    turn = firstPlayer;
                                    number = Randomizer(0, 10);
                                    Console.WriteLine("Random number is:  " + number);
                                    Send(server, player[firstPlayer], "DATA " + number.ToString());
                                    Send(server, player[UpSideDown(firstPlayer)], "DATA " + number.ToString());
                                    Send(server, player[firstPlayer], "ACK 202 " + name[UpSideDown(firstPlayer)]);
                                    Send(server, player[UpSideDown(firstPlayer)], "ACK 203 " + name[firstPlayer]);
                                    status = "GAME";
                                }
                                break;

                            default:
                                ThrowErrorStack(server, remote, pieces[0]);
                                break;
                        }
                        break;

                    case "GAME":
                        if (pieces[0] != null)
                        {
                            switch (pieces[0])
                            {
                                case "DATA":

                                    if (FormatChecker(pieces[1]))
                                    {
                                        quess = int.Parse(pieces[1]);
                                        Send(server, player[turn], "ACK 300 DATA OK");
                                        Send(server, player[UpSideDown(turn)], "DATA " + pieces[1]);
                                        turn = UpSideDown(turn);
                                        String winningNumber = number.ToString();
                                        if (pieces[1] == winningNumber)
                                        {
                                            Send(server, player[turn], "QUIT 502 Your opponent won by number " + winningNumber);
                                            Send(server, player[UpSideDown(turn)], "QUIT 501 You win! ");
                                            status = "END";
                                        }
                                        else
                                        {
                                            status = "WAIT_ACK";
                                        }
                                    }
                                    else
                                    {
                                        Send(server, player[turn], "ACK 407 Input error: Type a number: ");
                                    }
                                    break;

                                default:
                                    ThrowErrorStack(server, remote, pieces[0]);
                                    break;
                            }
                        }
                        break;

                    case "WAIT_ACK":
                        if (pieces[0] != null)
                        {
                            switch (pieces[0])
                            {
                                case "ACK":
                                    switch (pieces[1])
                                    {
                                        case "300":
                                            status = "GAME";
                                            break;

                                        default:
                                            break;
                                    }
                                    break;

                                default:
                                    ThrowErrorStack(server, remote, pieces[0]);
                                    break;
                            }               
                        }

                        break;

                    case "END":
                        if(pieces[0] != null)
                        {
                            switch (pieces[0])
                            {
                                case "ACK":
                                    switch (pieces[1])
                                    {
                                        case "500":
                                            on = false;
                                            Console.WriteLine("Press any key to quit");
                                            Console.ReadKey();
                                            break;

                                        default:
                                            break;
                                    }
                                    break;

                                default:
                                    ThrowErrorStack(server, remote, pieces[0]);
                                    break;
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
