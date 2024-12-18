using NetTcpManager.Model;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;

namespace NetTcpManager.MessageQueue
{
	public class NetMessageQueueManager
	{
		#region => Field

		private Thread _recvMsgQueueThread;
		private Thread _sendMsgQueueThread;

		private bool _isRecvMsgThreadRunning = false;
		private bool _isSendMsgThreadRunning = false;

		private bool _isServer;

		#endregion => Field

		#region => Delegate

		// 서버 -> 클라이언드 Delegate
		public Action<string, Socket> SendToClient;

		// 클라이언트 -> 서버 Delegate
		public Action<string> SendToServer;

		#endregion => Delegate

		#region => Property

		public ConcurrentQueue<RecvMessage> RecvMsgQueue { get; set; }
		public ConcurrentQueue<SendMessage> SendMsgQueue { get; set; }

		#endregion => Property

		#region => Constructor

		/// <summary>
		/// _isServer = true => Server
		/// _isServer = false => Client
		/// </summary>
		public NetMessageQueueManager(bool isServer)
		{
			RecvMsgQueue = new ConcurrentQueue<RecvMessage>();
			SendMsgQueue = new ConcurrentQueue<SendMessage>();
			_isServer = isServer;
		}

		#endregion => Constructor

		#region => Method

		/// <summary>
		/// Message Queue Thread 시작
		/// </summary>
		public void StartMsgQueueThread()
		{
			_isRecvMsgThreadRunning = true;
			_recvMsgQueueThread = new Thread(RecvMsgQueueMonitoring);
			_recvMsgQueueThread.IsBackground = true;
			_recvMsgQueueThread.Start();

			_isSendMsgThreadRunning = true;
			_sendMsgQueueThread = new Thread(SendMsgQueueMonitoring);
			_sendMsgQueueThread.IsBackground = true;
			_sendMsgQueueThread.Start();
		}

		/// <summary>
		/// Message Queue Thread 종료
		/// </summary>
		/// <exception cref="Exception"></exception>
		public void StopMsgQueueThread()
		{
			_isRecvMsgThreadRunning = false;
			_isSendMsgThreadRunning = false;

			if (_recvMsgQueueThread.IsAlive || _sendMsgQueueThread.IsAlive)
			{
				Stopwatch sw = Stopwatch.StartNew();

				while ((_recvMsgQueueThread.IsAlive || _sendMsgQueueThread.IsAlive) && sw.Elapsed.TotalSeconds <= 5)
				{
					Thread.Sleep(100);
				}

				if (_recvMsgQueueThread.IsAlive || _sendMsgQueueThread.IsAlive)
				{
					throw new Exception("MessageQueue : MessageQueueThread 종료 후 5초 경과하였으나 Thread가 살아있습니다.");
				}
			}

			_recvMsgQueueThread = null;
			_sendMsgQueueThread = null;
		}

		/// <summary>
		/// RecvMsgQueue 체크해 Queue에 요소 존재할 때 수행할 로직
		/// </summary>
		public void RecvMsgQueueMonitoring()
		{
			while (_isRecvMsgThreadRunning)
			{
				if (RecvMsgQueue.Count > 0)
				{
					RecvMsgQueue.TryDequeue(out RecvMessage recvMsg);

					try
					{
						// 클라이언트로부터 받은 요청 처리 로직
						if (_isServer)
						{
							Console.WriteLine(recvMsg.Message);
						}
						// 서버로부터 받은 요청 처리 로직
						else
						{
							Console.WriteLine(recvMsg.Message);
						}
					}
					catch
					{

					}
				}
				else
				{
					// CPU 부하 방지
					Thread.Sleep(100);
				}
			}
		}

		/// <summary>
		/// SendMsgQueue 체크해 Queue에 요소 존재할 때 수행할 로직
		/// </summary>
		public void SendMsgQueueMonitoring()
		{
			while (_isSendMsgThreadRunning)
			{
				if (SendMsgQueue.Count > 0)
				{
					SendMsgQueue.TryDequeue(out SendMessage sendMsg);

					try
					{
						// 서버 -> 클라이언트 메시지 전송
						if (_isServer)
						{
							SendToClient?.Invoke(sendMsg.Message, sendMsg.Client);
						}
						// 클라이언트 -> 서버 메시지 전송
						else
						{
							SendToServer?.Invoke(sendMsg.Message);
						}
					}
					catch
					{
						throw new Exception("MessageQueue : Send Data Fail");
					}
				}
				else
				{
					// CPU 부하 방지
					Thread.Sleep(100);
				}
			}
		}

		#endregion => Method
	}
}
