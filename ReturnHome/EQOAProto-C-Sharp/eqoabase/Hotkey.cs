using System;
using System.Collections.Generic;

#nullable disable

namespace ReturnHome.eqoabase
{
    public partial class Hotkey
    {
        public int Hotkeyid { get; set; }
        public int Serverid { get; set; }
        public string Direction { get; set; }
        public string Nlabel { get; set; }
        public string Nmessage { get; set; }
        public string Wlabel { get; set; }
        public string Wmessage { get; set; }
        public string Elabel { get; set; }
        public string Emessage { get; set; }
        public string Slabel { get; set; }
        public string Smessage { get; set; }

        public virtual Character Server { get; set; }
    }
}
