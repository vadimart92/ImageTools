using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageTools.ImageRenamer {
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
}
