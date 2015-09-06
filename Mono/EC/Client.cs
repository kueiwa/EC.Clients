using System;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace EC
{
	class Client:IClient
	{
		private int BUFFER_SIZE = 1024 * 4;

		public Client ()
		{
			Connected = false;
			Init ();

		}

		public Client(string host,int port)
		{
			Host = host;
			Port = port;
			Init ();
		}

		private void Init()
		{
			mConnectArgs.Completed += OnConnect;
			mReceiveArgs.SetBuffer (new byte[BUFFER_SIZE], 0, BUFFER_SIZE);
			mReceiveArgs.Completed += OnReveive;
		}

		private IPacket mPacket;

		private Exception mLastError;

		private System.Net.Sockets.SocketAsyncEventArgs mConnectArgs = new System.Net.Sockets.SocketAsyncEventArgs();

		private System.Net.Sockets.SocketAsyncEventArgs mReceiveArgs = new SocketAsyncEventArgs();

		private System.Collections.Generic.Dictionary<string,object> mProperties = new System.Collections.Generic.Dictionary<string, object>();

		private  System.Net.Sockets.Socket mSocket;

		private bool mIsConnecting = false;

		private System.Threading.Semaphore mSemaphore = new System.Threading.Semaphore(0,3);

		private void SetLastError(Exception e)
		{
			mLastError = e;
		}

		private void OnConnect(object sender,SocketAsyncEventArgs e)
		{

			mIsConnecting = false;
			if (e.SocketError == SocketError.Success) {
				SetLastError (null);
				Connected = true;
					BeginReceive ();
			} else {

				try{
					SetLastError( new System.Net.Sockets.SocketException((int)e.SocketError));
				}
				catch{
				}
				DisConnect ();
			}
			if(sender !=null && sender is Socket)
				mSemaphore.Release ();
		}

		private void BeginReceive()
		{
			try
			{
				if(!mSocket.ReceiveAsync(mReceiveArgs))
				{
					OnReveive(null,mReceiveArgs);
				}
			}
			catch(Exception e_) {
				SetLastError (e_);
				DisConnect ();
			}


		}

		private void OnSend(object sender,SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success) {

			} else {
				if (e.BytesTransferred == 0) {
					SetLastError( new ECException ("socket disconnected!"));
				} else {
					try{
						SetLastError( new System.Net.Sockets.SocketException((int)e.SocketError));
					}
					catch{
					}
				}
				DisConnect ();
			}
		}

		private void OnReveive(object sender,SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success && e.BytesTransferred > 0) {
				try{
					if (Packet != null) {
						Packet.Import (e.Buffer, 0, e.BytesTransferred);
					} else {
						if (Receive != null) {
							DataReceiveArgs args = new DataReceiveArgs ();
							args.Channel = this;
							byte[] buffer = new byte[e.BytesTransferred];
							Buffer.BlockCopy (e.Buffer, 0, buffer, 0, e.BytesTransferred);
							args.Data = new Data ();
							args.Data.Array = buffer;
							args.Data.Offset = 0;
							args.Data.Length = e.BytesTransferred;
							Receive (this, args);

						}
					}
				}
				catch(Exception e_) {
					SetLastError (new ECException ("receive data error!",e_));
				}
				finally{
					BeginReceive ();
				}
			} else {
				if (e.BytesTransferred == 0) {
					SetLastError (new ECException ("socket disconnected!"));
				} else {
					try{
						SetLastError( new System.Net.Sockets.SocketException((int)e.SocketError));
					}
					catch{
					}
				}
				DisConnect ();
			}
		}

		public Boolean Connected { get; private set;}

		public Exception LastError{
			get{
				return mLastError;
			}
		}

		public bool Connect()
		{
			lock (this) {
				if (Connected)
					return Connected;
				if (mIsConnecting)
					return Connected;

				try {

					IPAddress address;
					if(!IPAddress.TryParse(Host,out address))
					{
						Task<IPHostEntry> task = Dns.GetHostEntryAsync (Host);
						task.Wait ();
						if (task.IsCompleted) {
							IPHostEntry ips = (IPHostEntry)task.Result;
							address = ips.AddressList[0];
						} else {
							if (task.Exception != null)
								SetLastError(task.Exception);
							Connected= false;
							return Connected;
						}
					}
					mSocket = new System.Net.Sockets.Socket (System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream,
						System.Net.Sockets.ProtocolType.Tcp);
					IPEndPoint poing = new IPEndPoint (address, Port);
						mConnectArgs.RemoteEndPoint = poing;
						mIsConnecting = true;
						if(!mSocket.ConnectAsync(mConnectArgs))
						{
							OnConnect(true,mConnectArgs);
						}
					else
					{
						mSemaphore.WaitOne();
					}

						

				
				
				} catch (Exception e_) {
					mIsConnecting = false;
					SetLastError( e_);
					DisConnect ();
				}
				return Connected;
			}
		}
			
		public object this [string key]
		{
			get{
				object result = null;
				mProperties.TryGetValue (key, out result);
				return result;

			}
			set{
				mProperties [key] = value;
			}
		}

		public IPacket Packet{
			get{
				return mPacket;
			}
			set{
				mPacket = value;
			}

		}

		public string Host{get;set;}

		public int Port{get;set;}

		public void DisConnect()
		{

			if(mSocket !=null)
			{
				try{
					mSocket.Close();
				}
				catch {}
			}
			mSocket =null;
			Connected = false;

		}

		public bool SendString(string value)
		{
			byte[] data = System.Text.Encoding.UTF8.GetBytes (value);
			return Send (data, 0, data.Length);
		}

		public bool Send(byte[] data,int offset,int length)
		{
			return Send (new Data (data, offset, length));
		}

		public EventHandler<DataReceiveArgs> Receive{get;set;}

		public IData LastSendData
		{
			get;
			set;
		}

		public bool Send(object data)
		{
			lock (this) {
				if (this.Connect ()) {
					IData sdata = null;
					if (data is IData) {
						sdata = (IData)data;
					} else {
						if (Packet != null) {
							sdata = Packet.GetMessageData (data);
						}

					}
					if (sdata != null) {
						LastSendData = sdata;
						SocketAsyncEventArgs mSendArgs = new SocketAsyncEventArgs();
						mSendArgs.Completed += OnSend;
						mSendArgs.SetBuffer (sdata.Array, sdata.Offset, sdata.Length);
						if (!mSocket.SendAsync (mSendArgs)) {
							OnSend (null, mSendArgs);
						}
					}
				}

				return Connected;
			}
		}

	}
}

