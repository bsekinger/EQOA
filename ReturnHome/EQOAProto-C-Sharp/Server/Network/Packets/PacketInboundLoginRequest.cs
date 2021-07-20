using System;
using ReturnHome.Database.Models.Auth;
using ReturnHome.Utilities;

namespace ReturnHome.Server.Network.Packets
{
    //read and ingest this packet
    public class PacketInboundLoginRequest
    {
        //public NetAuthType NetAuthType { get; }
        private int offset = 0;
        public int AccountNameLength { get; }
        public string AccountName { get; }
        private ReadOnlyMemory<byte> PasswordArray;
        private string Password;
        private int _EQOACheckLength;
        private string _EQOACheck = "EQOA";
        public bool EQOACheck { get; }

        public PacketInboundLoginRequest(PacketMessage message)
        {
            //Skip first 5 bytes
            offset += 7;

            (_EQOACheckLength, offset)  = BinaryPrimitiveWrapper.GetLEInt(message.Data, offset);
            if (_EQOACheck == Utility_Funcs.GetMemoryString(message.Data.Span, offset, _EQOACheckLength))
            {
                offset += _EQOACheckLength;

                (AccountNameLength, offset) = BinaryPrimitiveWrapper.GetLEInt(message.Data, offset);
                AccountName = Utility_Funcs.GetMemoryString(message.Data.Span, offset, AccountNameLength);
                offset += AccountNameLength;
                //This should be 0x01 at this point if not... fail out
                if (message.Data.Span[offset] == 1)
                {
                    offset += 1;

                    PasswordArray = message.Data.Slice(offset, 32);
                    offset += 32;
                    EQOACheck = true;
                    return;
                }
                EQOACheck = false;
            }
            EQOACheck = false;
        }

        public static bool VerifyPassword(out Account account)
        {
            account = new Account();
            account.AccessLevel = 0;
            account.AccountId = 1;
            account.AccountName = "Test123";
            //Do database stuff here to verify password and return Account
            return true;
        }
    }
}
