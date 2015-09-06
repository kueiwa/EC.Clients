using System;

namespace EC
{
	public interface IServiceChannel
	{
		bool Send(object message);

		EventPackageReceive Receive
		{
			get;
			set;
		}

        T CreateInstance<T>();

        Object CreateInstance(Type type);

		IClient Client{ get;}

		Result Execute(ICommunicationObject client, string service, System.Reflection.MethodBase method, params object[] data);

		void RegisterRemote(long id, MethodReturnArgs e);

		void UnRegisterRemote(long id);

		MethodReturnArgs Pop();

		void Push(MethodReturnArgs args);

	}
}

