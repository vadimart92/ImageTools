using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using ImageTools.ModuleMessageLayer;
using LevDan.Exif;

namespace ImageTools.ImageRenamer {
	public class ImageRenameConfig : IComparable<ImageRenameConfig>,INotifyPropertyChanged {
		public string OldDisplayValue {
			get {
				return _fileNameOnly;
			}
		}
		public string CurrentSortingTagValue {
			get;
			set;
		}

		public string OldFileFullName {
			get { return _oldName; }
		}

		public ExifTagCollection ExifTags {
			get { return _fileInfo; }
		}

		public string NewFileName {
			get { return _newFilename; }
			set {
				if (!string.Equals(_newFilename,value,StringComparison.OrdinalIgnoreCase)) {
					_newFilename = value;
					if (PropertyChanged != null) {
						lock (this) {
							PropertyChanged(this, GetArgs());
						}
					}
				}
			}
		}

		public bool ExifInfoReadSuccess {
			get {
				return _exifInfoReadSuccess;
			}
			set {
				_exifInfoReadSuccess = value;
			}
		}

		private ExifTagCollection _fileInfo;
		private string _oldName;
		private bool _exifInfoReadSuccess = true;
		private string _fileNameOnly;
		private string _newFilename;
		public ImageRenameConfig(ExifTagCollection fileInfo, string fileName) {
			_fileInfo = fileInfo;
			_oldName = fileName;
			_fileNameOnly = new FileInfo(fileName).Name;
		}
		public override string ToString() {
			return _oldName;
		}

		#region Члены IComparable<ImageRenameConfig>

		public int CompareTo(ImageRenameConfig other) {
			Func<ImageRenameConfig, int> comparer = (config) => (config.ExifInfoReadSuccess ? 1 : 0);
			return comparer(this).CompareTo(comparer(other));
		}

		#endregion

		#region Члены INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;
		private PropertyChangedEventArgs GetArgs([CallerMemberName]string propertyName = null) {
			return new PropertyChangedEventArgs(propertyName ?? string.Empty);
		}

		#endregion
	}
}
