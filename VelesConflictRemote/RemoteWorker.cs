using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace VelesConflictRemote
{
    class PhoneConnection
    {
        TcpClient client;
        BufferedStream bufferStream;
        NetworkStream networkStream;
        StreamReader reader;
        public string Name { get; set; }
        public string DeviceId { get; private set; }
        public bool Dead { get; private set; }

        public PhoneConnection(TcpClient client)
        {
            this.client = client;
            networkStream=client.GetStream();
            bufferStream = new BufferedStream(networkStream);
            reader = new StreamReader(bufferStream);
            Thread clientThread = new Thread(ReadThread);
            clientThread.IsBackground = true;
            clientThread.Start();
        }

        void ReadThread()
        {
            while (true)
            {
                try
                {
                    string input = reader.ReadLine();
                    try
                    {
                        if (input[0] == '+')
                        {
                            //Command start
                            string[] Split = input.Split(' ');
                            switch (Split[0])
                            {
                                case "+HELLO":
                                    DeviceId = Split[1];
                                    break;
                            }
                        }
                    }
                    catch
                    {

                    }
                }
                catch(Exception e)
                {
                    Dead = true;
                    break;
                }
            }
        }
        public void Write(string command)
        {
            try
            {
                bufferStream.Write(ASCIIEncoding.ASCII.GetBytes(command), 0, command.Length);
                bufferStream.Flush();
            }
            catch
            {
                Dead = true;
            }
        }
    }
    class RemoteWorker
    {
        TcpListener server;
        List<PhoneConnection> PhoneConnections;

        public RemoteWorker(string[] args)
        {
            server = new TcpListener(IPAddress.Any, 29092);
            PhoneConnections = new List<PhoneConnection>();
        }

        void ParseConsole()
        {
            Console.Write(">");
            string input = Console.ReadLine();
            lock (PhoneConnections)
            {
                for (int i = 0; i < PhoneConnections.Count; i++)
                {
                    if (PhoneConnections[i].Dead)
                    {
                        PhoneConnections.RemoveAt(i--);
                    }
                }
            }
            string[] Split=input.Split(' ');
            switch (Split[0].ToLower())
            {
                case "list":
                    lock (PhoneConnections)
                    {
                        Console.WriteLine("** Connections **");
                        int Index = 0;
                        foreach (PhoneConnection con in PhoneConnections)
                        {
                            Console.WriteLine("{0} {1} {2}", Index++, con.Name == null ? "No Name" : con.Name, con.DeviceId == null ? "No Id" : con.DeviceId);
                        }
                        Console.WriteLine("** Connections **");
                    }
                    break;
                case "name":
                    lock (PhoneConnections)
                    {
                        int Index = int.Parse(Split[1]);
                        PhoneConnections[Index].Name = Split[2];
                    }
                    break;
                case "add":
                    {
                        PhoneConnection connection;
                        if (Split[1].Length == 64)
                        {
                            connection = (from con in PhoneConnections where con.DeviceId == Split[1] select con).FirstOrDefault();
                        }
                        else
                        {
                            connection = (from con in PhoneConnections where con.Name == Split[1] select con).FirstOrDefault();
                        }
                        if (connection == null)
                            return;
                        string Command = string.Format("+ADD {0} {1} {2} {3} {4} {5} {6} {7}\n", Split[2], Split[3], Split[4], Split[5], Split[6], Split[7], Split[8], Split[9]);
                        connection.Write(Command);
                    }
                    break;
                case "update":
                    {
                        PhoneConnection connection;
                        if (Split[1].Length == 64)
                        {
                            connection = (from con in PhoneConnections where con.DeviceId == Split[1] select con).FirstOrDefault();
                        }
                        else
                        {
                            connection = (from con in PhoneConnections where con.Name == Split[1] select con).FirstOrDefault();
                        }
                        if (connection == null)
                            return;
                        string Command = string.Format("+UPDATE {0} {1} {2} {3} {4} {5} {6} {7}\n", Split[2], Split[3], Split[4], Split[5], Split[6], Split[7], Split[8], Split[9]);
                        connection.Write(Command);
                    }
                    break;
                case "delete":
                    {
                        PhoneConnection connection;
                        if (Split[1].Length == 64)
                        {
                            connection = (from con in PhoneConnections where con.DeviceId == Split[1] select con).FirstOrDefault();
                        }
                        else
                        {
                            connection = (from con in PhoneConnections where con.Name == Split[1] select con).FirstOrDefault();
                        }
                        if (connection == null)
                            return;
                        string Command = string.Format("+DELETE {0}\n", Split[2]);
                        connection.Write(Command);
                    }
                    break;
                case "clear":
                    {
                        PhoneConnection connection;
                        if (Split[1].Length == 64)
                        {
                            connection = (from con in PhoneConnections where con.DeviceId == Split[1] select con).FirstOrDefault();
                        }
                        else
                        {
                            connection = (from con in PhoneConnections where con.Name == Split[1] select con).FirstOrDefault();
                        }
                        if (connection == null)
                            return;
                        string Command = string.Format("+CLEAR\n");
                        connection.Write(Command);
                    }
                    break;
            }
        }

        void GetClient(IAsyncResult result)
        {
            TcpClient client = server.EndAcceptTcpClient(result);
            server.BeginAcceptTcpClient(GetClient, null);


            PhoneConnection connection = new PhoneConnection(client);
            lock (PhoneConnections)
            {
                
                PhoneConnections.Add(connection);
            }

        }

        public void Run()
        {
            server.Start();
            server.BeginAcceptTcpClient(GetClient, null);
            Console.WriteLine("Listening for connection...");
            while (true)
            {
                try
                {
                    ParseConsole();
                }
                catch
                {
                }
            }
        }
    }
}
