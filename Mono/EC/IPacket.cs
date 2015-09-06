using System;
using System.IO;

namespace EC
{
	public interface IPacket
	{
		IClient Channel { get; set; }
		EventPackageReceive Receive { get; set; }
		object GetMessage(Stream stream);
		IData GetMessageData(object message);
		IData GetMessagesData(System.Collections.Generic.IList<Message> messages);
		void Import(byte[] data, int start, int count);
		void Recover(IData data);
	}
}

