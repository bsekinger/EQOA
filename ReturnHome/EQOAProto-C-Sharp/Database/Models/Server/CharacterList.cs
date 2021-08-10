using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ReturnHome.Database.Models.Server
{
    public class CharacterListContext : DbContext
    {
        public DbSet<CharacterList> _CharacterList { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(
                //@"Server=(localdb)\mssqllocaldb;Database=Blogging;Integrated Security=True");
        }
    }

    public record CharacterList
    {
        public string charName { get; private set; }
        public int serverID { get; private set; }
        public int modelID { get; private set; }
        public int Class { get; private set; }
        public int race { get; private set; }
        public int level { get; private set; }
        public int hairColor { get; private set; }
        public int hairLength { get; private set; }
        public int hairStyle { get; private set; }
        public int faceOption { get; private set; }
        public int robeType { get; private set; }
        public int primaryWeapon { get; private set; }
        public int secondaryWeapon { get; private set; }
        public int shield { get; private set; }
        public byte chest { get; private set; }
        public byte bracer { get; private set; }
        public byte gloves { get; private set; }
        public byte legs { get; private set; }
        public byte boots { get; private set; }
        public byte helm { get; private set; }
        public uint chestColor { get; private set; }
        public uint bracerColor { get; private set; }
        public uint gloveColor { get; private set; }
        public uint legColor { get; private set; }
        public uint bootColor { get; private set; }
        public uint helmColor { get; private set; }
        public uint robeColor { get; private set; }
    }
}
