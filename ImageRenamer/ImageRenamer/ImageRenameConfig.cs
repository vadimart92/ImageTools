using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LevDan.Exif;

namespace ImageTools.ImageRenamer {
	class ImageRenameUtils {
		public static List<ImageRenameConfig> GetImageRenameConfigFiles(string directory, bool recursive) {
			List<ImageRenameConfig> result = new List<ImageRenameConfig>();
			foreach (var file in Directory.EnumerateFiles(directory,"*.*",SearchOption.AllDirectories)) {
				var ef = new ExifTagCollection(file);
				result.Add(new ImageRenameConfig(ef, file));
			}
			return result;
		}
		
	}
	class ImageRenameConfig
	{
		private ExifTagCollection _fileInfo;
		private string _oldName;
		public string OldDisplayValue {
			get { return _oldName; }
		}

		public ImageRenameConfig(ExifTagCollection fileInfo, string fileName) {
			_fileInfo = fileInfo;
			_oldName = fileName;
		}
		public override string ToString() {
			return _oldName;
		}
	}
}
