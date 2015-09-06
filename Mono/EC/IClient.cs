using System;

namespace EC
{
	public interface IClient
	{
		Boolean Connected { get; }

		Exception LastError{get;}

		bool Connect();

		object this [string key]{ get; set;}

		IPacket Packet{get;set;}

		string Host{get;set;}

		int Port{get;set;}

		void DisConnect();

		EventHandler<DataReceiveArgs> Receive{get;set;}

		bool Send(object data);

		bool SendString(string value);

		bool Send(byte[] data,int offset,int length);

		IData LastSendData
		{
			get;
			set;
		}
	}
}

