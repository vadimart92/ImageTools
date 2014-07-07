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
	class ImageRenameUtils {
		public static List<ImageRenameConfig> GetImageRenameConfigFiles(string directory, bool recursive, IEnumerable<String> fileExtensions, object invoker = null) {
			var result = new List<ImageRenameConfig>();
			var searchOption = (recursive) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			var files = from ext in fileExtensions
						from fileName in Directory.EnumerateFiles(directory, ext, searchOption)
						select fileName;
			var count = files.Count();
			var i = 0;
			foreach (var file in files) {
				try {
					var ef = new ExifTagCollection(file);
					result.Add(new ImageRenameConfig(ef, file));
					i++;
					ModulesMessageHelper.Messager.PostMessage(invoker, new MessageEventArgs("hello"){Parameter = i*100/count});
					Thread.Sleep(2000);
				} catch (ArgumentException) {
					result.Add(new ImageRenameConfig(null, file) {
						ExifInfoReadSuccess = false
					});
				}

			}
			return result;
		}

		public static List<ExifTagInfo> GetExifTagInfos(IEnumerable<ImageRenameConfig> imagesConfig) {
			var allTags = (from config in imagesConfig
						  from tag in config.ExifTags
						  select new ExifTagInfo {Name = tag.FieldName, TagNumber = tag.Id});
			var result = new Dictionary<int,ExifTagInfo>();
			foreach (var t in allTags) {
				if (!result.ContainsKey(t.TagNumber)) {
					result.Add(t.TagNumber, t);
				}
			}
			return result.Values.ToList();
		}
	}
	public class ExifTagInfo : IComparable<ExifTagInfo>, IEquatable<ExifTagInfo> {
		public string Name {
			get;
			set;
		}
		public int TagNumber {
			get;
			set;
		}

		#region Члены IComparable<ExifTagInfo>

		public int CompareTo(ExifTagInfo other) {
			return TagNumber.CompareTo(other.TagNumber);
		}

		#endregion

		#region Члены IEquatable<ExifTagInfo>

		public bool Equals(ExifTagInfo other) {
			return TagNumber == other.TagNumber;
		}

		#endregion
	}
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
