using NetTcpManager.MessageQueue;
using NetTcpManager.Model;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetTcpManager.Client
{
	public class NetTcpClientManager
	{
		#region => Field

		private Socket _clientSocket;
		private Thread _recvDataThread;
		private bool _isConnected = false;

		#endregion => Field

		#region => Property

		public NetMessageQueueManager MessageQueue { get; set; }

		#endregion => Property

		#region => Constructor

		public NetTcpClientManager()
		{
			_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			MessageQueue = new NetMessageQueueManager(false);
			MessageQueue.StartMsgQueueThread();
			MessageQueue.SendToServer = SendData;
		}

		#endregion => Constructor

		#region => Method

		/// <summary>
		/// 서버 연결
		/// </summary>
		/// <param name="ipAddress"></param>
		/// <param name="port"></param>
		public void ConnectToServer(string ipAddress = "127.0.0.1", int port = 9999)
		{
			try
			{
				_clientSocket.Connect(new IPEndPoint(IPAddress.Parse(ipAddress), port));
				_isConnected = true;

				// 데이터 수신을 위한 스레드 시작
				_recvDataThread = new Thread(RecvDataMonitoring);
				_recvDataThread.IsBackground = true;
				_recvDataThread.Start();
			}
			catch (Exception ex)
			{
			}
		}

		/// <summary>
		/// 서버 연결 끊기
		/// </summary>
		public void CloseConnection()
		{
			try
			{
				MessageQueue.StopMsgQueueThread();
				_isConnected = false;
				_clientSocket.Shutdown(SocketShutdown.Both);
				_clientSocket.Close();
			}
			catch (Exception ex)
			{
			}
		}

		/// <summary>
		/// 서버로부터 응답 메시지 모니터링
		/// </summary>
		private void RecvDataMonitoring()
		{
			while (_isConnected)
			{
				try
				{
					byte[] data = new byte[1024];
					int dataSize = _clientSocket.Receive(data);

					if (dataSize > 0)
					{
						ProcessRecvData(data, dataSize);
					}
					else
					{
						Thread.Sleep(100);
					}
				}
				catch (Exception ex)
				{
					break;
				}
			}
		}

		/// <summary>
		/// 서버로부터 받은 데이터 처리 메서드
		/// </summary>
		/// <param name="data"></param>
		/// <param name="dataSize"></param>
		private void ProcessRecvData(byte[] data, int dataSize)
		{
			string recvData = Encoding.UTF8.GetString(data, 0, dataSize);

			// Recv Data 처리 로직

			RecvMessage recvMsg = new RecvMessage(recvData);
			MessageQueue.RecvMsgQueue.Enqueue(recvMsg);
		}

		/// <summary>
		/// 전송 데이터 포맷팅 처리 메서드
		/// </summary>
		/// <param name="message"></param>
		public void ProcessSendData(string message)
		{
			SendMessage sendMsg;

			// 데이터 포맷팅 처리 로직

			// SendMsgQueue 요소 추가
			sendMsg = new SendMessage(message);
			MessageQueue.SendMsgQueue.Enqueue(sendMsg);
		}

		/// <summary>
		/// 서버에 데이터 전송
		/// </summary>
		/// <param name="message"></param>
		public void SendData(string message)
		{
			if (_isConnected)
			{
				try
				{
					byte[] data = Encoding.UTF8.GetBytes(message);
					_clientSocket.Send(data);
				}
				catch (Exception ex)
				{
					throw new Exception("Client : Send Data Fail");
				}
			}
			else
			{
				return;
			}
		}

		#endregion => Method
	}
}
