using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POC.Model
{
    public class App
    {
        public static bool HasMoveFinished { get; set; } = true;
        public static int DesiredPos { get; set; }
        public static int ActualPos { get; set; }
    }
}
