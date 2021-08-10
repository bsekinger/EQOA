using System;
using System.Collections.Generic;

#nullable disable

namespace ReturnHome.eqoabase
{
    public partial class WeaponHotBar
    {
        public int WeaponsetId { get; set; }
        public int Serverid { get; set; }
        public string Hotbarname { get; set; }
        public int WeaponId { get; set; }
        public int SecondaryId { get; set; }

        public virtual Character Server { get; set; }
    }
}
