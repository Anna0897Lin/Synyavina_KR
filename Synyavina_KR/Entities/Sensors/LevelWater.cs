using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synyavina_KR.Entities.Sensors
{
    public class LevelWater : Sensor
    {
        public LevelWater(string name, string description)
           : base(name, description) { }

        public override void ReadValue()
        {
            Value = new Random().NextDouble() * 100;
            Console.WriteLine($"{Name} Level Water: {Value} m");
        }
    }
}
