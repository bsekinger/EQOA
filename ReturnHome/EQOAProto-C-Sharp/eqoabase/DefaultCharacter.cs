using System;
using System.Collections.Generic;

#nullable disable

namespace ReturnHome.eqoabase
{
    public partial class DefaultCharacter
    {
        public int DefaultCharacterId { get; set; }
        public string Tclass { get; set; }
        public string Race { get; set; }
        public string HumType { get; set; }
        public int Level { get; set; }
        public int Tunar { get; set; }
        public int BankTunar { get; set; }
        public int UnusedTp { get; set; }
        public int TotalTp { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float? Z { get; set; }
        public float Facing { get; set; }
        public int? World { get; set; }
        public int? Strength { get; set; }
        public int? Stamina { get; set; }
        public int? Agility { get; set; }
        public int? Dexterity { get; set; }
        public int? Wisdom { get; set; }
        public int? Intel { get; set; }
        public int? Charisma { get; set; }
    }
}
