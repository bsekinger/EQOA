using System;
using System.Collections.Generic;

#nullable disable

namespace ReturnHome.eqoabase
{
    public partial class CharInventory
    {
        public int Itemid { get; set; }
        public int Serverid { get; set; }
        public int Stackleft { get; set; }
        public int RemainHp { get; set; }
        public int Remaincharge { get; set; }
        public int Patternid { get; set; }
        public int Equiploc { get; set; }
        public int Location { get; set; }
        public int Listnumber { get; set; }

        public virtual ItemPattern Pattern { get; set; }
        public virtual Character Server { get; set; }
    }
}
