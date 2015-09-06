﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EC
{
     class ProtobufPacket : PacketBase
    {




        public override object GetMessage(System.IO.Stream stream)
        {
            byte[] typedata = new byte[2];
            stream.Read(typedata, 0, 2);
            short typevalue = BitConverter.ToInt16(typedata, 0);
            Type type = TypeMapper.GetType(typevalue);
			if (type == null)
				throw new Exception (string.Format ("{0} value type notfound", typevalue));
            if (stream.Position == stream.Length)
                return null;
          
            return ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, type);
        }

        public override IData GetMessagesData(IList<Message> messages)
        {
            IData data = null;
            byte[] buffer;
            long index = 0;
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                foreach (Message message in messages)
                {
                    stream.Write(new byte[4], 0, 4);
                    short typevalue = TypeMapper.GetValue(message.Type);
                    if (typevalue == 0)
						throw new Exception (string.Format ("{0} type value not registed", message.Type));
                      
                    byte[] typedata = BitConverter.GetBytes(typevalue);
                    stream.Write(typedata, 0, typedata.Length);
                    if (message.Value != null)
                        ProtoBuf.Meta.RuntimeTypeModel.Default.Serialize(stream, message.Value);
                    buffer = stream.GetBuffer();
                    BitConverter.GetBytes((int)stream.Length - 4 - (int)index).CopyTo(buffer, index);
                    index = stream.Position;
                }
                byte[] array = stream.ToArray();
                data = new Data(array,0, array.Length);
             

            }
            return data;
        }
		public override IData GetMessageData(object message)
        {
			IData data = null;
            using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
            {
                stream.Write(new byte[4], 0, 4);
                short typevalue = TypeMapper.GetValue(message);
                if (typevalue == 0)
					throw new Exception (string.Format ("{0} type value not registed", message.GetType()));
                byte[] typedata = BitConverter.GetBytes(typevalue);
                stream.Write(typedata, 0, typedata.Length);
                ProtoBuf.Meta.RuntimeTypeModel.Default.Serialize(stream, message);
                byte[] array = stream.ToArray();
                data = new Data(array, 0,array.Length);
                BitConverter.GetBytes(array.Length - 4).CopyTo(array, 0);
            }

            return data;
        }


    }
}
