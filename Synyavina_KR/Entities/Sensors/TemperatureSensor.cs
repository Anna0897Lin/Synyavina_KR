using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synyavina_KR.Entities.Sensors
{
    public class TemperatureSensor : Sensor
    {
       public TemperatureSensor(string name, string description) 
            : base (name, description) {}

        public override void ReadValue()
        {
            Value = new Random().NextDouble() * 100;
            Console.WriteLine($"{Name} Temperature: {Value} °C");
        }
    }
}
