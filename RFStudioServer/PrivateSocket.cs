using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RFStudioServer
{
    class PrivateSocket
    {
        private mainUI mainForm;
        private int port;
        private Socket listener;
        private Socket handler;

        private string data = null;
        private IPAddress ipAddress;

        public PrivateSocket(mainUI form, int port)
        {
            mainForm = form;
            this.port = port;
        }

        public void StartListening()
        {
            byte[] bytes = new Byte[10240];
            ipAddress = new IPAddress(new byte[] { 0, 0, 0, 0 });
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            LingerOption lingerOption = new LingerOption(false, 0);

            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                while (true)
                {
                    handler = listener.Accept();
                    data = null;

                    while (true)
                    {
                        String message="";
                        byte[] msg;
                        bytes = new byte[65536];
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        mainForm.logData(data);
                        try {
                            if (mainForm.accountVerify(data.Substring(0, data.IndexOf('@'))))
                            {
                                string incoming = data.Substring(data.IndexOf('@') + 1);
                                mainForm.logData("10000:VERIFIED : " + incoming);
                                Console.WriteLine("10000:VERIFIED : " + incoming);
                                mainForm.updateStatus(incoming);
                                msg = Encoding.ASCII.GetBytes(mainForm.processData(incoming));                             
                            }
                            else
                            {
                                message = "10001:INVALID_ACCOUNT";
                                msg = Encoding.ASCII.GetBytes(message);
                                Console.WriteLine(message);
                                mainForm.logData(message);
                                mainForm.updateStatus(message);

                            }
                        } catch(Exception ex)
                        {
                            mainForm.logError(ex);
                            message = "10002:DATA_ERR";
                            msg = Encoding.ASCII.GetBytes(message);
                            Console.WriteLine(message);
                            mainForm.logData(message);
                            mainForm.updateStatus(message);
                        }

                        mainForm.logError(msg.ToString());
                        
                        handler.Send(msg);
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Data);
                mainForm.logError(e);
            }
            Console.WriteLine("SOCKET_THREAD_CLOSED");
            mainForm.logError("SOCKET_THREAD_CLOSED");
        }

        public void stopConnection()
        {
            try
            {
                handler.Dispose();
                listener.Dispose();
            }
            catch (Exception e) { mainForm.logError(e); }
        }
    }
}
