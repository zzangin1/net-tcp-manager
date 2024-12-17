using System.Net.Sockets;

namespace NetTcpManager.Model
{
	/// <summary>
	/// SendMessage 구조
	/// </summary>
	public class SendMessage
	{
		#region => Field
		#endregion => Field

		#region => Property

		public string Message { get; set; }

		public Socket Client { get; set; }

		#endregion => Property

		#region => Constructor

		public SendMessage(string message, Socket client = null)
		{
			Message = message;
			Client = client;
		}

		#endregion => Constructor

		#region => Method
		#endregion => Method
	}
}
