using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synyavina_KR.Entities.Sensors
{
    public class VoltageOut : Sensor
    {
        public VoltageOut(string name, string description)
           : base(name, description) { }

        public override void ReadValue()
        {
            Value = new Random().NextDouble() * 50;
            Console.WriteLine($"{Name} Voltage Out Generator: {Value} kV");
        }
    }
}
