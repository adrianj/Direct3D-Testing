using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using System.Drawing;
using System.ComponentModel;

namespace Direct3DLib
{
    public interface IVertex
    {
        InputElement[] GetInputElements();
        Vector3 Position { get; set; }
    }

    [StructLayout(LayoutKind.Sequential),TypeConverterAttribute(typeof(BasicTypeConverter))]
    public struct Vertex : IVertex
    {
        private Vector4 mPosition;
        private Vector4 mNormal;
        private Color4 mColor;
		public float Alpha { get { return mColor.Alpha; } }
		public Vertex(Vector3 pos) : this(pos, new Vector3(0, 1, 0)) { }
        public Vertex(float x, float y, float z) : this(x, y, z, FloatToColor(new Vector3(x,y,z))) { }
        public Vertex(float x, float y, float z, Color col) : this(x, y, z, FloatToColor(new Vector3(x, y, z)),new Vector4(0,1,0,1)) { }
		public Vertex(float x, float y, float z, float nx, float ny, float nz)
			: this(x, y, z, FloatToColor(x, y, z), nx, ny, nz) { }
		public Vertex(float x, float y, float z, Color col, float nx, float ny, float nz) :
			this(new Vector4(x, y, z,1), new Color4(col), new Vector4(nx, ny, nz,1)) { }
        public Vertex(float x, float y, float z, Color col, Vector4 normal) :
            this(new Vector4(x, y, z,1), new Color4(FloatToColor(new Vector3(x, y, z))),normal) { }
        public Vertex(Vector4 pos, Color col) : this(pos, col,pos) { }
		public Vertex(Vector3 pos, Vector3 normal) : this(pos, FloatToColor(pos), normal) { }
		public Vertex(Vector3 pos, Color col, Vector3 normal) : this(new Vector4(pos,1), col, new Vector4(normal, 1)) { }
        public Vertex(Vector4 pos, Color4 col, Vector4 normal)
        {
            mPosition = pos;
            mColor = col;
            mNormal = normal;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Vector3 Position { get { return new Vector3(mPosition.X,mPosition.Y,mPosition.Z); } set { mPosition = new Vector4(value.X,value.Y,value.Z,1); } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Color Color { get { return mColor.ToColor(); } set { mColor = new Color4(value); } }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Vector3 Normal { get { return new Vector3(mNormal.X, mNormal.Y, mNormal.Z); } set { mNormal = new Vector4(value.X, value.Y, value.Z, 1); } }
		public Vector2 TextureCoordinates { get { return new Vector2(mColor.Red, mColor.Green); } set { mColor.Red = value.X; mColor.Green = value.Y; } }
		public InputElement[] GetInputElements()
        {
            InputElement [] ret = new InputElement[]
            {
                new InputElement("POSITION",0,Format.R32G32B32_Float,0,0),
                new InputElement("NORMAL",0,Format.R32G32B32A32_Float,16,0),
                new InputElement("COLOR",0,Format.R32G32B32A32_Float,32,0)
            };
            return ret;
        }

		public static Color FloatToColor(Vector3 rgb)
		{
			rgb = Vector3.Normalize(rgb);
			return FloatToColor(rgb.X, rgb.Y, rgb.Z);
		}
        public static Color FloatToColor(float r, float g, float b)
        {
            int ri = (int)Math.Abs((r + 1) * 127.9);
            int gi = (int)Math.Abs((g + 1) * 127.9);
            int bi = (int)Math.Abs((b + 1) * 127.9);
            
            return Color.FromArgb(ri%256, gi%256, bi%256);
        }

		public override string ToString()
		{
			return "" + mPosition;
		}

    }


    public class VertexTypeConverter : TypeConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            return TypeDescriptor.GetProperties(typeof(Vertex));
        }
    }
}
