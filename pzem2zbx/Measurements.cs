using Newtonsoft.Json;

namespace pzem004tToZabbix
{
    public class Measurements
    {
        public Measurements(double voltageV, double currentA, double powerW, double energyWh, double frequencyHz, double powerFactor)
        {
            VoltageV = voltageV;
            CurrentA = currentA;
            PowerW = powerW;
            EnergyWh = energyWh;
            FrequencyHz = frequencyHz;
            PowerFactor = powerFactor;
        }

        public double VoltageV { get; }
        public double CurrentA { get; }
        public double PowerW { get; }
        public double EnergyWh { get; }
        public double FrequencyHz { get; }
        public double PowerFactor { get; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}