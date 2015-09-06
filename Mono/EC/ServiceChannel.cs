using System;
using System.Collections.Generic;

namespace EC
{
    public class ServiceChannel : IServiceChannel
    {
        public ServiceChannel(string host, int port)
        {
            mClient = new Client(host, port);
            mClient.Packet = new ProtobufPacket();
            mClient.Packet.Receive = OnReceive;
            mPool.Push(new MethodReturnArgs());
            mPool.Push(new MethodReturnArgs());
            mPool.Push(new MethodReturnArgs());
            mPool.Push(new MethodReturnArgs());
            mPool.Push(new MethodReturnArgs());
            

        }
        public IClient Client
        {
            get
            {
                return mClient;
            }
        }

		public  T CreateInstance<T>()
		{
			return (T)CreateInstance (typeof(T));
		}

		public  Object CreateInstance(Type type)
		{
			ICommunicationObject result = (ICommunicationObject)InterfaceBuilder.CreateInstance (type);
            result.Client = mClient;
            result.Channel = this;
			return result;
		}
        public static void Register(System.Reflection.Assembly assembly)
        {
            ProtobufPacket.TypeMapper.Register(assembly);

        }
        public static void Register(short value, Type type)
        {
            ProtobufPacket.TypeMapper.Register(value, type);
        }
        public static void Register<T>(short value)
        {
            ProtobufPacket.TypeMapper.Register<T>(value);
        }
        private IClient mClient;

        private Stack<MethodReturnArgs> mPool = new Stack<MethodReturnArgs>();

        private MethodReturnArgs mMethodReturnArgs = null;

        private Dictionary<long, MethodReturnArgs> mRemotingMethods = new Dictionary<long, MethodReturnArgs>(64);

        private void OnReceive(object sender, PacketReceiveArgs e)
        {
            lock (this)
            {
                if (e.Message is RPC.MethodResult)
                {
                    RPC.MethodResult result = (RPC.MethodResult)e.Message;
                    if (mRemotingMethods.TryGetValue(result.ID, out mMethodReturnArgs))
                    {
                        mMethodReturnArgs.Import(result);
                    }
                }
                else
                {
                    if (mMethodReturnArgs == null)
                    {
                        if (Receive != null)
                            Receive(sender, e);
                    }
                    else
                    {
                        if (mMethodReturnArgs.Status == InvokeStatus.Return)
                        {
                            mMethodReturnArgs.SetReturn(e.Message);
                        }
                        else if (mMethodReturnArgs.Status == InvokeStatus.Parameter)
                        {
                            mMethodReturnArgs.AddParameter(e.Message);
                        }
                    }
                }
                if (mMethodReturnArgs != null && mMethodReturnArgs.Status == InvokeStatus.Completed)
                {
                    mMethodReturnArgs.Completed();
                    mMethodReturnArgs = null;
                }
            }

        }

        bool IServiceChannel.Send(object message)
        {
            return mClient.Send(message);
        }

        public EventPackageReceive Receive
        {
            get;
            set;
        }

        void IServiceChannel.RegisterRemote(long id, MethodReturnArgs e)
        {
            lock (mRemotingMethods)
            {
                mRemotingMethods[id] = e;
            }
        }

        void IServiceChannel.UnRegisterRemote(long id)
        {
            lock (mRemotingMethods)
            {
                mRemotingMethods.Remove(id);
            }
        }

        MethodReturnArgs IServiceChannel.Pop()
        {
            MethodReturnArgs result;
            lock (mPool)
            {
                if (mPool.Count > 0)
                {
                    result = mPool.Pop();
                    result.Reset();
                }
                result = new MethodReturnArgs();
                return result;
            }
        }

        void IServiceChannel.Push(MethodReturnArgs args)
        {
            lock (mPool)
            {
                mPool.Push(args);
            }
        }

        public Result Execute(ICommunicationObject client, string service, System.Reflection.MethodBase method, params object[] data)
        {
            return ProxyFactory.CursorFactory.Execute(client, service, method, data);
        }

       
    }
}

