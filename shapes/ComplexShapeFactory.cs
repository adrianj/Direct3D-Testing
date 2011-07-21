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
		private delegate Shape CreateShapeFromFile(string filename);
		public enum SupportedFileType { Unknown, STL, DXF, HGT }
		private static Dictionary<CreateShapeFromFile, string> TypeDictionary = new Dictionary<CreateShapeFromFile, string>
		{
			{ComplexShapeFactory.CreateFromUnknownFile,"unknown"},
			{ComplexShapeFactory.CreateFromDxfFile,".dxf"},
			{ShapeHGTFactory.CreateFromFile,".hgt"},
			{ShapeSTLFactory.CreateFromFile,".stl"}
		};

		public static string SupportedFileTypeFilter
		{
			get
			{
				string fileNames = "3D Model Files";
				string fileExts = "|";
				bool first = true;
				foreach (string ext in TypeDictionary.Values)
				{
					if (ext.Substring(0, 1).Equals("."))
					{
						if (!first) fileExts += ";"; first = false;
						fileExts += "*" + ext;
					}
				}
				string allFiles = "|All Files|*.*";
				return fileNames + fileExts + allFiles;
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
			CreateShapeFromFile functionDelegate = DetermineFileType(filename);
			Shape retShape = functionDelegate(filename);
			return retShape;
		}

		private static CreateShapeFromFile DetermineFileType(string filename)
		{
			// Figure out file type from file extension.
			foreach (KeyValuePair<CreateShapeFromFile, string> keyPair in TypeDictionary)
			{
				if (IsGivenFileExtension(keyPair.Value, filename))
				{
					return keyPair.Key;
				}
			}
			return ComplexShapeFactory.CreateFromUnknownFile;
		}

		private static Shape CreateFromUnknownFile(string filename)
		{
			return null;
		}

		private static Shape CreateFromKnownFileType(SupportedFileType type, string filename)
		{
			if (type == SupportedFileType.Unknown) return null;
			if (type == SupportedFileType.STL) return ShapeSTLFactory.CreateFromFile(filename);
			if (type == SupportedFileType.DXF) return ShapeSTLFactory.CreateFromFile(filename);
			if (type == SupportedFileType.HGT) return ShapeSTLFactory.CreateFromFile(filename);
			return null;
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
