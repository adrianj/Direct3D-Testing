using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SlimDX;
using System.Windows.Forms;

namespace Direct3DLib
{
	/// <summary>
	/// Class for creating more complex shapes.
	/// Typically these are created from STL files or some other similar file format.
	/// </summary>
	public class ComplexShapeFactory
	{
		public enum SupportedFileType { Unknown, STL, DXF, HGT }
		private static Dictionary<SupportedFileType,string> TypeDictionary = new Dictionary<SupportedFileType,string>
		{
			{SupportedFileType.Unknown,"unknown"},
			{SupportedFileType.DXF,".dxf"},
			{SupportedFileType.HGT,".hgt"},
			{SupportedFileType.STL,".stl"}
		};

		public static string SupportedFileTypeFilter
		{
			get
			{
				string ret = "All Files (*.*)|*.*";
				return ret;
			}
		}

		/// <summary>
		/// Reads a ComplexShape structure from a File.
		/// This method attempts to deduce what the file format is from the file extension
		/// and header contents.
		/// </summary>
		/// <param name="filename">Path of the file to read</param>
		/// <returns></returns>
		public static Shape CreateFromFile(string filename)
		{
			SupportedFileType type = DetermineFileType(filename);

			Shape retShape = null;
			// Call the appropriate method to read the file.
			if (type == SupportedFileType.STL)
				retShape = ShapeSTL.CreateFromFile(filename);
			
			return retShape;
		}

		private static SupportedFileType DetermineFileType(string filename)
		{
			SupportedFileType type = SupportedFileType.Unknown;
			// Figure out file type from file extension.
			if (IsGivenFileExtension(".stl", filename))
			{
				type = SupportedFileType.STL;
			}
			else if (IsGivenFileExtension(".dxf", filename))
			{
				type = SupportedFileType.DXF;
			}
			return type;
		}

		private static bool IsGivenFileExtension(string extension, string filename)
		{
			if (Path.GetExtension(filename).Equals(extension, StringComparison.InvariantCultureIgnoreCase))
				return true;
			return false;
		}

				/// <summary>
		/// Reads a ComplexShape structure from a DXF file.
		/// </summary>
		/// <param name="filename">Path of the file to read</param>
		/// <returns></returns>
		private static Shape CreateFromDxfFile(string filename)
		{
			return null;
		}


		public static bool CheckNormal(Vector3 norm, Vector3 v1, Vector3 v2, Vector3 v3)
		{
			Vector3 calcNorm;
			return CheckNormal(norm, v1, v2, v3, out calcNorm);
		}
		public static bool CheckNormal(Vector3 norm, Vector3 v1, Vector3 v2, Vector3 v3, out Vector3 calcNorm)
		{
			norm = Vector3.Normalize(norm);
			float tol = 0.2f;
			calcNorm = new Plane(v1, v2, v3).Normal;
			if (Math.Abs(calcNorm.X - norm.X) > tol || Math.Abs(calcNorm.Y - norm.Y) > tol
				|| Math.Abs(calcNorm.Z - norm.Z) > tol)
			{
				return false;
			}
			return true;
		}



	}
}
