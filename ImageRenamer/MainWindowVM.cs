using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImageTools.ModuleMessageLayer;

namespace ImageTools {
	class MainWindowVM: INotifyPropertyChanged {
		readonly MainWindow _window;
		int _progress = -1;
		public MainWindowVM(MainWindow window) {
			_window = window;
			Init();
		}
		void Init() {
			SubscribeOnEvents();
		}
		private bool _isShowProgress;
		public bool IsProgressBarVisible {
			get {
				return _isShowProgress;
			}
			set {
				if (_isShowProgress != value) {
					_isShowProgress = value;
					Utils.NotifyPropertyChanged(this, ref PropertyChanged);
				}
			}
		}
		public int Progress {
			get{
				return _progress;
			}
			set {
				if (value != _progress) {
					_progress = value;
					Utils.NotifyPropertyChanged(this, ref PropertyChanged);
				}
			}
		}
		private void SubscribeOnEvents() {
			var UISyncContext = TaskScheduler.FromCurrentSynchronizationContext();
			ModulesMessageHelper.Messager.Subscribe((sndr, arg) => {
				Progress = (int)arg;
			}, GlobalConsts.UpdateProgressMsg, syncContext: UISyncContext);
			ModulesMessageHelper.Messager.Subscribe((sndr, arg) => {
				IsProgressBarVisible = (bool)arg; 
			}, GlobalConsts.SetProgressBarVisibilityMsg, syncContext: UISyncContext);
		}
		#region Члены INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
