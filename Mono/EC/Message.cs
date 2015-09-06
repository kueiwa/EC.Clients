﻿using System;

namespace EC
{
	public struct Message
	{
		public Message(object value, Type type)
		{
			this.Value = value;
			this.Type = type;
		}

		public object Value;

		public Type Type;
	}
}

