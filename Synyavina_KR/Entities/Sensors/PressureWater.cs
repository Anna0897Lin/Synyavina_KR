using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synyavina_KR.Entities.Sensors
{
    public class PressureWater : Sensor
    {
        public PressureWater(string name, string description)
           : base(name, description) { }

        public override void ReadValue() 
        {
            Value = new Random().NextDouble() * 1;
            Console.WriteLine($"{Name} Pressure Water: {Value} MPa");
        }
    }
}
