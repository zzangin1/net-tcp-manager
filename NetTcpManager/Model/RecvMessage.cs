using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTcpManager.Model
{
	/// <summary>
	/// RecvMessage 구조
	/// </summary>
	public class RecvMessage
	{
		#region => Field
		#endregion => Field

		#region => Property

		public Guid MessageId { get; set; }

		public string Message { get; set; }

		#endregion => Property

		#region => Constructor

		public RecvMessage(string message)
		{
			MessageId = Guid.NewGuid();
			Message = message;
		}

		#endregion => Constructor

		#region => Method
		#endregion => Method
	}
}
