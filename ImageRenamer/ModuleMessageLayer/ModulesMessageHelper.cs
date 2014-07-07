using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTools.ModuleMessageLayer {
	public class ModulesMessageHelper
	{
		private static ModulesMessageHelper _instanse;

		static ModulesMessageHelper() {
			_instanse = new ModulesMessageHelper();
		}

		private ModulesMessageHelper() {
			
		}
		private event Action<object, object> WideMessageRecived;

		private Dictionary<string, Action<object, object>> _addressMessageRecivers =
			new Dictionary<string, Action<object, object>>();

		public static ModulesMessageHelper Messager {
			get {
				return _instanse;
			}
		}

		public void Subscribe(Action<object,object> handler, string mesageName = null) {
			if (string.IsNullOrWhiteSpace(mesageName)) {
				WideMessageRecived += handler;
			} else {
				if (_addressMessageRecivers.ContainsKey(mesageName)) {
					throw new ArgumentException("Already has subscriber.");
				}
				_addressMessageRecivers[mesageName] = handler;
			}
		}

		public void PostMessage(object sender, MessageEventArgs message) {
			switch (message.Type) {
				case MessageType.Address: {
					if (string.IsNullOrWhiteSpace(message.Message)) {
						throw new ArgumentException("Empty message.");
					}
					if (_addressMessageRecivers.ContainsKey(message.Message)) {
						_addressMessageRecivers[message.Message](sender, message.Parameter);
					}
					break;
				}
				case MessageType.Wide: {
					WideMessageRecived(sender, message.Parameter);
					break;
				}
			}
		}
	}

	public enum MessageType
	{
		Wide, Address
	}

	public class MessageEventArgs : EventArgs
	{
		public readonly String Message;
		public MessageType Type = MessageType.Wide;
		public object Parameter;
		public MessageEventArgs(string message, MessageType type = MessageType.Wide) {
			Message = message;
			Type = type;
		}
	}
}
