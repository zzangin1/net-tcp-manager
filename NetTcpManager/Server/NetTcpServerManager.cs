using NetTcpManager.MessageQueue;
using NetTcpManager.Model;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetTcpManager.Server
{
	public class NetTcpServerManager
	{
		#region => Field

		private const int MAX_CLIENT = 10;

		private static NetTcpServerManager _instance;
		private Socket _server;
		private List<Socket> _client;
		private Thread _serverThread;
		private List<Thread> _recvDataThreads;
		private bool _isServerRunning = false;

		#endregion => Field

		#region => Property

		public static NetTcpServerManager Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new NetTcpServerManager();
				}
				return _instance;
			}
		}

		public NetMessageQueueManager MessageQueue { get; set; }

		#endregion => Property

		#region => Constructor

		public NetTcpServerManager()
		{
			_client = new List<Socket>();
			_recvDataThreads = new List<Thread>();
			MessageQueue = new NetMessageQueueManager(true);
			MessageQueue.StartMsgQueueThread();
			MessageQueue.SendToClient = SendData;
		}

		#endregion => Constructor

		#region => Method

		/// <summary>
		/// 서버 시작 메서드
		/// </summary>
		/// <param name="port"></param>
		public void StartServer(int port = 9999)
		{
			_server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_server.Bind(new IPEndPoint(IPAddress.Any, port));
			_server.Listen(MAX_CLIENT); // 최대 대기 클라이언트 수

			_serverThread = new Thread(ServerMonitoring);
			_serverThread.IsBackground = true;
			_serverThread.Start();
		}

		/// <summary>
		/// 서버 종료 메서드
		/// </summary>
		public void StopServer()
		{
			MessageQueue.StopMsgQueueThread();
			StopRecvDataThread();
			StopServerThread();
		}

		/// <summary>
		/// Server Thread 시작할 때 수행할 메서드
		/// </summary>
		private void ServerMonitoring()
		{
			_isServerRunning = true;

			while (_isServerRunning)
			{
				if (_server.Poll(1000, SelectMode.SelectRead))
				{
					Socket client = _server.Accept();

					lock (_client)
					{
						_client.Add(client);
					}

					Thread recvDataThread = new Thread(() => RecvDataMonitoring(client));
					recvDataThread.IsBackground = true;
					recvDataThread.Start();

					lock (_recvDataThreads)
					{
						_recvDataThreads.Add(recvDataThread);
					}
				}
				else
				{
					Thread.Sleep(100);
				}
			}

			_serverThread = null;
		}

		/// <summary>
		/// Recv Data Thread 시작할 때 수행할 메서드
		/// </summary>
		/// <param name="client"></param>
		private void RecvDataMonitoring(Socket client)
		{
			if (client == null) return;

			byte[] data = new byte[1024];

			while (client.Connected)
			{
				if (client.Poll(1000, SelectMode.SelectRead))
				{
					try
					{
						int dataSize = client.Receive(data);

						if (dataSize > 0)
						{
							ProcessRecvData(data, dataSize, client);
						}
					}
					catch
					{
						// 클라이언트와 연결이 끊어졌을 경우 로직
						break;
					}
				}
				else
				{
					Thread.Sleep(100);
				}
			}

			lock (_client)
			{
				_client.Remove(client);
			}

			client.Close();
		}

		/// <summary>
		/// Server Thread 종료 메서드
		/// </summary>
		/// <exception cref="Exception"></exception>
		private void StopServerThread()
		{
			if (_serverThread != null)
			{
				_isServerRunning = false;
				_server.Close();

				var sw = Stopwatch.StartNew();

				while (_serverThread.IsAlive && sw.Elapsed.TotalSeconds <= 5)
				{
					Thread.Sleep(100);
				}

				if (_serverThread.IsAlive)
				{
					throw new Exception("Server : ServerThread 종료 후 5초 경과하였으나 Thread가 살아있습니다.");
				}
			}
		}

		/// <summary>
		/// Recv Data Thread 종료 메서드
		/// </summary>
		private void StopRecvDataThread()
		{
			foreach (var client in _client)
			{
				if (client.Connected)
				{
					client.Shutdown(SocketShutdown.Both);
					client.Close();
				}
			}

			foreach (var recvDataThread in _recvDataThreads)
			{
				if (recvDataThread.IsAlive)
				{
					recvDataThread.Join();
				}
			}

			_client.Clear();
			_recvDataThreads.Clear();
		}

		/// <summary>
		/// 수신한 데이터 처리 메서드
		/// </summary>
		/// <param name="data"></param>
		/// <param name="dataSize"></param>
		/// <param name="client"></param>
		private void ProcessRecvData(byte[] data, int dataSize, Socket client)
		{
			string recvData = Encoding.UTF8.GetString(data, 0, dataSize);
			string processData = string.Empty;

			// 받은 데이터 처리 로직

			// Client에 응답 데이터 전송
			RecvMessage recvMsg = new RecvMessage(recvData);
			SendMessage sendMsg = new SendMessage(processData, client);
			MessageQueue.RecvMsgQueue.Enqueue(recvMsg);
			MessageQueue.SendMsgQueue.Enqueue(sendMsg);
		}

		/// <summary>
		/// 전송 데이터 포맷팅 처리 메서드
		/// </summary>
		/// <param name="message"></param>
		/// <param name="client"></param>
		public void ProcessSendData(string message, Socket client)
		{
			// 데이터 포맷팅 처리 로직


			// SendMsgQueue 추가
			var sendMsg = new SendMessage(message, client);
			MessageQueue.SendMsgQueue.Enqueue((sendMsg));
		}

		/// <summary>
		/// 클라이언트에 메시지 전송 메서드
		/// </summary>
		/// <param name="message"></param>
		/// <param name="client"></param>
		/// <exception cref="Exception"></exception>
		public void SendData(string message, Socket client)
		{
			if (client == null || client.Connected == false) return;

			try
			{
				byte[] data = Encoding.UTF8.GetBytes(message);
				client.Send(data);
			}
			catch
			{
				throw new Exception("Server : Send Data Fail");
			}
		}

		#endregion => Method
	}
}
