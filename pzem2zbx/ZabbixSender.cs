using System;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace Zabbix.Sender
{
    public class ZabbixSender
    {
        private readonly string _server;
        private readonly int _port;
        private const string ZabbixPreamble = "ZBXD\x0001";

        public ZabbixSender(string server, int port)
        {
            _server = server;
            _port = port;
        }

        public SenderResponse Send(SenderData[] data)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(new SenderRequest()
            {
                data = data
            }));
            SenderResponse senderResponse = null;
            using (TcpClient tcpClient = new TcpClient())
            {
                tcpClient.ReceiveTimeout = 10000;
                tcpClient.SendTimeout = 10000;
                tcpClient.Connect(_server, _port);
                using (NetworkStream stream = tcpClient.GetStream())
                {
                    stream.Write(Encoding.ASCII.GetBytes(ZabbixPreamble), 0, ZabbixPreamble.Length);
                    stream.Write(BitConverter.GetBytes((ulong)bytes.Length), 0, 8);
                    //                    stream.Write(BitConverter.GetBytes(0), 0, 4);
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();

                    byte[] buffer = new byte[1024];
                    ReadBytes(stream, buffer, 0, 13);
                    if (!Encoding.ASCII.GetString(buffer, 0, 5).Equals(ZabbixPreamble))
                        throw new Exception("ProtocolException, expected start of 'ZBXD\x0001'");
                    int remainingLength = BitConverter.ToInt32(buffer, 5);
                    if (remainingLength == 0)
                        throw new Exception("ProtocolException, invalid remaining length");
                    ReadBytes(stream, buffer, 13, remainingLength);


                    string jsonResponse = Encoding.ASCII.GetString(buffer, 13, remainingLength);
                    senderResponse = JsonConvert.DeserializeObject<SenderResponse>(jsonResponse);
                    stream.Close();
                }
                tcpClient.Close();
            }
            return senderResponse;
        }

        private void ReadBytes(NetworkStream stream, byte[] buffer, int startIx, int bytesToRead)
        {
            if (bytesToRead == 0)
                return;
            do
            {
                var read = stream.Read(buffer, startIx, bytesToRead);
                bytesToRead -= read;
                if (bytesToRead == 0)
                    return;
                startIx += read;
                if (read <= 0)
                    throw new Exception("Unexpected read => returned 0 bytes");

            } while (bytesToRead > 0);
        }

        public SenderResponse Send(SenderData data)
        {
            return Send(new[] { data });
        }
    }

    public class SenderData
    {
        public SenderData(string host, string key, string value)
        {
            this.host = host;
            this.key = key;
            this.value = value;
        }
        public SenderData(string host, string key, int value) : this(host, key, value.ToString(NumberFormatInfo.InvariantInfo)) { }
        public SenderData(string host, string key, double value) : this(host, key, value.ToString(NumberFormatInfo.InvariantInfo)) { }

        public string host { get; set; }
        public string key { get; set; }
        public string value { get; set; }
    }

    internal class SenderRequest
    {
        public string request => "sender data";
        public SenderData[] data { get; set; }
    }

    public class SenderResponse
    {
        private string _info;

        public int Failed { get; private set; }

        [JsonProperty("info")]
        public string Info
        {
            get
            {
                return this._info;
            }
            set
            {
                _info = value;
                if (Response == "success")
                {
                    string[] array = _info.Split(';').Select(v => v.Trim()).ToArray<string>();
                    Processed = int.Parse(array[0].Split(' ')[1]);
                    Failed = int.Parse(array[1].Split(' ')[1]);
                    Total = int.Parse(array[2].Split(' ')[1]);
                }
            }
        }

        public int Processed { get; private set; }

        [JsonProperty("response")]
        public string Response { get; set; }

        public int Total { get; private set; }
    }
}