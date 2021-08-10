using System;
using System.Collections.Generic;

#nullable disable

namespace ReturnHome.eqoabase
{
    public partial class Spell
    {
        public Spell()
        {
            DefaultSpells = new HashSet<DefaultSpell>();
        }

        public int Serverid { get; set; }
        public int Spellid { get; set; }
        public int Addedorder { get; set; }
        public int OnHotBar { get; set; }
        public int WhereonBar { get; set; }
        public int Unk1 { get; set; }
        public int Showhide { get; set; }

        public virtual Character Server { get; set; }
        public virtual SpellPattern SpellNavigation { get; set; }
        public virtual ICollection<DefaultSpell> DefaultSpells { get; set; }
    }
}
