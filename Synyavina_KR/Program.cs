using System;
using System.Text;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Synyavina_KR.Constans;
using Synyavina_KR.Entities;
using Synyavina_KR.Entities.Sensors;
using System.Xml.Linq;
using Microsoft.AspNetCore.SignalR.Client;

namespace AutomatedGES     
{
    class Program
    {
        private static readonly ConcurrentDictionary<string, Task> tasks =
           new ConcurrentDictionary<string, Task>();
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> tokens =
            new ConcurrentDictionary<string, CancellationTokenSource>();
        private static readonly ConcurrentDictionary<string, double> values =
            new ConcurrentDictionary<string, double>();

        private static AutoGES autoGES = new AutoGES();
        private static HubConnection connection;

        static async Task Main(string[] args)
        {
            connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7045/indicator")
                .Build();

            await connection.StartAsync();

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:7045/api/");
            var result = await client.GetAsync("indicator");
            var content = await result.Content.ReadAsStringAsync();

            var deserializedResult = JsonSerializer.Deserialize<List<IndicatorModel>>(content, new
                JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var sensorFactories = new Dictionary<string, Func<Sensor>>
            {
                 {"Temperature",() => new TemperatureSensor("Temperature", "No description") },
                 {"Level",() => new LevelWater("Level", "No description") },
                 {"Pressure",() => new PressureWater("Pressure", "No description") },
                 {"Speed",() => new SpeedTurbine("Speed", "No description") },
                 {"Voltage",() => new VoltageOut("Voltage", "No description") },
            };


            foreach (var indicator in deserializedResult)
            {
                if (sensorFactories.TryGetValue(indicator.Name, out var createSensor))
                {
                    var sensor = createSensor();
                    sensor.Name = indicator.Name;
                    sensor.Description = indicator.Description;
                    autoGES.Sensors.Add(sensor);
                }
                else
                {
                    Console.WriteLine($"Unknown indicator: {indicator.Name}");
                }
            }

            foreach (var model in deserializedResult)
            {
                AddDataProcessTask(model.Id,
                    model.Value,
                    model.IndicatorValues.LastOrDefault() ?? "0",
                    model);
            }

            connection.On("UpdateTargetValue", (string id, string value) =>
            {
                tokens.TryGetValue(id, out CancellationTokenSource? token);
                if (tokens == null)
                {
                    Console.WriteLine($"No token found for ID: {id}");
                    return;
                }

                token.Cancel();
                Console.WriteLine($"CancellingTask with ID: {id} and adding new task");
                AddDataProcessTask(Guid.Parse(id), value, "0", new IndicatorModel());
            });

            connection.Closed += async (error) =>
            {
                Console.WriteLine("Connection closed. Trying to reconnect ...");
                await Task.Delay(new Random().Next(10, 11) * 1000);
                await connection.StartAsync();
            };

            for (int i = 1; i <= SystemConstants.numOfCycle; i++)
            {
                Console.WriteLine($"\n --- Cycle number {i} ---");

                int size = autoGES.Sensors.Count();

                foreach (var sensor in autoGES.Sensors)
                {
                    if ((sensor is SpeedTurbine && sensor.Value > SystemConstants.SpeedUst + SystemConstants.gist)
                        || (sensor is TemperatureSensor && sensor.Value > SystemConstants.TemperUst + SystemConstants.gist))
                    { autoGES.Turbine.TurnOff(); }
                    else if ((sensor is PressureWater && sensor.Value >= SystemConstants.PressUst + SystemConstants.PressGist)
                        || (sensor is LevelWater && sensor.Value >= SystemConstants.LevUst + SystemConstants.gist))
                    { autoGES.Turbine.TurnOn(); }

                    if (sensor is LevelWater && sensor.Value > SystemConstants.LevUst + SystemConstants.gist)
                    { autoGES.ValveShutter.TurnOn(); }
                    else if ((sensor is LevelWater && sensor.Value < SystemConstants.LevUst - SystemConstants.gist)
                            || (sensor is VoltageOut && sensor.Value > SystemConstants.Uust + SystemConstants.gist))
                    { autoGES.ValveShutter.TurnOff(); }

                    if (
                        (sensor is LevelWater && ((sensor.Value > SystemConstants.LevUst + SystemConstants.AllarmGist) ||
                        (sensor.Value < SystemConstants.LevUst - SystemConstants.AllarmGist)))
                       || (sensor is PressureWater && ((sensor.Value > SystemConstants.PressUst + SystemConstants.PressGist) ||
                        (sensor.Value < SystemConstants.PressUst - SystemConstants.PressGist)))
                       || (sensor is SpeedTurbine && ((sensor.Value > SystemConstants.SpeedUst + SystemConstants.AllarmGist) ||
                        (sensor.Value < SystemConstants.SpeedUst - SystemConstants.AllarmGist)))
                       || (sensor is TemperatureSensor && ((sensor.Value > SystemConstants.TemperUst + SystemConstants.AllarmGist) ||
                        (sensor.Value < SystemConstants.TemperUst - SystemConstants.AllarmGist)))
                       || (sensor is VoltageOut && ((sensor.Value > SystemConstants.Uust + SystemConstants.AllarmGist) ||
                        (sensor.Value < SystemConstants.Uust - SystemConstants.AllarmGist)))
                        )
                    {
                        size++;
                    }

                }

                if (size == autoGES.Sensors.Count())
                { autoGES.AllarmNotification.TurnOff(); }
                else
                {
                    int quantityAllarm = size - autoGES.Sensors.Count();
                    autoGES.AllarmNotification.TurnOn();
                    Console.WriteLine($"Quantity Allarm: {quantityAllarm}");
                }

                if (i % 3 == 0)
                {
                    autoGES.Generator.TurnOn();
                    autoGES.Transformers.TurnOn();
                }
                else
                {
                    autoGES.Generator.TurnOff();
                    autoGES.Transformers.TurnOff();
                }

            }
            Thread.Sleep(3000);

            Console.ReadLine();
        }

        private static void AddDataProcessTask(Guid id, string value, string lastValue, IndicatorModel indicatorModel)
        {
            var source = new CancellationTokenSource();

            var task = CreateDataProcessingTask(
                id,
                double.Parse(value),
                 double.Parse(lastValue),
                 source.Token,
                 indicatorModel);

            tasks.TryAdd(id.ToString(), task);
            tokens.TryAdd(id.ToString(), source);
        }

        private static async Task CreateDataProcessingTask(Guid id, double baseValue, double lastValue,
            CancellationToken token, IndicatorModel indicatorModel)
        {
            while (!token.IsCancellationRequested)
            {
                double value = lastValue;
                values.AddOrUpdate(id.ToString(), lastValue, (name, currentValue) =>
                {
                    value = GenerateValue(baseValue, currentValue, indicatorModel);
                    return value;
                });

                await connection.InvokeAsync("SendValue", id.ToString(), value.ToString(), token);
                await Task.Delay(3000, token);
            }

            tasks.Remove(id.ToString(), out _);
        }

        private static double GenerateValue(double targetValue, double currentValue, IndicatorModel indicatorModel)
        {
            autoGES.Monitor();

            var value = autoGES.Sensors.FirstOrDefault(x => x.Name == indicatorModel.Name);

            return Math.Round(value.Value, 2);
        }

    }
}
