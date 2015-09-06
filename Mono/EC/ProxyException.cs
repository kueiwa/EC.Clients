using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EC
{
    public class ECException:Exception
    {
		public ECException()
        {
        }
		public ECException(string error) : base(error) { }
		public ECException(string error,Exception interError) : base(error,interError) { }
    }
}
