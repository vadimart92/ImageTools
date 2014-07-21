using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using ImageTools.Properties;

namespace ImageTools.ImageRenamer {
	class ImageRenamerVM : INotifyPropertyChanged {
		private ObservableCollection<ExifTagInfo> _exifTags = new ObservableCollection<ExifTagInfo>();
		private ObservableCollection<ImageRenameConfig> _images = new ObservableCollection<ImageRenameConfig>();
		private string _outDir = string.Empty;
		private ImageRenamer _view;
		public ImageRenamerVM(ImageRenamer view) {
			_view = view;
		}
		public ObservableCollection<ExifTagInfo> ExifTags {
			get {
				return _exifTags;
			}
		}
		public ObservableCollection<ImageRenameConfig> Images {
			get {
				return _images;
			}
		}
		public bool IncludeSubDirs {
			get {
				return Settings.Default.UseSubDirs;
			}
			set {
				if (Settings.Default.UseSubDirs != value) {
					Settings.Default.UseSubDirs = value;
					Settings.Default.Save();
					Utils.NotifyPropertyChanged(this, ref PropertyChanged);
				}
			}
		}
		public bool SaveToSameFolder {
			get {
				return Settings.Default.SaveToSameFolder;
			}
			set {
				if (Settings.Default.SaveToSameFolder != value) {
					Settings.Default.SaveToSameFolder = value;
					Settings.Default.Save();
					Utils.NotifyPropertyChanged(this, ref PropertyChanged);
				}
				
			}
		}
		public string InitDir {
			get {
				return Settings.Default.InDir;
			}
			set {
				if (Settings.Default.InDir != value){
					Settings.Default.InDir = value;
					Settings.Default.Save();
					Utils.NotifyPropertyChanged(this, ref PropertyChanged);
				}
			}
		}
		public string OutDir {
			get {
				return _outDir;
			}
			set {
				if (_outDir != value) {
					_outDir = value;
					Utils.NotifyPropertyChanged(this, ref PropertyChanged);
				}
			}
		}
		public void SelectDestFolder(){
			foreach (ImageRenameConfig image in _images) {
				image.NewFileName = DateTime.Now.ToLongTimeString();
			}
		}
		public async Task SelectFolder() {
			var fbd = new FolderBrowserDialog();
			if (string.IsNullOrEmpty(Settings.Default.InDir)) {
				fbd.RootFolder = Environment.SpecialFolder.DesktopDirectory;
			} else {
				fbd.SelectedPath = Settings.Default.InDir;
			}
			var fileExtensions = Settings.Default.ImgFilesExtensions.Split('|');
			if (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl) || fbd.ShowDialog() == DialogResult.OK) {
				InitDir = fbd.SelectedPath;
				var path = fbd.SelectedPath;
				if (!Directory.Exists(path)) {
					return;
				}
				var infos = new List<ExifTagInfo>();
				var config = await Task.Run(() => {
					var c = ImageRenameUtils.GetImageRenameConfigFiles(path, IncludeSubDirs, fileExtensions);
					c.Sort();
					infos = ImageRenameUtils.GetExifTagInfos(c);
					return c;
				});
				
				_images.Clear();
				_exifTags.Clear();
				infos.ForEach(i => _exifTags.Add(i));
				config.ForEach(fc => _images.Add(fc));
			}
		}
		public async void orderByComboBox_SelectionChanged(ExifTagInfo exifProperty) {
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
					} catch (Exception) {
						return 0;
					}
				};
				images.Sort(comparsion);
				var format = "D" + images.Count.ToString().Length;
				for (int i = 0; i < images.Count; i++) {
					var img = images[i];
					var tagInfo = img.ExifTags.SingleOrDefault(t => t.Id == tagNumber);
					img.CurrentSortingTagValue = (tagInfo == null) ? "<not set>" : tagInfo.Value;
					var fi = new FileInfo(img.OldFileFullName);
					img.NewFileName = "DSC_" + (i + 1).ToString(format) + fi.Extension;
				}
			});
			_images.Clear();
			images.ForEach(i => _images.Add(i));
		}
		public async void lv_files_SelectionChanged(ImageRenameConfig selectedImage) {
			
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
				_view.Background = new ImageBrush(img);
			}
		}
		public async void Export() {
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
		#region Члены INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
