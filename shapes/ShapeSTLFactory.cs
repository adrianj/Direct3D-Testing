using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

using SlimDX;

namespace Direct3DLib
{
	public class ShapeSTLFactory
	{
		public static Shape CreateFromStream(Stream stream)
		{
			bool isBinary = IsFileBinary(stream);
			if (isBinary)
				return CreateFromStlBinaryFile(stream);
			else
				return CreateFromStlAsciiFile(stream);
		}

		private static bool IsFileBinary(Stream stream)
		{
			long pos = stream.Position;
			try
			{
				// Need to read file to work out if it's ASCII or Binary, or still Unknown.
				StreamReader reader = new StreamReader(stream);
				string line = reader.ReadLine();
				// Check for the heading "solid ", indicating an STL file.
				if (line.Substring(0, 6).Equals("solid "))
				{
					// Check next line. Should begin with "facet " indicating it is a text file.
					line = reader.ReadLine();
					if (line.Substring(0, 6).Equals("facet "))
						return false;
					else
						return true;
				}
				else
					throw new ArgumentException("Unknown file type. STL files should begin with \"solid\"");
			}
			finally
			{
				stream.Seek(pos, SeekOrigin.Begin);
			}
		}

		private static Shape CreateFromStlBinaryFile(Stream stream)
		{
			throw new NotImplementedException("Reading binary STL Files not yet implemented");
		}

		/// <summary>
		/// Reads a ComplexShape structure from an STL file.
		/// </summary>
		/// <param name="filename">Path of the file to read</param>
		/// <returns></returns>
		private static Shape CreateFromStlAsciiFile(Stream stream)
		{
			Shape ret = new Shape();
			System.Drawing.Color col = System.Drawing.Color.FromArgb(240, 240, 240);
			bool alreadyWarnedAboutNormal = false;
			int lineNum = 0;
			StreamReader reader = new StreamReader(stream);

			string line = reader.ReadLine();
			lineNum++;
			string[] words = line.Split(new char[] { ' ' });
			//ret. = words[1];
			List<Vector3> verts = new List<Vector3>();
			Vector3 norm = Vector3.Zero;
			// Loop through to end of file.
			while (!reader.EndOfStream)
			{
				line = reader.ReadLine();
				lineNum++;
				words = line.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
				if (words.Length < 1)
					continue;
				else if (words[0].Equals("endsolid"))
					break;
				else if (words[0].Equals("facet"))
				{
					if (words[1].Equals("normal"))
					{
						norm = PopulateVector(words[2], words[3], words[4]);
					}
				}
				else if (words[0].Equals("vertex"))
				{
					verts.Add(PopulateVector(words[1], words[2], words[3]));
				}
				else if (words[0].Equals("endloop"))
					continue;
				else if (words[0].Equals("endfacet"))
				{
					if (verts.Count < 3) continue;
					Vector3 calcNorm;
					bool normCheck = ComplexShapeFactory.CheckNormal(norm, verts[0], verts[1], verts[2], out calcNorm);
					if (!normCheck && !alreadyWarnedAboutNormal)
					{
						MessageBox.Show(""+stream + " line: " + lineNum + "\nCalculated Normal is different to Normal in file.\n" +
							"\nin File:     " + norm +
							"\ncalculated: " + calcNorm +
							"\n\nV0: " + verts[0] +
							"\nV1: " + verts[1] +
							"\nV2: " + verts[2] +
							"\n\nPerhaps it has a different meaning?");
						alreadyWarnedAboutNormal = true;
					}
					ret.Vertices.Add(new Vertex(verts[0], col, norm));
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



			return ret;
		}


		public static Vector3 PopulateVector(string w1, string w2, string w3)
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

	}
}
