
namespace ReturnHome.Server.Network.GameMessages
{
    public abstract class GameMessage
    {
		public byte Messagetype {get; private set;}
		
        public GameMessageOpcode Opcode { get; private set; }

        public System.IO.MemoryStream Data { get; private set; }

        public GameMessageGroup Group { get; private set; }

        protected System.IO.BinaryWriter Writer { get; private set; }

        protected GameMessage(MessageType messageType, GameMessageOpcode opCode, GameMessageGroup group)
        {
			Messagetype = (byte)messageType;
			
            Opcode = opCode;

            Group = group;

            Data = new System.IO.MemoryStream();

            Writer = new System.IO.BinaryWriter(Data);

            
            if(opCode != GameMessageOpcode.None)
                Writer.Write((ushort)Opcode);
        }
    }
}
