using System;
using Modbus.Device;

namespace pzem004tToZabbix
{
    public abstract class Pzem004TDeviceBase : IDisposable
    {
        private ModbusMaster _modbusMaster;
        public abstract void Open();

        protected void InitModbusMaster(ModbusMaster modbusMaster)
        {
            _modbusMaster = modbusMaster;
        }

        public Measurements ReadData()
        {
            ushort[] registers = _modbusMaster.ReadInputRegisters(1, 0, 10);
            double voltage = registers[0] / 10.0;
            double current = (registers[1] + (registers[2] << 16)) / 1000.0;
            double power = (registers[3] + (registers[4] << 16)) / 10.0;
            double energy = registers[5] + (registers[6] << 16);
            double frequency = registers[7] / 10.0;
            double powerFactor = registers[8] / 100.0;
            return new Measurements(voltage, current, power, energy, frequency, powerFactor);
        }

        public virtual void Dispose()
        {
            _modbusMaster?.Dispose();
        }
    }
}