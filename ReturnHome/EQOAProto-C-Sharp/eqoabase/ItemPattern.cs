using System;
using System.Collections.Generic;

#nullable disable

namespace ReturnHome.eqoabase
{
    public partial class ItemPattern
    {
        public ItemPattern()
        {
            CharInventories = new HashSet<CharInventory>();
            DefaultcharInventories = new HashSet<DefaultcharInventory>();
        }

        public int Patternid { get; set; }
        public int Patternfam { get; set; }
        public int Unk1 { get; set; }
        public long Itemicon { get; set; }
        public int Unk2 { get; set; }
        public int Equipslot { get; set; }
        public int Unk3 { get; set; }
        public int Trade { get; set; }
        public int Rent { get; set; }
        public int Unk4 { get; set; }
        public int Attacktype { get; set; }
        public int Weapondamage { get; set; }
        public int Unk5 { get; set; }
        public int Levelreq { get; set; }
        public int Maxstack { get; set; }
        public int Maxhp { get; set; }
        public int Duration { get; set; }
        public int Classuse { get; set; }
        public int Raceuse { get; set; }
        public int Procanim { get; set; }
        public int Lore { get; set; }
        public int Unk6 { get; set; }
        public int Craft { get; set; }
        public string Itemname { get; set; }
        public string Itemdesc { get; set; }
        public long Model { get; set; }
        public long Color { get; set; }
        public int? Str { get; set; }
        public int? Sta { get; set; }
        public int? Agi { get; set; }
        public int? Wis { get; set; }
        public int? Dex { get; set; }
        public int? Cha { get; set; }
        public int? Int { get; set; }
        public int? Hpmax { get; set; }
        public int? Powmax { get; set; }
        public int? PoT { get; set; }
        public int? HoT { get; set; }
        public int? Ac { get; set; }
        public int? Pr { get; set; }
        public int? Dr { get; set; }
        public int? Fr { get; set; }
        public int? Cr { get; set; }
        public int? Lr { get; set; }
        public int? Ar { get; set; }

        public virtual ICollection<CharInventory> CharInventories { get; set; }
        public virtual ICollection<DefaultcharInventory> DefaultcharInventories { get; set; }
    }
}
