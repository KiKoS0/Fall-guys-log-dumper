using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FallGuys
{
    public class Config
    {
        public string LogDirectory { get; set; }
        public string LogFileName { get; set; }
        public string WindowName { get; set; }
        public string DetectCountdownPattern { get; set; }
    }
}
