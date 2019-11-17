using System;
using System.Globalization;
using System.Threading;
using Microsoft.Extensions.CommandLineUtils;
using Zabbix.Sender;
using ZabbixSender.Async;

namespace pzem004tToZabbix
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new Microsoft.Extensions.CommandLineUtils.CommandLineApplication();
            app.Description =
                "Fetches data from a PZEM-004T device over Serial-ModbusRTU and pushes the data into a zabbix trapper item";
            app.HelpOption("-?|--help|--usage");
            var zbxServerOption = app.Option("-s|--zbxServer", "zabbix server to send to", CommandOptionType.SingleValue);
            var zbxPortOption = app.Option("-p|--zbxPort", "port of zabbix server to send to", CommandOptionType.SingleValue);
            var zbxHostOption = app.Option("-h|--hostname", "hostname configured in zabbix with configured trapper items", CommandOptionType.SingleValue);
            var devPortOption = app.Option("-d|--devicePort", "device where the PZEM-004T is connected to", CommandOptionType.SingleValue);


            app.OnExecute(() => Main(zbxHostOption.Value(), zbxServerOption.Value(), int.Parse(zbxPortOption.Value()),
                devPortOption.Value()));
            app.Execute(args);

        }

        private static int Main(string zbxHost, string zbxServer, int zbxPort, string devPort)
        {
            var senderService = new Zabbix.Sender.ZabbixSender(zbxServer, zbxPort);

            Pzem004TDeviceBase device;

            using (device = CreateDevice(devPort))
            {
                device.Open();
                while (true)
                {
                    var measurements = device.ReadData();
                    Console.WriteLine(measurements);
                    var senderResponse = senderService.Send(new[]
                    {
                        new SenderData(zbxHost, "pzem004t.voltage", measurements.VoltageV.ToString(NumberFormatInfo.InvariantInfo)),
                        new SenderData(zbxHost, "pzem004t.current", measurements.CurrentA.ToString(NumberFormatInfo.InvariantInfo)),
                        new SenderData(zbxHost, "pzem004t.power", measurements.PowerW.ToString(NumberFormatInfo.InvariantInfo)),
                        new SenderData(zbxHost, "pzem004t.energy", measurements.EnergyWh.ToString(NumberFormatInfo.InvariantInfo)),
                        new SenderData(zbxHost, "pzem004t.freq", measurements.FrequencyHz.ToString(NumberFormatInfo.InvariantInfo)),
                        new SenderData(zbxHost, "pzem004t.pFac", measurements.PowerFactor.ToString(NumberFormatInfo.InvariantInfo))
                    });

                    if (senderResponse == null)
                    {
                        Console.WriteLine("No response from zabbix server");
                    }
                    else
                    {
                        Console.WriteLine(senderResponse.Info);
                    }
                    Thread.Sleep(5000);
                }
            }
        }

        private static Pzem004TDeviceBase CreateDevice(string target)
        {
            if (target.Contains(":"))
            {
                var targetParts = target.Split(':');
                return new Pzem004RemoteDevice(targetParts[0], int.Parse(targetParts[1]));
            }
            else
                return new Pzem004TLocalDevice(target);
        }
    }
}
