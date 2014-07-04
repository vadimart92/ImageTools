using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using ImageTools.Properties;

namespace ImageTools.ImageRenamer {
	/// <summary>
	/// Логика взаимодействия для ImageRenamer.xaml
	/// </summary>
	public partial class ImageRenamer
	{
		public bool IncludeSubDirs {
			get;
			set;
		}
		public bool SaveToSameFolder {
			get;
			set;
		}
		
		public string InitDir {
			get {
				return Settings.Default.InDir;
			}
			set {
				Settings.Default.InDir = value;
				Settings.Default.Save();
			}
		}

		private ObservableCollection<ImageRenameConfig> _images; 
		public ImageRenamer() {
			InitializeComponent();
			InitBindings();
		}

		private void InitBindings() {
			DataContext = this;
			_images = new ObservableCollection<ImageRenameConfig>();
			lv_files.DataContext = this.DataContext;
			lv_files.ItemsSource = _images;
		}

		private async void selectFolder_Click(object sender, RoutedEventArgs e) {
			var fbd = new FolderBrowserDialog();
			if (string.IsNullOrEmpty(Settings.Default.InDir)) {
				fbd.RootFolder = Environment.SpecialFolder.DesktopDirectory;
			} else {
				fbd.SelectedPath = Settings.Default.InDir;
			}
			var fileExtensions = Settings.Default.ImgFilesExtensions.Split('|');
			if (fbd.ShowDialog() == DialogResult.OK) {
				this.
				InitDir = fbd.SelectedPath;
				var cont = selectFolder.Content;
				selectFolder.Content = "Loading...";
				_images.Clear();
				var config = await Task.Run(() => {
					var c = ImageRenameUtils.GetImageRenameConfigFiles(fbd.SelectedPath, IncludeSubDirs, fileExtensions);
					c.Sort();
					return c;
				});
				selectFolder.Content = cont;
				config.ForEach(fc => _images.Add(fc));
			}
			
		}

		private void SelectDestFolder_Click(object sender, RoutedEventArgs e) {
			foreach (ImageRenameConfig image in _images) {
				image.NewFileName = DateTime.Now.ToLongTimeString();
			}
		}
	}
}
