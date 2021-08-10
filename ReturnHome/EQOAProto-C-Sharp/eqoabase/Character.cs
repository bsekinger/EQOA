using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace ReturnHome.eqoabase
{

    public partial class Character
    {
        /*
        public Character()
        {
            CharInventories = new HashSet<CharInventory>();
            Hotkeys = new HashSet<Hotkey>();
            Spells = new HashSet<Spell>();
            WeaponHotBars = new HashSet<WeaponHotBar>();
        }*/

        public string CharName { get; set; }
        public int Accountid { get; set; }
        public int Serverid { get; set; }
        public long Modelid { get; set; }
        public int Tclass { get; set; }
        public int Race { get; set; }
        public string HumType { get; set; }
        public int Level { get; set; }
        public int Haircolor { get; set; }
        public int Hairlength { get; set; }
        public int Hairstyle { get; set; }
        public int Faceoption { get; set; }
        public int ClassIcon { get; set; }
        public int TotalXp { get; set; }
        public int Debt { get; set; }
        public int Breath { get; set; }
        public int Tunar { get; set; }
        public int BankTunar { get; set; }
        public int UnusedTp { get; set; }
        public int TotalTp { get; set; }
        public int World { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Facing { get; set; }
        public float Unknown { get; set; }
        public int Strength { get; set; }
        public int Stamina { get; set; }
        public int Agility { get; set; }
        public int Dexterity { get; set; }
        public int Wisdom { get; set; }
        public int Intel { get; set; }
        public int Charisma { get; set; }
        public int CurrentHp { get; set; }
        public int MaxHp { get; set; }
        public int CurrentPower { get; set; }
        public int MaxPower { get; set; }
        public int Unk421 { get; set; }
        public int Healot { get; set; }
        public int Powerot { get; set; }
        public int Ac { get; set; }
        public int Unk422 { get; set; }
        public int Unk423 { get; set; }
        public int Unk424 { get; set; }
        public int Unk425 { get; set; }
        public int Unk426 { get; set; }
        public int Unk427 { get; set; }
        public int Unk428 { get; set; }
        public int Poisonr { get; set; }
        public int Diseaser { get; set; }
        public int Firer { get; set; }
        public int Coldr { get; set; }
        public int Lightningr { get; set; }
        public int Arcaner { get; set; }
        public int Fishing { get; set; }
        public int BaseStrength { get; set; }
        public int BaseStamina { get; set; }
        public int BaseAgility { get; set; }
        public int BaseDexterity { get; set; }
        public int BaseWisdom { get; set; }
        public int BaseIntel { get; set; }
        public int BaseCharisma { get; set; }
        public int CurrentHp2 { get; set; }
        public int BaseHp { get; set; }
        public int CurrentPower2 { get; set; }
        public int BasePower { get; set; }
        public int Unk429 { get; set; }
        public int Healot2 { get; set; }
        public int Powerot2 { get; set; }
        public int Unk4210 { get; set; }
        public int Unk4211 { get; set; }

        public virtual AccountInfo Account { get; set; }
        public virtual CharacterModel Model { get; set; }
        public virtual ICollection<CharInventory> CharInventories { get; set; }
        public virtual ICollection<Hotkey> Hotkeys { get; set; }
        public virtual ICollection<Spell> Spells { get; set; }
        public virtual ICollection<WeaponHotBar> WeaponHotBars { get; set; }
    }
}
