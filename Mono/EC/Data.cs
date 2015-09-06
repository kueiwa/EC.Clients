using System;

namespace EC
{
	public class Data:IData
	{
		public Data ()
		{
		}

		public Data(byte[] array,int offset,int length)
		{
			this.Array = array;
			this.Offset = offset;
			this.Length = length;
		}

		public byte[] Array{get;set;}

		public int Offset{get;set;}

		public int Length{get;set;}
	}
}

