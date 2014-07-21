using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			var j = 0;
			var prevJ = 0;
			ModulesMessageHelper.Messager.PostMessage(invoker,
				new MessageEventArgs(GlobalConsts.SetProgressBarVisibilityMsg, MessageType.Address) {
					Parameter = true
				});
			foreach (var file in files) {
				i++;
				j = i * 100 / count;
				if (j != prevJ || j == 0) {
					prevJ = j;
					ModulesMessageHelper.Messager.PostMessage(invoker,
						new MessageEventArgs(GlobalConsts.UpdateProgressMsg, MessageType.Address) {
							Parameter = j
						});
				}
				try {
					var ef = new ExifTagCollection(file);
					result.Add(new ImageRenameConfig(ef, file));
				} catch (ArgumentException) {
					result.Add(new ImageRenameConfig(null, file) {
						ExifInfoReadSuccess = false
					});
				} catch (NullReferenceException) {
					result.Add(new ImageRenameConfig(null, file) {
						ExifInfoReadSuccess = false
					});
				}

			}
			ModulesMessageHelper.Messager.PostMessage(invoker,
				new MessageEventArgs(GlobalConsts.SetProgressBarVisibilityMsg, MessageType.Address) {
					Parameter = false
				});
			return result;
		}
		public static List<ExifTagInfo> GetExifTagInfos(IEnumerable<ImageRenameConfig> imagesConfig) {
			var allTags = (from config in imagesConfig
						   where config.ExifTags != null
						   from tag in config.ExifTags
						   select new ExifTagInfo {
							   Name = tag.FieldName, TagNumber = tag.Id
						   });
			var result = new Dictionary<int, ExifTagInfo>();
			bool any;
			try {
				any = allTags.Any();
			} catch (NullReferenceException) {
				return result.Values.ToList();
			}
			if (!any) {
				return result.Values.ToList();
			}
			foreach (var t in allTags) {
				if (!result.ContainsKey(t.TagNumber)) {
					result.Add(t.TagNumber, t);
				}
			}
			return result.Values.ToList();
		}
	}
}
