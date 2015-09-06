using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EC
{
    class ProxyFactory
    {

        static ProxyFactory()
        {
            CursorFactory = new ProxyFactory();
            CursorFactory.Handler = new ECProxyHandler();

        }

        public static ProxyFactory CursorFactory
        {
            get;
            private set;
        }

        public IProxyHandler Handler { get; set; }

        public Result Execute(ICommunicationObject client, string service, System.Reflection.MethodBase method, params object[] data)
        {


            RemoteInvokeArgs info = new RemoteInvokeArgs();
            info.Interface = service;
            int index = method.Name.LastIndexOf('.');
            index = index>0?(index+1):0;
            info.Method = method.Name.Substring(index, method.Name.Length - index);
            info.Parameters = data;
            info.CommunicationObject = client;
            info.ParameterInfos = method.GetParameters();
            foreach (System.Reflection.ParameterInfo pi in method.GetParameters())
            {
                info.ParameterTypes.Add(pi.ParameterType.Name);
            }
            return Handler.Execute(info);

        }


    }
}
