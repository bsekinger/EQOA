using System;
using System.IO;
using System.Text;

using ReturnHome.Utilities;

namespace ReturnHome.Server.EntityObject.Player
{
    public class Hotkey
    {
        public string Direction { get; private set; }
        public string NLabel { get; private set; }
        public string NMessage { get; private set; }
        public string WLabel { get; private set; }
        public string WMessage { get; private set; }
        public string ELabel { get; private set; }
        public string EMessage { get; private set; }
        public string SLabel { get; private set; }
        public string SMessage { get; private set; }

        public Hotkey()
        { }

        //Will instantiate a Hotkey object
        public Hotkey(string thisDirection, string thisNMessage, string thisNLabel, string thisWMessage, string thisWLabel, string thisEMessage, string thisELabel, string thisSMessage, string thisSLabel)
        {
            Direction = thisDirection;
            NLabel = thisNLabel;
            NMessage = thisNMessage;
            WLabel = thisWLabel;
            WMessage = thisWMessage;
            ELabel = thisELabel;
            EMessage = thisEMessage;
            SLabel = thisSLabel;
            SMessage = thisSMessage;
        }

        public void PullHotkey(MemoryStream memStream)
        {
            //Packdata in list here and return it to calling method
            //Get directions integer value and perform technique
            memStream.Write(Utility_Funcs.DoublePack(HotKeyFuncs.OutHoingHotkeyDict[Direction])); 

            //North HK
            memStream.WriteByte(0);
            ConvertHotKey(memStream, NLabel, NMessage);

            //West HK
            memStream.WriteByte(2);
            ConvertHotKey(memStream, WLabel, WMessage);

            //East HK
            memStream.WriteByte(4);
            ConvertHotKey(memStream, ELabel, EMessage);

            //South HK
            memStream.WriteByte(6);
            ConvertHotKey(memStream, SLabel, SMessage);
        }

        private void ConvertHotKey(MemoryStream memStream, string label, string message)
        {
            //If message
            if(label != null)
            {
                //Add string length, 4 bytes then string as utf-16-le
                memStream.Write(BitConverter.GetBytes(label.Length));
                memStream.Write(Encoding.Unicode.GetBytes(label));

                //If label
                if (message != null)
                {
                    //Add string length, 4 bytes then string as utf-16-le
                    memStream.Write(BitConverter.GetBytes(message.Length));
                    memStream.Write(Encoding.Unicode.GetBytes(message));
                }

                //no label
                else
                {
                    //Length of 0, 4 bytes
                    memStream.Write(BitConverter.GetBytes(0));
                }
            }

            //No message
            else
            {
                //Length of 0, 4 bytes
                memStream.Write(BitConverter.GetBytes(0));

                //If label
                if (message != null)
                {
                    //Add string length, 4 bytes then string as utf-16-le
                    memStream.Write(BitConverter.GetBytes(message.Length));
                    memStream.Write(Encoding.Unicode.GetBytes(message));
                }

                //no label
                else
                {
                    //Length of 0, 4 bytes
                    memStream.Write(BitConverter.GetBytes(0));
                }
            }
        }
    }
}
