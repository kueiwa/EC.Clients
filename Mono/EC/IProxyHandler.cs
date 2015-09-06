using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EC
{
    interface IProxyHandler
    {

      
        Result Execute(RemoteInvokeArgs info);
    }
}
