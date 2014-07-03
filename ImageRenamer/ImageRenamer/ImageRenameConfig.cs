using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LevDan.Exif;

namespace ImageTools.ImageRenamer {
	class ImageRenameUtils {
		public static List<ImageRenameConfig> GetImageRenameConfigFiles(string directory, bool recursive, IEnumerable<String> fileExtensions) {
			var result = new List<ImageRenameConfig>();
			var searchOption = (recursive) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
			var files = from ext in fileExtensions
						from fileName in Directory.EnumerateFiles(directory, ext, searchOption)
						select fileName;
			foreach (var file in files) {
				try {
					var ef = new ExifTagCollection(file);
					result.Add(new ImageRenameConfig(ef, file));
				}
				catch (ArgumentException) {
					result.Add(new ImageRenameConfig(null, file) { ExifInfoReadSuccess = false});
				}
				
			}
			return result;
		}
		
	}
	class ImageRenameConfig : IComparable<ImageRenameConfig>
	{
		public string OldDisplayValue {
			get {
				return _oldName;
			}
		}
		public bool ExifInfoReadSuccess {
			get { return _exifInfoReadSuccess; }
			set { _exifInfoReadSuccess = value; }
		}

		private ExifTagCollection _fileInfo;
		private string _oldName;
		private bool _exifInfoReadSuccess = true;

		public ImageRenameConfig(ExifTagCollection fileInfo, string fileName) {
			_fileInfo = fileInfo;
			_oldName = fileName;
		}
		public override string ToString() {
			return _oldName +  ((ExifInfoReadSuccess)? string.Empty: ": exif read error") ;
		}

		#region Члены IComparable<ImageRenameConfig>

		public int CompareTo(ImageRenameConfig other) {
			Func<ImageRenameConfig, int> comparer = (config) => (config.ExifInfoReadSuccess ? 1 : 0);
			return comparer(this).CompareTo(comparer(other));
		}

		#endregion
	}
}
