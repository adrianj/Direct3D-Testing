using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SlimDX;
using D3D = SlimDX.Direct3D10;
using DXGI = SlimDX.DXGI;

namespace Direct3DExtensions
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public interface Vertex {
		Vector3 Pos { get; set; }
	}

	public class VertexTypes
	{
		//public interface IPosition : Vertex  {  }
		public interface INormal : Vertex  { Vector3 Normal { get; set; } }
		public interface ITextured : Vertex  { Vector2 TexCoord { get; set; } }

		
		public struct Position : Vertex
		{
			public Vector3 Pos { get; set; }
			public override string ToString() { return VertexToString(this); }
		}
		 

		public struct PositionNormal : INormal
		{
			public Vector3 Pos { get; set; }
			public Vector3 Normal { get; set; }
			public override string ToString() { return VertexToString(this); }
		}

		public struct PositionNormalTextured : INormal, ITextured
		{
			public Vector3 Pos { get; set; }
			public Vector3 Normal { get; set; }
			public Vector2 TexCoord { get; set; }
			public override string ToString() { return VertexToString(this); }
		}

		public struct PositionTexture : ITextured
		{
			public Vector3 Pos { get; set; }
			public Vector2 TexCoord { get; set; }
			public override string ToString() { return VertexToString(this); }
		}

		public static D3D.InputElement[] GetInputElements(Type t)
		{
			int offset = 0;
			List<D3D.InputElement> list = new List<D3D.InputElement>();
			if (t.GetInterface("Vertex") != null)
			{
				list.Add(new D3D.InputElement("POSITION", 0, DXGI.Format.R32G32B32_Float, offset, 0));
				offset += 12;
			}
			if (t.GetInterface("INormal") != null)
			{
				list.Add(new D3D.InputElement("NORMAL", 0, DXGI.Format.R32G32B32_Float, offset, 0));
				offset += 12;
			}
			if (t.GetInterface("ITextured") != null)
			{
				list.Add(new D3D.InputElement("TEXCOORD", 0, DXGI.Format.R32G32_Float, offset, 0));
				offset += 8;
			}
			return list.ToArray();
		}

		public static string VertexToString(Vertex v)
		{
			string ret = "V:";
				ret += " {P: " + v.Pos+"}";
			if (v.GetType().GetInterface("INormal") != null)
				ret += " {N: " + (v as INormal).Normal+"}";
			if (v.GetType().GetInterface("ITextured") != null)
				ret += " {T: " + (v as ITextured).TexCoord+"}";
			return ret;
		}

		public static byte[] GetBytes(Vertex v)
		{
			int rawsize = Marshal.SizeOf(v);
			IntPtr buffer = Marshal.AllocHGlobal(rawsize);
			Marshal.StructureToPtr(v, buffer, false);
			byte[] rawdatas = new byte[rawsize];
			Marshal.Copy(buffer, rawdatas, 0, rawsize);
			Marshal.FreeHGlobal(buffer);
			return rawdatas;
		}

		public static D3D.InputLayout GetInputLayout(D3D.Device device, D3D.EffectPass pass, Type vertexType)
		{
			D3D.InputElement[] inputElements = GetInputElements(vertexType);
			D3D.InputLayout layout = new D3D.InputLayout(device, pass.Description.Signature, inputElements);
			
			return layout;
		}

		public static int SizeOf(Type type)
		{
			return Marshal.SizeOf(type);
		}

		public static Vertex Cast(Vertex vertexToCast, Type newVertexType)
		{
			Vertex ret = Activator.CreateInstance(newVertexType) as Vertex;
			ret.Pos = vertexToCast.Pos;
			if (newVertexType.GetInterface("ITextured") != null && vertexToCast is ITextured)
				(ret as ITextured).TexCoord = (vertexToCast as ITextured).TexCoord;
			if (newVertexType.GetInterface("INormal") != null && vertexToCast is INormal)
				(ret as INormal).Normal = (vertexToCast as INormal).Normal;
			return ret;
		}
	}
}
