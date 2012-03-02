using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using SlimDX;
using System.Windows.Forms;

namespace Direct3DExtensions
{
	public class HGTFileReader
	{
		public const int Width = 1201;
		public const int Height = 1201;

		public static Mesh CreateFromStream(Stream stream)
		{
			Mesh mesh = new BasicMesh();
			mesh.Vertices = new Vertex[Width * Height];
			using (BinaryReader reader = new BinaryReader(stream))
			{
				for (int x = 0; x < Width; x++)
				{
					float xf = (float)x / ((float)Width - 1.0f);
					for (int y = 0; y < Height; y++)
					{
						ushort val = reader.ReadUInt16();
						float yf = (float)val;
						float zf = (float)y / ((float)Width - 1.0f);
						VertexTypes.PositionNormalTextured vert = new VertexTypes.PositionNormalTextured();
						vert.Pos = new Vector3(xf*10, yf/100000.0f, (1.0f-zf)*10);
						vert.TexCoord = new Vector2(xf, zf);
						vert.Normal = new Vector3(xf, yf/10.0f, zf);
						mesh.Vertices[y * Width + x] = vert;
					}
				}
			}
			mesh.Indices = new int[(Width-1)*(Height-1)*6];
			int i = 0;
			for (int x = 0; x < Width - 1; x++)
				for (int y = 0; y < Height - 1; y++)
				{
					int v0 = IndexFromCoord(x,y+1);
					int v1 = IndexFromCoord(x,y);
					int v2 = IndexFromCoord(x+1,y);
					int v3 = IndexFromCoord(x+1,y+1);
					mesh.Indices[i++] = v0;
					mesh.Indices[i++] = v1;
					mesh.Indices[i++] = v2;
					mesh.Indices[i++] = v0;
					mesh.Indices[i++] = v2;
					mesh.Indices[i++] = v3;
				}
			return mesh;
		}

		public static int IndexFromCoord(int x, int y)
		{
			return y * Width + x;
		}
	}
}
