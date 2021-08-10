using System;
using System.Collections.Generic;

#nullable disable

namespace ReturnHome.eqoabase
{
    public partial class DefaultSpell
    {
        public int DefSpellId { get; set; }
        public string Tclass { get; set; }
        public int Spellid { get; set; }
        public int Addedorder { get; set; }
        public int OnHotBar { get; set; }
        public int WhereonBar { get; set; }
        public int Unk1 { get; set; }
        public int Showhide { get; set; }

        public virtual Spell Spell { get; set; }
    }
}
