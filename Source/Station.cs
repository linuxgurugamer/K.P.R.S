using SpaceTuxUtility;

using static KPRS.RegisterToolbar;


namespace KPRS
{
    internal class Station
    {
        internal string name;
        internal int id;         // Used by mod to associate a station with a transmitter
        internal string abbr;
        internal bool repeat;
        internal float repeatDelay;
        internal int channelNumber;
        internal string channelCallSign;
        internal string playlist;      
        internal string location;
        internal bool interplanetary;
        // range = 
        internal double power; 		// Transmission power

        internal bool selected = false;

        internal Station(ConfigNode node)
        {
            this.name = node.SafeLoad("name", "");
            this.id = node.SafeLoad("id", -1);
            this.abbr = node.SafeLoad("abbr", "");
            if (abbr.Length == 0)
                abbr = id.ToString();
            this.repeatDelay = node.SafeLoad("repeatDelay", 0f);
            this.channelNumber = node.SafeLoad("channelNumber", 0);
            this.channelCallSign = node.SafeLoad("channelCallSign", "");
            this.playlist = node.SafeLoad("playlist", "");
            this.location = node.SafeLoad("location", "");
            this.interplanetary = node.SafeLoad("interplanetary", true);
            this.power = node.SafeLoad("power", 1f);

            Log.Info(this.ToString());
        }

        public override string ToString()
        {
            return name + " " + id + " " + repeat +" "+ repeatDelay + " " + playlist +
                " " + location + " " + interplanetary + " " + power;
        }

    }

}
