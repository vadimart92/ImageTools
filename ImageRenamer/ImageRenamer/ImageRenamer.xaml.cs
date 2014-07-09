using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using ImageTools.Properties;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Drawing;
using System.Windows.Media;

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
			if (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl) || fbd.ShowDialog() == DialogResult.OK) {
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

		private async void orderByComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			var exifProperty = e.AddedItems.OfType<ExifTagInfo>().FirstOrDefault();
			if (exifProperty == null) {
				return;
			}
			var tagNumber = exifProperty.TagNumber;
			var images = _images.ToList();
			await Task.Run(() => {
				Comparison<ImageRenameConfig> comparsion = (conf1, conf2) => {
					var tagInfo1 = conf1.ExifTags.SingleOrDefault(t => t.Id == tagNumber);
					var tagInfo2 = conf2.ExifTags.SingleOrDefault(t => t.Id == tagNumber);
					//TODO: realize
					try {
						return tagInfo1.Value.CompareTo(tagInfo2.Value);
					}
					catch (Exception) {
						return 0;
					}
				};
				images.Sort(comparsion);
				var format = "D" + images.Count.ToString().Length;
				for (int i = 0; i < images.Count; i++) {
					var img = images[i];
					var tagInfo = img.ExifTags.SingleOrDefault(t => t.Id == tagNumber);
					img.CurrentSortingTagValue = (tagInfo == null)? "<not set>" : tagInfo.Value;
					var fi = new FileInfo(img.OldFileFullName);
					img.NewFileName = "DSC_"+(i+1).ToString(format) + fi.Extension;
				}
			});
			_images.Clear();
			images.ForEach(i => _images.Add(i));
		}

		private async void lv_files_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			var selectedImage = e.AddedItems.OfType<ImageRenameConfig>().FirstOrDefault();
			if (selectedImage == null)
				return;
			if (File.Exists(selectedImage.OldFileFullName)) {
				var img = await Task.Run(() => {
					var bmp = ImageTools.Utils.GetBitmapImage(selectedImage.OldFileFullName, (i) => {
						return new Bitmap(i, new System.Drawing.Size(i.Width / 100 * 30, i.Height / 100 * 30));
					});
					bmp.Freeze();
					return bmp;
				});
				renameControl.Background = new ImageBrush(img);
			}
		}

		private async void export_Click(object sender, RoutedEventArgs e) {
			if (SaveToSameFolder) {
				if (_images.Any(i => !string.IsNullOrWhiteSpace(i.NewFileName))) {
					var dir = Path.Combine(Settings.Default.InDir, "export_" + DateTime.Now.ToString("yyyy-MM-dd"));
					if (!Directory.Exists(dir)) {
						Directory.CreateDirectory(dir);
					}
					await Utils.CopyFilesAsync(dir, _images);
				}
			}
		}
		
	}
	public class BoolValueInverter : IValueConverter {

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
