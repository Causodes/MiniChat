using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace Mini_Chat_Server
{
    class Program
    {
        public static Hashtable clientList = new Hashtable();

        static void Main(string[] args)
        {
            IPAddress localIP = IPAddress.Parse(GetLocalIPAddress()); //local IP address
            TcpListener server = new TcpListener(localIP, 8080);//open the port 8080 to listen for the connection
            TcpClient clientRequest = default(TcpClient);     

            //Start listening for client connection request.
            server.Start();
            Console.WriteLine("Waiting for connections....");

            while ((true))
            {
                clientRequest = server.AcceptTcpClient();//Connect to a request

                // Buffer for reading data
                byte[] bufferBytes = new byte[10025];
                string msgStr = null;
                string userName = null;

                NetworkStream networkStream = clientRequest.GetStream();
                int i = networkStream.Read(bufferBytes, 0, bufferBytes.Length); //i is the total number of bytes read into buffer
                msgStr = System.Text.Encoding.ASCII.GetString(bufferBytes, 0, i);//The 1st message after the connection is the user name.
                userName = msgStr; //The 1st message received is always the user name
                try
                {
                    clientList.Add(msgStr, clientRequest);//Add to the hash table
                    sendToAll(msgStr, userName, false);//See the method created below
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                }
                Console.WriteLine(userName + " joined chat room.");

                //Setup thread and handle the message from each client
                handleClinet client = new handleClinet();
                client.startClient(clientRequest, userName, clientList);
            }

            //clientRequest.Close();
            //server.Stop();
            //Console.WriteLine("exit");
            //Console.ReadLine();
        }

        public static void sendToAll(string msg, string uName, bool flag)
        {
            foreach (DictionaryEntry pair in clientList)
            {
                TcpClient sourceClient;
                sourceClient = (TcpClient)pair.Value;
                NetworkStream streamSentToAll = sourceClient.GetStream();
                byte[] bytesSentToAll = null;

                try
                {
                    if (flag == true)//true when it is the real chat message
                    {
                        if (msg == "$$$left$$$")//client will send this message if exists the chat 
                        {
                            bytesSentToAll = Encoding.ASCII.GetBytes(uName + " left chat room.");
                        }
                        else
                        {
                            bytesSentToAll = Encoding.ASCII.GetBytes(uName + " says: " + msg);//True chat message
                        }
                    }
                    else
                    {
                        bytesSentToAll = Encoding.ASCII.GetBytes(uName + " joined chat room.");//1st message after the connection is established
                    }

                    streamSentToAll.Write(bytesSentToAll, 0, bytesSentToAll.Length);
                    streamSentToAll.Flush();
                } catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                }
            }
        } //end sendToAll method


        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

    }//end Program class

    public class handleClinet
    {
        TcpClient clientSocket;
        string userName;
        Hashtable clientsList;

        public void startClient(TcpClient inClientSocket, string uName, Hashtable cList)
        {
            this.clientSocket = inClientSocket;
            this.userName = uName;
            this.clientsList = cList;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }

        private void doChat()
        {
            int i;
            byte[] bufferBytes = new byte[10025];
            string msgFromClient = null;

            while ((true))
            {
                try
                {
                    NetworkStream networkStream = clientSocket.GetStream();
                    i = networkStream.Read(bufferBytes, 0, (int)clientSocket.ReceiveBufferSize);
                    msgFromClient = System.Text.Encoding.ASCII.GetString(bufferBytes, 0, i);

                    if (msgFromClient == "$$$left$$$")
                    {
                        Console.WriteLine(userName + " left chat room");
                    }
                    else
                    {
                        Console.WriteLine(userName + " : " + msgFromClient);
                    }

                    Program.sendToAll(msgFromClient, userName, true);
                }
                catch (Exception ex)
                {
                    //Console.WriteLine(ex.ToString());
                }
            }//end while
        }//end doChat

    } //end class handleClinet
}
