using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;

namespace TCPRelay
{
    public class TCPRelay
    {
        public static int EntryPoint(string pwzArgument)
        {
            string[] args = pwzArgument.Split(',');

            // Retrieve the listening port and the destination port from the arguments
            int listPort, destPort;

            try {
                listPort = int.Parse(args[0]);
                destPort = int.Parse(args[1]);
            }
            catch {
                MessageBox.Show("Error parsing arguments: " + pwzArgument, "ERROR");
                return 0;
            }

            // If provided, retrieve the destination IP from arguments, 
            string destIP;
            if (args.Length == 3) {
                destIP = args[2];
            }
            else
            {
                destIP = "127.0.0.1";
            }

            // Start the TCP Forwarder
            new TcpForwarderSlim().Start(
                new IPEndPoint(IPAddress.Parse("0.0.0.0"), listPort),
                new IPEndPoint(IPAddress.Parse(destIP), destPort));

            //string processName = Process.GetCurrentProcess().ProcessName;
            //MessageBox.Show("The current process is " + processName);
            //MessageBox.Show("Arguments: " + pwzArgument);

            return 0;
        }
    }

    public class TcpForwarderSlim
    {
        private readonly Socket _mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public void Start(IPEndPoint local, IPEndPoint remote)
        {
            _mainSocket.Bind(local);
            _mainSocket.Listen(10);

            while (true)
            {
                var source = _mainSocket.Accept();
                var destination = new TcpForwarderSlim();
                var state = new State(source, destination._mainSocket);
                destination.Connect(remote, source);
                source.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
            }
        }

        private void Connect(EndPoint remoteEndpoint, Socket destination)
        {
            var state = new State(_mainSocket, destination);
            _mainSocket.Connect(remoteEndpoint);
            _mainSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, OnDataReceive, state);
        }

        private static void OnDataReceive(IAsyncResult result)
        {
            var state = (State)result.AsyncState;
            try
            {
                var bytesRead = state.SourceSocket.EndReceive(result);
                if (bytesRead > 0)
                {
                    state.DestinationSocket.Send(state.Buffer, bytesRead, SocketFlags.None);
                    state.SourceSocket.BeginReceive(state.Buffer, 0, state.Buffer.Length, 0, OnDataReceive, state);
                }
            }
            catch
            {
                state.DestinationSocket.Close();
                state.SourceSocket.Close();
            }
        }

        private class State
        {
            public Socket SourceSocket { get; private set; }
            public Socket DestinationSocket { get; private set; }
            public byte[] Buffer { get; private set; }

            public State(Socket source, Socket destination)
            {
                SourceSocket = source;
                DestinationSocket = destination;
                Buffer = new byte[8192];
            }
        }
    }
}
