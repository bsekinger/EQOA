using System.Collections.Generic;
using System.IO;
using ReturnHome.eqoabase;
using ReturnHome.Utilities;
using System.Linq;
using System;

namespace ReturnHome.Server.Network.GameMessages.Messages
{
    class CharacterList : GameMessage
    {
        public CharacterList(Session session) : base(MessageType.ReliableMessage, GameMessageOpcode.Camera2, GameMessageGroup.SecureWeenieQueue)
        {
            //List<CharacterListCharacter> characterList = new List<CharacterListCharacter>();
            //Query for our characters
            using (var cont = new eqoabaseContext())
            {
                var charList = cont.Characters.Where(c => c.Accountid == session.AccountID).ToList();
                Console.WriteLine();
            }

                //Write total character count
                /*Utility_Funcs.DoublePack(Writer, characterList.Count);

            foreach (CharacterListCharacter character in characterList)
            {
                character.CollectCharacter(Writer);
            }*/
        }
    }

    public record CharacterListCharacter
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

        public CharacterListCharacter(string _charName, int _serverID, int _modelID, int _Class, int _race, int _level, int _hairColor, int _hairLength, int _hairStyle, int _faceOption)
        {
            charName = _charName;
            serverID = _serverID;
            modelID = _modelID;
            Class = _Class;
            race = _race;
            level = _level;
            hairColor = _hairColor;
            hairLength = _hairLength;
            hairStyle = _hairStyle;
            faceOption = _faceOption;
        }

        public void CollectCharacter(BinaryWriter Writer)
        {
            Writer.Write(charName.Length);
            Writer.Write(charName);
            Utility_Funcs.DoublePack(Writer, serverID);
            Utility_Funcs.DoublePack(Writer, modelID);
            Utility_Funcs.DoublePack(Writer, Class);
            Utility_Funcs.DoublePack(Writer, race);
            Utility_Funcs.DoublePack(Writer, level);
            Utility_Funcs.DoublePack(Writer, hairColor);
            Utility_Funcs.DoublePack(Writer, hairLength);
            Utility_Funcs.DoublePack(Writer, hairStyle);
            Utility_Funcs.DoublePack(Writer, faceOption);
            //Writer.Write(robeType);
            //Writer.Write(primaryWeapon);
            //Writer.Write(secondaryWeapon);
            //Writer.Write(shield);
            Writer.Write(0x0004); //This is character animation. Some how using the types of weapons equipped... we should be able to deduce the animation here, eventually.
            Writer.Write((byte)0x00); //Some unknown value, packet analysis indicated it is always 0
            //Writer.Write((byte)chest);
           // Writer.Write((byte)bracer);
           // Writer.Write((byte)gloves);
            //Writer.Write((byte)legs);
           // Writer.Write((byte)boots);
           // Writer.Write((byte)helm);
           // Writer.Write((byte)chest);
            Writer.Write(0x00); //Another unknown value, seems to be 4 bytes long
            Writer.Write((short)0x00); //Additionally unknown value, seems to be 2 bytes long
            Writer.Write(0xFFFFFFFF); //Unknown value? Seems to always be this
            Writer.Write(0xFFFFFFFF); //Unknown value? Seems to always be this
            Writer.Write(0xFFFFFFFF); //Unknown value? Seems to always be this
           // Writer.Write(ByteSwaps.SwapBytes(chestColor));
           // Writer.Write(ByteSwaps.SwapBytes(bracerColor));
            //Writer.Write(ByteSwaps.SwapBytes(gloveColor));
            //Writer.Write(ByteSwaps.SwapBytes(legColor));
            //Writer.Write(ByteSwaps.SwapBytes(bootColor));
            //Writer.Write(ByteSwaps.SwapBytes(helmColor));
            //Writer.Write(ByteSwaps.SwapBytes(robeColor));
        }
    }
}
