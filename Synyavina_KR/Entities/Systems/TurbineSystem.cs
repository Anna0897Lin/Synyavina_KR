﻿using Synyavina_KR.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synyavina_KR.Entities.Systems
{
    public class TurbineSystem : IControllable
    {
        public bool IsOn { get; private set; }

        public void TurnOn()
        {
            IsOn = true;
            Console.WriteLine("Turbine System turned on.");
        }

        public void TurnOff()
        {
            IsOn = false;
            Console.WriteLine("Turbine System turned off.");
        }
    }
}