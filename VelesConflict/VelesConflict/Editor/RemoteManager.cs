using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net;
using VelesConflict.Shared;
using System.IO;
using VelesConflict.Gameplay;
using Atom.Shared;

namespace VelesConflict.Editor
{
    class RemoteManager
    {
        Socket socket;
        List<byte> buffer;
        StreamReader reader;
        public bool Reconnect { get; set; }
        public bool Connected { get; private set; }

        public RemoteManager()
        {
            Reconnect = true;
            buffer = new List<byte>();
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Connect(IPEndPoint address)
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = address;
            args.Completed += new EventHandler<SocketAsyncEventArgs>(socket_Connected);
            socket.ConnectAsync(args);
        }

        void SendHello()
        {
            string HelloMessage = "+HELLO " + GameBase.DeviceID+"\n";
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(UTF8Encoding.UTF8.GetBytes(HelloMessage),0,HelloMessage.Length);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(socket_Sent);
            socket.SendAsync(args);
        }

        void Recieve()
        {
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            byte[] Buffer = new byte[128];
            args.SetBuffer(Buffer, 0, 128);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(socket_Recieved);
            socket.ReceiveAsync(args);
        }

        void socket_Recieved(object sender, SocketAsyncEventArgs e)
        {
            
            
            int Lines = 0;
            foreach (byte b in e.Buffer)
            {
                if (b == 0)
                    break;
                buffer.Add(b);
                if (b == (char)'\n')
                {
                    Lines++;
                }

            }
            string BufferString = UTF8Encoding.UTF8.GetString(buffer.ToArray(), 0, buffer.Count);
            int Count=0;
            StringReader reader = new StringReader(BufferString);
            for (int i = 0; i < Lines; i++)
            {
                string s=reader.ReadLine();
                Count+=s.Length+1;
                ParseCommand(s);
            }
            buffer.RemoveRange(0, Count);
            Recieve();
        }
        void ParseCommand(string command)
        {
            if (command == null)
                return;
            try
            {
                string[] Split = command.Split(' ');
                switch (Split[0])
                {
                    case "+ADD":
                        {
                            //Id positionX positionY size count owner growthReset growthRate
                            Planet planet = new Planet();
                            planet.Id = int.Parse(Split[1]);
                            planet.Position = new Microsoft.Xna.Framework.Vector2(float.Parse(Split[2]), float.Parse(Split[3]));
                            planet.PlanetSize = float.Parse(Split[4]);
                            planet.Forces = int.Parse(Split[5]);
                            planet.Owner = (PlayerType)int.Parse(Split[6]);
                            planet.GrowthCounter = planet.GrowthReset = int.Parse(Split[7]);
                            planet.Growth = int.Parse(Split[8]);
                            lock (GameBase.GameManager)
                                GameBase.GameManager.State.Planets.Add(planet);
                        }
                        break;
                    case "+UPDATE":
                        {
                            int Id = int.Parse(Split[1]);
                            lock (GameBase.GameManager)
                            {
                                Planet planet = GameBase.GameManager.State.Planets.First(p => p.Id == Id);
                                planet.Position = new Microsoft.Xna.Framework.Vector2(float.Parse(Split[2]), float.Parse(Split[3]));
                                planet.PlanetSize = float.Parse(Split[4]);
                                planet.Forces = int.Parse(Split[5]);
                                planet.Owner = (PlayerType)int.Parse(Split[6]);
                                planet.GrowthCounter = planet.GrowthReset = int.Parse(Split[7]);
                                planet.Growth = int.Parse(Split[8]);
                            }
                        }
                        break;
                    case "+DELETE":
                        //Id
                        {
                            int Id = int.Parse(Split[1]);
                            Planet toRemove = GameBase.GameManager.State.Planets.First(p => p.Id == Id);
                            lock(GameBase.GameManager)
                                GameBase.GameManager.State.Planets.Remove(toRemove);
                        }
                        break;
                    case "+CLEAR":
                        lock (GameBase.GameManager)
                        {
                            GameBase.GameManager.State.Planets.Clear();
                        }
                        break;
                    case "+PLAY":
                        GameBase Game=Globals.Engine as GameBase;
                        Game.EditorStartGame();
                        break;
                }
            }
            catch(Exception e)
            {

            }
        }
        void socket_Sent(object sender, SocketAsyncEventArgs e)
        {
            //throw new NotImplementedException();
        }

        void socket_Connected(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError==SocketError.Success)
            {
                Connected = true;
                SendHello();
                Recieve();
            }
            else
            {
                Connected = false;
            }
        }

    }
}
