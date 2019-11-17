using System.IO.Ports;

namespace pzem004tToZabbix
{
    public class Pzem004TLocalDevice : Pzem004TDeviceBase
    {
        private readonly SerialPort _serialPort;

        public Pzem004TLocalDevice(string serialPort)
        {
            _serialPort = new SerialPort(serialPort, 9600, Parity.None, 8, StopBits.One);
        }

        public override void Open()
        {
            _serialPort.Open();
            InitModbusMaster(Modbus.Device.ModbusSerialMaster.CreateRtu(_serialPort));
        }

        public override void Dispose()
        {
            base.Dispose();
            _serialPort?.Dispose();
        }
    }
}