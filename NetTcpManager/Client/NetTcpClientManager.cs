using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
		#endregion => Property

		#region => Constructor

		public NetTcpClientManager()
		{
			_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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
				}
			}
			else
			{
			}
		}

		/// <summary>
		/// 서버로부터 응답 메시지 모니터링
		/// </summary>
		private void RecvDataMonitoring()
		{
			byte[] data = new byte[1024];

			while (_isConnected)
			{
				try
				{
					int dataSize = _clientSocket.Receive(data);

					if (dataSize > 0)
					{
						string recvData = Encoding.UTF8.GetString(data, 0, dataSize);
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
		/// 서버 연결 Close
		/// </summary>
		public void CloseConnection()
		{
			try
			{
				_isConnected = false;
				_clientSocket.Shutdown(SocketShutdown.Both);
				_clientSocket.Close();
			}
			catch (Exception ex)
			{
			}
		}

		#endregion => Method
	}
}
