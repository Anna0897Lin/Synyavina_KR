using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synyavina_KR.Entities.Sensors
{
    public class SpeedTurbine : Sensor
    {
        public SpeedTurbine(string name, string description)
           : base(name, description) { }

        public override void ReadValue()
        {
            Value = new Random().NextDouble() * 1000;
            Console.WriteLine($"{Name} Speed Turbine: {Value} RPM");
        }
    }
}
