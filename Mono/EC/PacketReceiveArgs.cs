using System;

namespace EC
{
	public delegate void EventPackageReceive(object sender,PacketReceiveArgs e);
	public class PacketReceiveArgs
	{
		public PacketReceiveArgs ()
		{
		}

		public IClient Channel { get; set; }

		public object Message { get; set; }
	}
}

