using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageTools {
	class MainWindowVM: INotifyPropertyChanged {
		int _progress;
		public int Progress {
			get{
				return _progress;
			}
			set {
				if (value != _progress) {
					_progress = value;
					NotifyPropertyChanged();
				}
			}
		}
		private void NotifyPropertyChanged([CallerMemberName]string propertyName = null) {
			PropertyChangedEventHandler handlers = null;
			handlers = Interlocked.CompareExchange<PropertyChangedEventHandler>(ref PropertyChanged, null, null);
			if (handlers != null) {
				handlers(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#region Члены INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
