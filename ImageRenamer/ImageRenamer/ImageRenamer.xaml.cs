using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using ImageTools.Properties;

namespace ImageTools.ImageRenamer {
	/// <summary>
	/// Логика взаимодействия для ImageRenamer.xaml
	/// </summary>
	public partial class ImageRenamer
	{
		public bool IncludeSubDirs {
			get {
				return Settings.Default.UseSubDirs;
			}
			set {
				Settings.Default.UseSubDirs = value;
				Settings.Default.Save();
			}
		}
		public bool SaveToSameFolder {
			get {
				return Settings.Default.SaveToSameFolder;
			}
			set {
				Settings.Default.SaveToSameFolder = value;
				Settings.Default.Save();
			}
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

		public string OutDir {
			get { return _outDir; }
			set {
				
			}
		}

		private ObservableCollection<ExifTagInfo> _exifTags = new ObservableCollection<ExifTagInfo>();
		private ObservableCollection<ImageRenameConfig> _images = new ObservableCollection<ImageRenameConfig>();
		private string _outDir = string.Empty;
		public ImageRenamer() {
			InitializeComponent();
			InitBindings();
		}

		private void InitBindings() {
			DataContext = this;
			_images = new ObservableCollection<ImageRenameConfig>();
			lv_files.DataContext = this.DataContext;
			lv_files.ItemsSource = _images;
			orderByComboBox.ItemsSource = _exifTags;
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
				InitDir = fbd.SelectedPath;
				var cont = selectFolder.Content;
				selectFolder.Content = "Loading...";
				var infos = new List<ExifTagInfo>();
				var config = await Task.Run(() => {
					var c = ImageRenameUtils.GetImageRenameConfigFiles(fbd.SelectedPath, IncludeSubDirs, fileExtensions);
					c.Sort();
					infos = ImageRenameUtils.GetExifTagInfos(c);
					return c;
				});
				selectFolder.Content = cont;
				_images.Clear();
				_exifTags.Clear();
				infos.ForEach(i => _exifTags.Add(i));
				config.ForEach(fc => _images.Add(fc));
			}
			
		}

		private void SelectDestFolder_Click(object sender, RoutedEventArgs e) {
			foreach (ImageRenameConfig image in _images) {
				image.NewFileName = DateTime.Now.ToLongTimeString();
			}
		}
	}
	public class BollValueInverter : IValueConverter {

		#region Члены IValueConverter

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return !(bool) value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			return !(bool)value;
		}

		#endregion
	}
}
