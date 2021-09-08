using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text.Json;
using TransferObjects;

namespace OSRSWalkThroughListener
{
    class Program
    {
        public static void Main(string[] args)
        {
            TcpListener server = null;
            ConnectionHandler handler = new ConnectionHandler();

            try
            {
                int port = 0;
                string ipAdress = "";

                IPAddress localAddress = IPAddress.Parse(ipAdress);

                server = new TcpListener(localAddress, port);

                server.Start();

                byte[] bytes = new byte[256];
                string data = null;

                while (true) //listening loop
                {
                    //Console.WriteLine("Waiting for a new connection...");

                    //This will block until a client connects, then continue
                    TcpClient client = server.AcceptTcpClient();

                    //Console.WriteLine("Connected to client");

                    data = "";

                    NetworkStream stream = client.GetStream();

                    int i = bytes.Length;

                    //Loop through all the bytes we got
                    while ( i ==  bytes.Length)
                    {
                        i = stream.Read(bytes, 0, bytes.Length);

                        data += System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    }

                    object result = handler.HandleRequest(data);
                    string response = JsonSerializer.Serialize(result);

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(response);

                    stream.Write(msg, 0, msg.Length);

                    //Console.WriteLine("Responded to client");

                    Debug.WriteLine(data);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }
    }
}
