using System;
using System.Collections.Generic;

#nullable disable

namespace ReturnHome.eqoabase
{
    public partial class SpellPattern
    {
        public int Spellid { get; set; }
        public int Abilitylvl { get; set; }
        public int Unk2 { get; set; }
        public int Unk3 { get; set; }
        public int Range { get; set; }
        public int Casttime { get; set; }
        public int Power { get; set; }
        public long IconColor { get; set; }
        public long Icon { get; set; }
        public int Scope { get; set; }
        public int Recast { get; set; }
        public int Eqprequire { get; set; }
        public string Spellname { get; set; }
        public string Spelldesc { get; set; }

        public virtual Spell Spell { get; set; }
    }
}
