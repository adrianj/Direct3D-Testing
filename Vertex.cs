using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SlimDX;
using System.Drawing;

namespace Direct3DLib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector3 Position;
        public Color4 Color;
        public Vertex(float x, float y, float z) : this(x, y, z, System.Drawing.Color.Black) { }
        public Vertex(Vector4 pos, Color col) : this(pos.X, pos.Y, pos.Z, col) { }
        public Vertex(Vector3 pos, Color col) : this(pos.X, pos.Y, pos.Z, col) { }
        public Vertex(float x, float y, float z, Color col)
        {
            Vector3 pp = new Vector3(x, y, z);
            Position = pp; Color = new Color4(col);
        }
    }
}
