using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace EC
{
    class ECProxyHandler : IProxyHandler
    {
        private Dictionary<string, MethodReturnArgs> mCallBackHandlers = new Dictionary<string, MethodReturnArgs>();

        public Result Execute(RemoteInvokeArgs info)
        {
            MethodReturnArgs returnArgs = info.CommunicationObject.Channel.Pop();
            IClient client = info.CommunicationObject.Client;
            List<Message> sendDatas = new List<Message>();
            RPC.MethodCall call = new RPC.MethodCall();
            Result result = new Result();
            result.Status = ResultStatus.Success;
            try
            {
                info.CommunicationObject.Channel.RegisterRemote(call.ID, returnArgs);
                call.Service = info.Interface;
                call.Method = info.Method;
                call.Parameters = info.ParameterTypes;
                sendDatas.Add(new Message(call, typeof(RPC.MethodCall)));
                if (info.Parameters != null)
                    for (int i = 0; i < info.Parameters.Length; i++)
                    {
                        Type ptype = info.ParameterInfos[i].ParameterType;
                        sendDatas.Add(new Message(info.Parameters[i], ptype.Name.IndexOf('&') == -1 ? ptype : ptype.GetElementType()));
                    }

                IData sdata = ((PacketBase)client.Packet).GetMessagesData(sendDatas);
                LastRemotingData = sdata;
                if (!client.Send(sdata))
                {
                    throw client.LastError;
                }
                returnArgs.Status = InvokeStatus.Receiving;
                if (!returnArgs.Wait())
                    throw new Exception("invoke timeout!");
                if (returnArgs.MethodResult.Status == ResultStatus.Error)
                    throw new Exception(returnArgs.MethodResult.Error);
                if (!returnArgs.MethodResult.IsVoid)
                    result.Data = returnArgs.Result;
                if (returnArgs.ParameterNames.Count > 0)
                {
                    for (int i = 0; i < returnArgs.ParameterNames.Count; i++)
                    {
                        result.Parameters[returnArgs.ParameterNames[i]] = returnArgs.Parameters[i];
                    }
                }
                return result;

            }
            catch (Exception e_)
            {
                throw new ECException(string.Format("remote access {0}.{1} error {2}", info.Interface, info.Method, e_.Message));
            }
            finally
            {
                info.CommunicationObject.Channel.UnRegisterRemote(call.ID);
                info.CommunicationObject.Channel.Push(returnArgs);
            }
        }

        public IData LastRemotingData
        {
            get;
            set;
        }
    }
}
