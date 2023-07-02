using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPRS
{
    internal class Transmitter
    {
        internal string location;
        internal string selectedStation;

        internal Transmitter( string selStat, string loc = null)
        {
            location = loc;
            selectedStation = selStat;
        }
    }
}
