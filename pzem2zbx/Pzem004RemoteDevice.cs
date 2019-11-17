using System.Net.Sockets;

namespace pzem004tToZabbix
{
    public class Pzem004RemoteDevice : Pzem004TDeviceBase
    {
        private readonly string _host;
        private readonly int _port;
        private readonly TcpClient _tcpClient;

        public Pzem004RemoteDevice(string host, int port)
        {
            _host = host;
            _port = port;
            _tcpClient = new TcpClient();
        }

        public override void Open()
        {
            _tcpClient.Connect(_host, _port);
            InitModbusMaster(Modbus.Device.ModbusSerialMaster.CreateRtu(_tcpClient));
        }

        public override void Dispose()
        {
            base.Dispose();
            _tcpClient?.Dispose();
        }
    }
}