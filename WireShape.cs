using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SlimDX.Direct3D10;
using SlimDX;
using SlimDX.DXGI;

namespace Direct3DLib
{
    /// <summary>
    /// Inheriting from Shape, this class displays shapes as set of Lines
    /// as opposed to filled triangles.
    /// </summary>
    public class MeshShape : Shape
    {
        /// <summary>
        /// Renders to the screen as a mesh of Lines.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cameraViewProj"></param>
        public override void Render(SlimDX.Direct3D10.Device context, SlimDX.Matrix cameraViewProj)
        {
            context.InputAssembler.SetInputLayout(vertexLayout);
            context.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.LineStrip);
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Marshal.SizeOf(typeof(Vertex)), 0));
            if (this.Indices != null)
                context.InputAssembler.SetIndexBuffer(indexBuffer, Format.R16_UInt, 0);
            Matrix wvp = World * cameraViewProj;
            transformVariable.SetMatrix(wvp);
            effectPass.Apply();

            if (this.Indices != null && this.Indices.Length > 0)
                context.DrawIndexed(this.NumElements, 0, 0);
            else
                context.Draw(this.NumElements, 0);
        }
    }
}
