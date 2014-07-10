using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageTools.ModuleMessageLayer {
	public class ModulesMessageHelper
	{
		private static ModulesMessageHelper _instanse;
		private Dictionary<Action<object, object>, TaskScheduler> _syncContextStorage = new Dictionary<Action<object, object>, TaskScheduler>();
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

		public void Subscribe(Action<object,object> handler, string mesageName = null,TaskScheduler syncContext = null) {
			if (syncContext != null) {
				_syncContextStorage[handler] = syncContext;
			} 
			if (string.IsNullOrWhiteSpace(mesageName)) {
				WideMessageRecived += handler;
			} else {
				if (_addressMessageRecivers.ContainsKey(mesageName)) {
					_addressMessageRecivers[mesageName] += handler;
				} else {
					_addressMessageRecivers[mesageName] = handler;
				}
			}
		}

		public void PostMessage(object sender, MessageEventArgs message) {
			switch (message.Type) {
				case MessageType.Address: {
					if (string.IsNullOrWhiteSpace(message.Message)) {
						throw new ArgumentException("Empty message.");
					}
					Action<object,object> action = null;
					lock (_addressMessageRecivers){
						if (_addressMessageRecivers.ContainsKey(message.Message)) {
							action = _addressMessageRecivers[message.Message];
						}
					}
					if (action != null) {
						action(sender, message.Parameter);
					}
					break;
				}
				case MessageType.Wide: {
						Action<object,object> handlers = null;
						handlers = Interlocked.CompareExchange<Action<object, object>>(ref WideMessageRecived, null, null);
						if (handlers != null) {
							InvokeInSyncContext(handlers,sender, message.Parameter);
						}
					break;
				}
			}
		}
		private void InvokeInSyncContext(Action<object,object> action,object sender,object parameter, TaskScheduler context = null) {
			var scheduler = (_syncContextStorage.ContainsKey(action)) ? _syncContextStorage[action] : TaskScheduler.FromCurrentSynchronizationContext();
			var factory = new TaskFactory(context);
			factory.StartNew(()=>{
				action(sender, parameter);
			});
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
