using Synyavina_KR.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synyavina_KR.Entities.Systems
{
    public class ValveShutterSystem : IControllable
    {
        public bool IsOn { get; private set; }

        public void TurnOn()
        {
            IsOn = true;
            Console.WriteLine("Valve-Shutter System turned on.");
        }

        public void TurnOff()
        {
            IsOn = false;
            Console.WriteLine("Valve-Shutter System turned off.");
        }
    }
    
}
