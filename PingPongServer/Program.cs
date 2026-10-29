using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PingPongServer
{
    internal class Program
    {
        enum ConversationState
        {
            WaitingForWhosThere,
            WaitingForWho,
            Finished
        }

        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();

            TcpClient client = listener.AcceptTcpClient();
            NetworkStream stream = client.GetStream();
            Console.WriteLine("Client connected.");

            string message = "Knock Knock!";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            stream.Write(messageBytes, 0, messageBytes.Length);

            ConversationState state = ConversationState.WaitingForWhosThere;

            while (state != ConversationState.Finished)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);



                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Console.WriteLine("Received message: " + response);

                if (bytesRead == 0)
                {
                    message = "Please send something.";
                    messageBytes = Encoding.UTF8.GetBytes(message);
                    stream.Write(messageBytes, 0, messageBytes.Length);
                    continue;
                }

                switch (state)
                {
                    case ConversationState.WaitingForWhosThere:
                            if (string.Equals(response, "whos there?", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(response, "whos there", StringComparison.OrdinalIgnoreCase))
                        {
                            state = ConversationState.WaitingForWho;
                            message = "Boo";
                            messageBytes = Encoding.UTF8.GetBytes(message);
                            stream.Write(messageBytes, 0, messageBytes.Length);
                        }
                        else
                        {
                            message = "Please respond with 'whos there?'";
                            messageBytes = Encoding.UTF8.GetBytes(message);
                            stream.Write(messageBytes, 0, messageBytes.Length);
                        }
                        break;

                    case ConversationState.WaitingForWho:
                        if (string.Equals(response, "boo who?", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(response, "boo who", StringComparison.OrdinalIgnoreCase))
                        {
                            state = ConversationState.Finished;
                            message = "Don't cry, it's just a joke!";
                            messageBytes = Encoding.UTF8.GetBytes(message);
                            stream.Write(messageBytes, 0, messageBytes.Length);
                        }
                        else
                        {
                            message = "Please respond with 'boo who?'";
                            messageBytes = Encoding.UTF8.GetBytes(message);
                            stream.Write(messageBytes, 0, messageBytes.Length);
                        }
                        break;
                }
            }

            client.Close();
            listener.Stop();
        }
    }
}
