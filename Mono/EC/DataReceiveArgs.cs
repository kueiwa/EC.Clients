using System;

namespace EC
{
	public class DataReceiveArgs
	{
		public DataReceiveArgs ()
		{
		}

		public IClient Channel { get; set; }

		public IData Data { get; set; }

	}

}

