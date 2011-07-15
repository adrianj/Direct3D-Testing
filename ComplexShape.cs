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
	public class ComplexShape : Shape
	{
		public enum SupportedFileType { Unknown, STL_Binary, STL_ASCII, DXF }

		/// <summary>
		/// Reads a ComplexShape structure from a File.
		/// This method attempts to deduce what the file format is from the file extension
		/// and header contents.
		/// </summary>
		/// <param name="filename">Path of the file to read</param>
		/// <returns></returns>
		public static Shape CreateFromFile(string filename)
		{
			
			SupportedFileType type = SupportedFileType.Unknown;
			// Figure out file type from file extension.
			if (Path.GetExtension(filename).Equals(".stl", StringComparison.InvariantCultureIgnoreCase))
			{
				// Need to read file to work out if it's ASCII or Binary, or still Unknown.
				using (StreamReader reader = new StreamReader(filename))
				{
					string line = reader.ReadLine();
					// Check for the heading "solid ", indicating an STL file.
					if (line.Substring(0, 6).Equals("solid "))
					{
						// Check next line. Should begin with "facet " indicating it is a text file.
						line = reader.ReadLine();
						if (line.Substring(0, 6).Equals("facet "))
							type = SupportedFileType.STL_ASCII;
						else
							type = SupportedFileType.STL_Binary;
					}
					else
					{
						type = SupportedFileType.Unknown;
					}
				}
			}
			else if (Path.GetExtension(filename).Equals(".dxf", StringComparison.InvariantCultureIgnoreCase))
			{

			}

			// Call the appropriate method to read the file.
			if (type == SupportedFileType.STL_ASCII)
				return CreateFromStlAsciiFile(filename);
			else
				return null;
		}

		/// <summary>
		/// Reads a ComplexShape structure from an STL file.
		/// </summary>
		/// <param name="filename">Path of the file to read</param>
		/// <returns></returns>
		private static Shape CreateFromStlAsciiFile(string filename)
		{
			Shape ret = new ComplexShape();
			System.Drawing.Color col = System.Drawing.Color.FromArgb(240, 240, 240);
			bool alreadyWarnedAboutNormal = false;
			int lineNum = 0;
			using (StreamReader reader = new StreamReader(filename))
			{
				string line = reader.ReadLine();
				lineNum++;
				string[] words = line.Split(new char[] { ' ' });
				ret.Name = words[1];
				List<Vector3> verts = new List<Vector3>();
				Vector3 norm = Vector3.Zero;
				// Loop through to end of file.
				while (!reader.EndOfStream)
				{
					line = reader.ReadLine();
					lineNum++;
					words = line.Split(new char[] { ' ','\t','\n','\r' },StringSplitOptions.RemoveEmptyEntries);
					if (words.Length < 1)
						continue;
					else if (words[0].Equals("endsolid"))
						break;
					else if (words[0].Equals("facet"))
					{
						if (words[1].Equals("normal"))
						{
							norm = populateVector(words[2], words[3], words[4]);
						}
					}
					else if (words[0].Equals("vertex"))
					{
						verts.Add(populateVector(words[1], words[2], words[3]));
					}
					else if (words[0].Equals("endloop"))
						continue;
					else if (words[0].Equals("endfacet"))
					{
						if(verts.Count < 3) continue;
						Vector3 calcNorm;
						bool normCheck = CheckNormal(norm, verts[0], verts[1], verts[2],out calcNorm);
						if (!normCheck && !alreadyWarnedAboutNormal)
						{
							MessageBox.Show(filename + " line: "+lineNum+"\nCalculated Normal is different to Normal in file.\n"+
								"\nin File:     "+ norm + 
								"\ncalculated: " + calcNorm +
								"\n\nV0: "+verts[0]+
								"\nV1: "+verts[1]+
								"\nV2: "+verts[2]+
								"\n\nPerhaps it has a different meaning?");
							alreadyWarnedAboutNormal = true;
						}
						ret.Vertices.Add(new Vertex(verts[0],col,norm));
						ret.Vertices.Add(new Vertex(verts[1], col, norm));
						ret.Vertices.Add(new Vertex(verts[2], col, norm));
						ret.Vertices.Add(new Vertex(verts[2], col, -norm));
						ret.Vertices.Add(new Vertex(verts[1], col, -norm));
						ret.Vertices.Add(new Vertex(verts[0], col, -norm));
						verts.Clear();
					}
					else
						continue;

				}

			}

			return ret;
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

		private static Vector3 populateVector(string w1, string w2, string w3)
		{
			float f;
			Vector3 ret;
			float.TryParse(w1, out f);
			ret.X = f;
			float.TryParse(w2, out f);
			ret.Y = f;
			float.TryParse(w3, out f);
			ret.Z = f;
			return ret;
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
