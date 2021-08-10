using System;
using System.Collections.Generic;

#nullable disable

namespace ReturnHome.eqoabase
{
    public partial class AccountInfo
    {
        public AccountInfo()
        {
            Characters = new HashSet<Character>();
        }

        public int Accountid { get; set; }
        public string Username { get; set; }
        public byte[] Pwhash { get; set; }
        public int? Acctstatus { get; set; }
        public int? Acctlevel { get; set; }
        public DateTime? Acctcreation { get; set; }
        public DateTime? Lastlogin { get; set; }
        public string Ipaddress { get; set; }
        public string Firstname { get; set; }
        public string Unknown1 { get; set; }
        public string Midinitial { get; set; }
        public string Lastname { get; set; }
        public string Unknown2 { get; set; }
        public string CountryAb { get; set; }
        public string Zip { get; set; }
        public string Birthday { get; set; }
        public string Birthyear { get; set; }
        public string Birthmon { get; set; }
        public string Sex { get; set; }
        public string Email { get; set; }
        public int? Result { get; set; }
        public int? Subtime { get; set; }
        public int? Partime { get; set; }
        public int? Subfeatures { get; set; }
        public int? Gamefeatures { get; set; }

        public virtual ICollection<Character> Characters { get; set; }
    }
}
