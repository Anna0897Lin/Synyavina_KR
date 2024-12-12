using Synyavina_KR.Entities.Sensors;
using Synyavina_KR.Entities.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synyavina_KR.Entities
{
    public class AutoGES
    {
        public List<Sensor> Sensors { get; set; }

        public AllarmNotificationSystem AllarmNotification {  get; set; }
        public GeneratorSystem Generator { get; set; }
        public TransformersSystem Transformers { get; set; }
        public TurbineSystem Turbine { get; set; }
        public ValveShutterSystem ValveShutter { get; set; }

        public AutoGES() 
        {
            Sensors = new List<Sensor>();
            AllarmNotification = new AllarmNotificationSystem();
            Generator = new GeneratorSystem();
            Transformers = new TransformersSystem();
            Turbine = new TurbineSystem();
            ValveShutter = new ValveShutterSystem();
        }

        public void Monitor()
        {
            foreach(var sensor in Sensors) 
            { 
                sensor.ReadValue(); 
            }
        }
    }
}
