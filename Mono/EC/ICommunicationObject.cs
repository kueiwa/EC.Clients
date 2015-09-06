using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EC
{
    public interface ICommunicationObject
    {
        IClient Client { get; set; }
		IServiceChannel Channel{get;set;}
    }
}
