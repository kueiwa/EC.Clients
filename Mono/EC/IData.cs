using System;

namespace EC
{
	public interface IData
	{
		byte[] Array{get;set;}

		int Offset{get;set;}

		int Length{get;set;}
	}
}

