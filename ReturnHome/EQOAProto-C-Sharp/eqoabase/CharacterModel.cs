using System;
using System.Collections.Generic;

#nullable disable

namespace ReturnHome.eqoabase
{
    public partial class CharacterModel
    {
        public CharacterModel()
        {
            Characters = new HashSet<Character>();
        }

        public string Sex { get; set; }
        public long Modelid { get; set; }
        public string Race { get; set; }

        public virtual ICollection<Character> Characters { get; set; }
    }
}
