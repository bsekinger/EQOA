using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace ReturnHome.eqoabase
{
    public partial class eqoabaseContext : DbContext
    {
        public eqoabaseContext()
        {
        }

        public eqoabaseContext(DbContextOptions<eqoabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AccountInfo> AccountInfos { get; set; }
        public virtual DbSet<CharInventory> CharInventories { get; set; }
        public virtual DbSet<Character> Characters { get; set; }
        public virtual DbSet<CharacterModel> CharacterModels { get; set; }
        public virtual DbSet<DefaultCharacter> DefaultCharacters { get; set; }
        public virtual DbSet<DefaultSpell> DefaultSpells { get; set; }
        public virtual DbSet<DefaultcharInventory> DefaultcharInventories { get; set; }
        public virtual DbSet<Hotkey> Hotkeys { get; set; }
        public virtual DbSet<ItemPattern> ItemPatterns { get; set; }
        public virtual DbSet<Spell> Spells { get; set; }
        public virtual DbSet<SpellPattern> SpellPatterns { get; set; }
        public virtual DbSet<WeaponHotBar> WeaponHotBars { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseMySQL("Server=192.168.1.253;port=3306;Database=eqoabase;Uid=fooUser;Pwd=fooPass");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountInfo>(entity =>
            {
                entity.HasKey(e => new { e.Accountid, e.Username })
                    .HasName("PRIMARY");

                entity.ToTable("AccountInfo");

                entity.HasIndex(e => e.Accountid, "accountid_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.Username, "username_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.Accountid)
                    .HasColumnType("int(11)")
                    .ValueGeneratedOnAdd()
                    .HasColumnName("accountid");

                entity.Property(e => e.Username)
                    .HasMaxLength(32)
                    .HasColumnName("username");

                entity.Property(e => e.Acctcreation)
                    .HasColumnName("acctcreation")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Acctlevel)
                    .HasColumnType("int(11)")
                    .HasColumnName("acctlevel")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Acctstatus)
                    .HasColumnType("int(11)")
                    .HasColumnName("acctstatus")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Birthday)
                    .HasMaxLength(16)
                    .HasColumnName("birthday")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Birthmon)
                    .HasMaxLength(16)
                    .HasColumnName("birthmon")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Birthyear)
                    .HasMaxLength(16)
                    .HasColumnName("birthyear")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.CountryAb)
                    .HasMaxLength(16)
                    .HasColumnName("countryAB")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Email)
                    .HasMaxLength(128)
                    .HasColumnName("email")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Firstname)
                    .HasMaxLength(32)
                    .HasColumnName("firstname")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Gamefeatures)
                    .HasColumnType("int(11)")
                    .HasColumnName("gamefeatures")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Ipaddress)
                    .HasMaxLength(16)
                    .HasColumnName("ipaddress")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Lastlogin)
                    .HasColumnName("lastlogin")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Lastname)
                    .HasMaxLength(32)
                    .HasColumnName("lastname")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Midinitial)
                    .HasMaxLength(16)
                    .HasColumnName("midinitial")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Partime)
                    .HasColumnType("int(11)")
                    .HasColumnName("partime")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Pwhash)
                    .IsRequired()
                    .HasColumnType("binary(60)")
                    .HasColumnName("pwhash");

                entity.Property(e => e.Result)
                    .HasColumnType("int(11)")
                    .HasColumnName("result")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Sex)
                    .HasMaxLength(16)
                    .HasColumnName("sex")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Subfeatures)
                    .HasColumnType("int(11)")
                    .HasColumnName("subfeatures")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Subtime)
                    .HasColumnType("int(11)")
                    .HasColumnName("subtime")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Unknown1)
                    .HasMaxLength(32)
                    .HasColumnName("unknown1")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Unknown2)
                    .HasMaxLength(32)
                    .HasColumnName("unknown2")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Zip)
                    .HasMaxLength(16)
                    .HasColumnName("zip")
                    .HasDefaultValueSql("'NULL'");
            });

            modelBuilder.Entity<CharInventory>(entity =>
            {
                entity.HasKey(e => e.Itemid)
                    .HasName("PRIMARY");

                entity.ToTable("charInventory");

                entity.HasIndex(e => e.Patternid, "inventory_patternid");

                entity.HasIndex(e => e.Serverid, "inventory_serverid_idx");

                entity.HasIndex(e => e.Itemid, "itemid_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.Itemid)
                    .HasColumnType("int(11)")
                    .HasColumnName("itemid");

                entity.Property(e => e.Equiploc)
                    .HasColumnType("int(11)")
                    .HasColumnName("equiploc");

                entity.Property(e => e.Listnumber)
                    .HasColumnType("int(11)")
                    .HasColumnName("listnumber");

                entity.Property(e => e.Location)
                    .HasColumnType("int(11)")
                    .HasColumnName("location");

                entity.Property(e => e.Patternid)
                    .HasColumnType("int(11)")
                    .HasColumnName("patternid");

                entity.Property(e => e.RemainHp)
                    .HasColumnType("int(11)")
                    .HasColumnName("remainHP");

                entity.Property(e => e.Remaincharge)
                    .HasColumnType("int(11)")
                    .HasColumnName("remaincharge");

                entity.Property(e => e.Serverid)
                    .HasColumnType("int(11)")
                    .HasColumnName("serverid");

                entity.Property(e => e.Stackleft)
                    .HasColumnType("int(11)")
                    .HasColumnName("stackleft");

                entity.HasOne(d => d.Pattern)
                    .WithMany(p => p.CharInventories)
                    .HasForeignKey(d => d.Patternid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("inventory_patternid");

                entity.HasOne(d => d.Server)
                    .WithMany(p => p.CharInventories)
                    .HasPrincipalKey(p => p.Serverid)
                    .HasForeignKey(d => d.Serverid)
                    .HasConstraintName("inventory_serverid");
            });

            modelBuilder.Entity<Character>(entity =>
            {
                entity.HasKey(e => new { e.Serverid, e.CharName })
                    .HasName("PRIMARY");

                entity.HasIndex(e => e.Accountid, "CharAccount_idx");

                entity.HasIndex(e => e.Modelid, "CharModel_idx");

                entity.HasIndex(e => e.CharName, "charName_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.Serverid, "serverid_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.Serverid)
                    .HasColumnType("int(11)")
                    .ValueGeneratedOnAdd()
                    .HasColumnName("serverid");

                entity.Property(e => e.CharName)
                    .HasMaxLength(32)
                    .HasColumnName("charName");

                entity.Property(e => e.Ac)
                    .HasColumnType("int(11)")
                    .HasColumnName("ac");

                entity.Property(e => e.Accountid)
                    .HasColumnType("int(11)")
                    .HasColumnName("accountid");

                entity.Property(e => e.Agility)
                    .HasColumnType("int(4)")
                    .HasColumnName("agility");

                entity.Property(e => e.Arcaner)
                    .HasColumnType("int(11)")
                    .HasColumnName("arcaner");

                entity.Property(e => e.BankTunar)
                    .HasColumnType("int(11)")
                    .HasColumnName("bankTunar");

                entity.Property(e => e.BaseAgility)
                    .HasColumnType("int(4)")
                    .HasColumnName("base_agility");

                entity.Property(e => e.BaseCharisma)
                    .HasColumnType("int(4)")
                    .HasColumnName("base_charisma");

                entity.Property(e => e.BaseDexterity)
                    .HasColumnType("int(4)")
                    .HasColumnName("base_dexterity");

                entity.Property(e => e.BaseHp)
                    .HasColumnType("int(11)")
                    .HasColumnName("baseHP");

                entity.Property(e => e.BaseIntel)
                    .HasColumnType("int(4)")
                    .HasColumnName("base_intel");

                entity.Property(e => e.BasePower)
                    .HasColumnType("int(11)")
                    .HasColumnName("basePower");

                entity.Property(e => e.BaseStamina)
                    .HasColumnType("int(4)")
                    .HasColumnName("base_stamina");

                entity.Property(e => e.BaseStrength)
                    .HasColumnType("int(4)")
                    .HasColumnName("base_strength");

                entity.Property(e => e.BaseWisdom)
                    .HasColumnType("int(4)")
                    .HasColumnName("base_wisdom");

                entity.Property(e => e.Breath)
                    .HasColumnType("int(11)")
                    .HasColumnName("breath");

                entity.Property(e => e.Charisma)
                    .HasColumnType("int(4)")
                    .HasColumnName("charisma");

                entity.Property(e => e.ClassIcon)
                    .HasColumnType("int(11)")
                    .HasColumnName("classIcon");

                entity.Property(e => e.Coldr)
                    .HasColumnType("int(11)")
                    .HasColumnName("coldr");

                entity.Property(e => e.CurrentHp)
                    .HasColumnType("int(11)")
                    .HasColumnName("currentHP");

                entity.Property(e => e.CurrentHp2)
                    .HasColumnType("int(11)")
                    .HasColumnName("currentHP2");

                entity.Property(e => e.CurrentPower)
                    .HasColumnType("int(11)")
                    .HasColumnName("currentPower");

                entity.Property(e => e.CurrentPower2)
                    .HasColumnType("int(11)")
                    .HasColumnName("currentPower2");

                entity.Property(e => e.Debt)
                    .HasColumnType("int(11)")
                    .HasColumnName("debt");

                entity.Property(e => e.Dexterity)
                    .HasColumnType("int(4)")
                    .HasColumnName("dexterity");

                entity.Property(e => e.Diseaser)
                    .HasColumnType("int(11)")
                    .HasColumnName("diseaser");

                entity.Property(e => e.Faceoption)
                    .HasColumnType("int(11)")
                    .HasColumnName("faceoption");

                entity.Property(e => e.Facing)
                    .HasColumnType("float(17,12)")
                    .HasColumnName("facing");

                entity.Property(e => e.Firer)
                    .HasColumnType("int(11)")
                    .HasColumnName("firer");

                entity.Property(e => e.Fishing)
                    .HasColumnType("int(11)")
                    .HasColumnName("fishing");

                entity.Property(e => e.Haircolor)
                    .HasColumnType("int(11)")
                    .HasColumnName("haircolor");

                entity.Property(e => e.Hairlength)
                    .HasColumnType("int(11)")
                    .HasColumnName("hairlength");

                entity.Property(e => e.Hairstyle)
                    .HasColumnType("int(11)")
                    .HasColumnName("hairstyle");

                entity.Property(e => e.Healot)
                    .HasColumnType("int(11)")
                    .HasColumnName("healot");

                entity.Property(e => e.Healot2)
                    .HasColumnType("int(11)")
                    .HasColumnName("healot2");

                entity.Property(e => e.HumType)
                    .IsRequired()
                    .HasMaxLength(12)
                    .HasColumnName("humType");

                entity.Property(e => e.Intel)
                    .HasColumnType("int(4)")
                    .HasColumnName("intel");

                entity.Property(e => e.Level)
                    .HasColumnType("int(11)")
                    .HasColumnName("level");

                entity.Property(e => e.Lightningr)
                    .HasColumnType("int(11)")
                    .HasColumnName("lightningr");

                entity.Property(e => e.MaxHp)
                    .HasColumnType("int(11)")
                    .HasColumnName("maxHP");

                entity.Property(e => e.MaxPower)
                    .HasColumnType("int(11)")
                    .HasColumnName("maxPower");

                entity.Property(e => e.Modelid)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("modelid");

                entity.Property(e => e.Poisonr)
                    .HasColumnType("int(11)")
                    .HasColumnName("poisonr");

                entity.Property(e => e.Powerot)
                    .HasColumnType("int(11)")
                    .HasColumnName("powerot");

                entity.Property(e => e.Powerot2)
                    .HasColumnType("int(11)")
                    .HasColumnName("powerot2");

                entity.Property(e => e.Race)
                    .HasColumnType("int(11)")
                    .HasColumnName("race");

                entity.Property(e => e.Stamina)
                    .HasColumnType("int(4)")
                    .HasColumnName("stamina");

                entity.Property(e => e.Strength)
                    .HasColumnType("int(4)")
                    .HasColumnName("strength");

                entity.Property(e => e.Tclass)
                    .HasColumnType("int(11)")
                    .HasColumnName("tclass");

                entity.Property(e => e.TotalTp)
                    .HasColumnType("int(11)")
                    .HasColumnName("totalTP");

                entity.Property(e => e.TotalXp)
                    .HasColumnType("int(11)")
                    .HasColumnName("totalXP");

                entity.Property(e => e.Tunar)
                    .HasColumnType("int(11)")
                    .HasColumnName("tunar");

                entity.Property(e => e.Unk421)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk42_1");

                entity.Property(e => e.Unk4210)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk42_10");

                entity.Property(e => e.Unk4211)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk42_11");

                entity.Property(e => e.Unk422)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk42_2");

                entity.Property(e => e.Unk423)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk42_3");

                entity.Property(e => e.Unk424)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk42_4");

                entity.Property(e => e.Unk425)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk42_5");

                entity.Property(e => e.Unk426)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk42_6");

                entity.Property(e => e.Unk427)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk42_7");

                entity.Property(e => e.Unk428)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk42_8");

                entity.Property(e => e.Unk429)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk42_9");

                entity.Property(e => e.Unknown)
                    .HasColumnType("float(17,12)")
                    .HasColumnName("unknown");

                entity.Property(e => e.UnusedTp)
                    .HasColumnType("int(11)")
                    .HasColumnName("unusedTP");

                entity.Property(e => e.Wisdom)
                    .HasColumnType("int(4)")
                    .HasColumnName("wisdom");

                entity.Property(e => e.World)
                    .HasColumnType("int(11)")
                    .HasColumnName("world");

                entity.Property(e => e.X)
                    .HasColumnType("float(17,12)")
                    .HasColumnName("x");

                entity.Property(e => e.Y)
                    .HasColumnType("float(17,12)")
                    .HasColumnName("y");

                entity.Property(e => e.Z)
                    .HasColumnType("float(17,12)")
                    .HasColumnName("z");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Characters)
                    .HasPrincipalKey(p => p.Accountid)
                    .HasForeignKey(d => d.Accountid)
                    .HasConstraintName("accountid");

                entity.HasOne(d => d.Model)
                    .WithMany(p => p.Characters)
                    .HasForeignKey(d => d.Modelid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("modelid");
            });

            modelBuilder.Entity<CharacterModel>(entity =>
            {
                entity.HasKey(e => e.Modelid)
                    .HasName("PRIMARY");

                entity.ToTable("characterModel");

                entity.Property(e => e.Modelid)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("modelid");

                entity.Property(e => e.Race)
                    .IsRequired()
                    .HasMaxLength(8)
                    .HasColumnName("race");

                entity.Property(e => e.Sex)
                    .IsRequired()
                    .HasMaxLength(16)
                    .HasColumnName("sex");
            });

            modelBuilder.Entity<DefaultCharacter>(entity =>
            {
                entity.ToTable("defaultCharacter");

                entity.Property(e => e.DefaultCharacterId)
                    .HasColumnType("int(11)")
                    .HasColumnName("defaultCharacterID");

                entity.Property(e => e.Agility)
                    .HasColumnType("int(11)")
                    .HasColumnName("agility")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.BankTunar)
                    .HasColumnType("int(11)")
                    .HasColumnName("bankTunar");

                entity.Property(e => e.Charisma)
                    .HasColumnType("int(11)")
                    .HasColumnName("charisma")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Dexterity)
                    .HasColumnType("int(11)")
                    .HasColumnName("dexterity")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Facing).HasColumnName("facing");

                entity.Property(e => e.HumType)
                    .IsRequired()
                    .HasMaxLength(12)
                    .HasColumnName("humType");

                entity.Property(e => e.Intel)
                    .HasColumnType("int(11)")
                    .HasColumnName("intel")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Level)
                    .HasColumnType("int(11)")
                    .HasColumnName("level");

                entity.Property(e => e.Race)
                    .IsRequired()
                    .HasMaxLength(8)
                    .HasColumnName("race");

                entity.Property(e => e.Stamina)
                    .HasColumnType("int(11)")
                    .HasColumnName("stamina")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Strength)
                    .HasColumnType("int(11)")
                    .HasColumnName("strength")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Tclass)
                    .IsRequired()
                    .HasMaxLength(6)
                    .HasColumnName("tclass");

                entity.Property(e => e.TotalTp)
                    .HasColumnType("int(11)")
                    .HasColumnName("totalTP");

                entity.Property(e => e.Tunar)
                    .HasColumnType("int(11)")
                    .HasColumnName("tunar");

                entity.Property(e => e.UnusedTp)
                    .HasColumnType("int(11)")
                    .HasColumnName("unusedTP");

                entity.Property(e => e.Wisdom)
                    .HasColumnType("int(11)")
                    .HasColumnName("wisdom")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.World)
                    .HasColumnType("int(11)")
                    .HasColumnName("world")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.X).HasColumnName("x");

                entity.Property(e => e.Y).HasColumnName("y");

                entity.Property(e => e.Z)
                    .HasColumnName("z")
                    .HasDefaultValueSql("'NULL'");
            });

            modelBuilder.Entity<DefaultSpell>(entity =>
            {
                entity.HasKey(e => e.DefSpellId)
                    .HasName("PRIMARY");

                entity.ToTable("defaultSpells");

                entity.HasIndex(e => e.Spellid, "defspell_spellid_idx");

                entity.Property(e => e.DefSpellId)
                    .HasColumnType("int(11)")
                    .HasColumnName("def_spell_id");

                entity.Property(e => e.Addedorder)
                    .HasColumnType("int(11)")
                    .HasColumnName("addedorder");

                entity.Property(e => e.OnHotBar)
                    .HasColumnType("int(11)")
                    .HasColumnName("onHotBar");

                entity.Property(e => e.Showhide)
                    .HasColumnType("int(11)")
                    .HasColumnName("showhide");

                entity.Property(e => e.Spellid)
                    .HasColumnType("int(11)")
                    .HasColumnName("spellid");

                entity.Property(e => e.Tclass)
                    .IsRequired()
                    .HasMaxLength(6)
                    .HasColumnName("tclass");

                entity.Property(e => e.Unk1)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk1");

                entity.Property(e => e.WhereonBar)
                    .HasColumnType("int(11)")
                    .HasColumnName("whereonBar");

                entity.HasOne(d => d.Spell)
                    .WithMany(p => p.DefaultSpells)
                    .HasForeignKey(d => d.Spellid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("defspell_spellid");
            });

            modelBuilder.Entity<DefaultcharInventory>(entity =>
            {
                entity.HasKey(e => e.Itemid)
                    .HasName("PRIMARY");

                entity.ToTable("defaultcharInventory");

                entity.HasIndex(e => e.Patternid, "defcharinv_patternid_idx");

                entity.Property(e => e.Itemid)
                    .HasColumnType("int(11)")
                    .HasColumnName("itemid");

                entity.Property(e => e.Equiploc)
                    .HasColumnType("int(11)")
                    .HasColumnName("equiploc");

                entity.Property(e => e.Listnumber)
                    .HasColumnType("int(11)")
                    .HasColumnName("listnumber");

                entity.Property(e => e.Location)
                    .HasColumnType("int(11)")
                    .HasColumnName("location");

                entity.Property(e => e.Patternid)
                    .HasColumnType("int(11)")
                    .HasColumnName("patternid");

                entity.Property(e => e.RemainHp)
                    .HasColumnType("int(11)")
                    .HasColumnName("remainHP");

                entity.Property(e => e.Remaincharge)
                    .HasColumnType("int(11)")
                    .HasColumnName("remaincharge");

                entity.Property(e => e.Stackleft)
                    .HasColumnType("int(11)")
                    .HasColumnName("stackleft");

                entity.Property(e => e.Tclass)
                    .IsRequired()
                    .HasMaxLength(6)
                    .HasColumnName("tclass");

                entity.HasOne(d => d.Pattern)
                    .WithMany(p => p.DefaultcharInventories)
                    .HasForeignKey(d => d.Patternid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("defcharinv_patternid");
            });

            modelBuilder.Entity<Hotkey>(entity =>
            {
                entity.HasIndex(e => e.Hotkeyid, "hotkeyid_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.Serverid, "serverid_hotkey_idx");

                entity.Property(e => e.Hotkeyid)
                    .HasColumnType("int(11)")
                    .HasColumnName("hotkeyid");

                entity.Property(e => e.Direction)
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasColumnName("direction");

                entity.Property(e => e.Elabel)
                    .HasMaxLength(128)
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Emessage)
                    .HasMaxLength(128)
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Nlabel)
                    .HasMaxLength(128)
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Nmessage)
                    .HasMaxLength(128)
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Serverid)
                    .HasColumnType("int(11)")
                    .HasColumnName("serverid");

                entity.Property(e => e.Slabel)
                    .HasMaxLength(128)
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Smessage)
                    .HasMaxLength(128)
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Wlabel)
                    .HasMaxLength(128)
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Wmessage)
                    .HasMaxLength(128)
                    .HasDefaultValueSql("'NULL'");

                entity.HasOne(d => d.Server)
                    .WithMany(p => p.Hotkeys)
                    .HasPrincipalKey(p => p.Serverid)
                    .HasForeignKey(d => d.Serverid)
                    .HasConstraintName("serverid_hotkey");
            });

            modelBuilder.Entity<ItemPattern>(entity =>
            {
                entity.HasKey(e => e.Patternid)
                    .HasName("PRIMARY");

                entity.ToTable("itemPattern");

                entity.HasIndex(e => e.Patternid, "patternid_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.Patternid)
                    .HasColumnType("int(11)")
                    .HasColumnName("patternid");

                entity.Property(e => e.Ac)
                    .HasColumnType("int(11)")
                    .HasColumnName("AC")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Agi)
                    .HasColumnType("int(11)")
                    .HasColumnName("agi")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Ar)
                    .HasColumnType("int(11)")
                    .HasColumnName("AR")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Attacktype)
                    .HasColumnType("int(11)")
                    .HasColumnName("attacktype");

                entity.Property(e => e.Cha)
                    .HasColumnType("int(11)")
                    .HasColumnName("cha")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Classuse)
                    .HasColumnType("int(11)")
                    .HasColumnName("classuse");

                entity.Property(e => e.Color)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("color");

                entity.Property(e => e.Cr)
                    .HasColumnType("int(11)")
                    .HasColumnName("CR")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Craft)
                    .HasColumnType("int(11)")
                    .HasColumnName("craft");

                entity.Property(e => e.Dex)
                    .HasColumnType("int(11)")
                    .HasColumnName("dex")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Dr)
                    .HasColumnType("int(11)")
                    .HasColumnName("DR")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Duration)
                    .HasColumnType("int(11)")
                    .HasColumnName("duration");

                entity.Property(e => e.Equipslot)
                    .HasColumnType("int(11)")
                    .HasColumnName("equipslot");

                entity.Property(e => e.Fr)
                    .HasColumnType("int(11)")
                    .HasColumnName("FR")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.HoT)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Hpmax)
                    .HasColumnType("int(11)")
                    .HasColumnName("HPMAX")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Int)
                    .HasColumnType("int(11)")
                    .HasColumnName("int")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Itemdesc)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("itemdesc");

                entity.Property(e => e.Itemicon)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("itemicon");

                entity.Property(e => e.Itemname)
                    .IsRequired()
                    .HasMaxLength(32)
                    .HasColumnName("itemname");

                entity.Property(e => e.Levelreq)
                    .HasColumnType("int(11)")
                    .HasColumnName("levelreq");

                entity.Property(e => e.Lore)
                    .HasColumnType("int(11)")
                    .HasColumnName("lore");

                entity.Property(e => e.Lr)
                    .HasColumnType("int(11)")
                    .HasColumnName("LR")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Maxhp)
                    .HasColumnType("int(11)")
                    .HasColumnName("maxhp");

                entity.Property(e => e.Maxstack)
                    .HasColumnType("int(11)")
                    .HasColumnName("maxstack");

                entity.Property(e => e.Model)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("model");

                entity.Property(e => e.Patternfam)
                    .HasColumnType("int(11)")
                    .HasColumnName("patternfam");

                entity.Property(e => e.PoT)
                    .HasColumnType("int(11)")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Powmax)
                    .HasColumnType("int(11)")
                    .HasColumnName("POWMAX")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Pr)
                    .HasColumnType("int(11)")
                    .HasColumnName("PR")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Procanim)
                    .HasColumnType("int(11)")
                    .HasColumnName("procanim");

                entity.Property(e => e.Raceuse)
                    .HasColumnType("int(11)")
                    .HasColumnName("raceuse");

                entity.Property(e => e.Rent)
                    .HasColumnType("int(11)")
                    .HasColumnName("rent");

                entity.Property(e => e.Sta)
                    .HasColumnType("int(11)")
                    .HasColumnName("sta")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Str)
                    .HasColumnType("int(11)")
                    .HasColumnName("str")
                    .HasDefaultValueSql("'NULL'");

                entity.Property(e => e.Trade)
                    .HasColumnType("int(11)")
                    .HasColumnName("trade");

                entity.Property(e => e.Unk1)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk1");

                entity.Property(e => e.Unk2)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk2");

                entity.Property(e => e.Unk3)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk3");

                entity.Property(e => e.Unk4)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk4");

                entity.Property(e => e.Unk5)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk5");

                entity.Property(e => e.Unk6)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk6");

                entity.Property(e => e.Weapondamage)
                    .HasColumnType("int(11)")
                    .HasColumnName("weapondamage");

                entity.Property(e => e.Wis)
                    .HasColumnType("int(11)")
                    .HasColumnName("wis")
                    .HasDefaultValueSql("'NULL'");
            });

            modelBuilder.Entity<Spell>(entity =>
            {
                entity.HasIndex(e => e.Serverid, "spell_serverid_idx");

                entity.Property(e => e.Spellid)
                    .HasColumnType("int(11)")
                    .ValueGeneratedOnAdd()
                    .HasColumnName("spellid");

                entity.Property(e => e.Addedorder)
                    .HasColumnType("int(11)")
                    .HasColumnName("addedorder");

                entity.Property(e => e.OnHotBar)
                    .HasColumnType("int(11)")
                    .HasColumnName("onHotBar");

                entity.Property(e => e.Serverid)
                    .HasColumnType("int(11)")
                    .HasColumnName("serverid");

                entity.Property(e => e.Showhide)
                    .HasColumnType("int(11)")
                    .HasColumnName("showhide");

                entity.Property(e => e.Unk1)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk1");

                entity.Property(e => e.WhereonBar)
                    .HasColumnType("int(11)")
                    .HasColumnName("whereonBar");

                entity.HasOne(d => d.Server)
                    .WithMany(p => p.Spells)
                    .HasPrincipalKey(p => p.Serverid)
                    .HasForeignKey(d => d.Serverid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("spell_serverid");

                entity.HasOne(d => d.SpellNavigation)
                    .WithOne(p => p.Spell)
                    .HasForeignKey<Spell>(d => d.Spellid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("spell_spellid");
            });

            modelBuilder.Entity<SpellPattern>(entity =>
            {
                entity.HasKey(e => e.Spellid)
                    .HasName("PRIMARY");

                entity.ToTable("spellPattern");

                entity.HasIndex(e => e.Spellid, "spellid_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.Spellid)
                    .HasColumnType("int(11)")
                    .HasColumnName("spellid");

                entity.Property(e => e.Abilitylvl)
                    .HasColumnType("int(11)")
                    .HasColumnName("abilitylvl");

                entity.Property(e => e.Casttime)
                    .HasColumnType("int(11)")
                    .HasColumnName("casttime");

                entity.Property(e => e.Eqprequire)
                    .HasColumnType("int(11)")
                    .HasColumnName("eqprequire");

                entity.Property(e => e.Icon)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("icon");

                entity.Property(e => e.IconColor)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("iconColor");

                entity.Property(e => e.Power)
                    .HasColumnType("int(11)")
                    .HasColumnName("power");

                entity.Property(e => e.Range)
                    .HasColumnType("int(11)")
                    .HasColumnName("range");

                entity.Property(e => e.Recast)
                    .HasColumnType("int(11)")
                    .HasColumnName("recast");

                entity.Property(e => e.Scope)
                    .HasColumnType("int(11)")
                    .HasColumnName("scope");

                entity.Property(e => e.Spelldesc)
                    .IsRequired()
                    .HasMaxLength(256)
                    .HasColumnName("spelldesc");

                entity.Property(e => e.Spellname)
                    .IsRequired()
                    .HasMaxLength(32)
                    .HasColumnName("spellname");

                entity.Property(e => e.Unk2)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk2");

                entity.Property(e => e.Unk3)
                    .HasColumnType("int(11)")
                    .HasColumnName("unk3");
            });

            modelBuilder.Entity<WeaponHotBar>(entity =>
            {
                entity.HasKey(e => e.WeaponsetId)
                    .HasName("PRIMARY");

                entity.ToTable("weaponHotBar");

                entity.HasIndex(e => e.Serverid, "weaponhotbar_serverid_idx");

                entity.HasIndex(e => e.WeaponsetId, "weaponsetID_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.WeaponsetId)
                    .HasColumnType("int(11)")
                    .HasColumnName("weaponsetID");

                entity.Property(e => e.Hotbarname)
                    .IsRequired()
                    .HasMaxLength(64)
                    .HasColumnName("hotbarname");

                entity.Property(e => e.SecondaryId)
                    .HasColumnType("int(11)")
                    .HasColumnName("secondaryID");

                entity.Property(e => e.Serverid)
                    .HasColumnType("int(11)")
                    .HasColumnName("serverid");

                entity.Property(e => e.WeaponId)
                    .HasColumnType("int(11)")
                    .HasColumnName("weaponID");

                entity.HasOne(d => d.Server)
                    .WithMany(p => p.WeaponHotBars)
                    .HasPrincipalKey(p => p.Serverid)
                    .HasForeignKey(d => d.Serverid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("weaponhotbar_serverid");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
