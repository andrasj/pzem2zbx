using System;
using System.IO.Ports;
using Modbus.Device;

namespace pzem004tToZabbix
{
    public class Pzem004TDevice : IDisposable
    {
        private SerialPort _serialPort;
        private ModbusSerialMaster _modbusMaster;

        public Pzem004TDevice(string serialPort)
        {
            _serialPort = new SerialPort(serialPort, 9600, Parity.None, 8, StopBits.One);
            _modbusMaster = Modbus.Device.ModbusSerialMaster.CreateRtu(_serialPort);
        }

        public void Open()
        {
            _serialPort.Open();
        }

        public Measurements ReadData()
        {
            ushort[] registers = _modbusMaster.ReadInputRegisters(1, 0, 10);
            return new Measurements(registers[0] / 10.0);
        }

        public void Dispose()
        {
            _modbusMaster?.Dispose();
            _serialPort?.Dispose();
        }
    }
}