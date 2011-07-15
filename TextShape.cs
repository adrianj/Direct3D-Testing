using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D10;
using SlimDX;
using SlimDX.Direct2D;

namespace Direct3DLib
{
    public class TextShape : Object3D
    {
        void Render(Device device, Matrix cameraViewProj)
        {
            
        }

        /// <summary>
        /// Prepares the object in Graphics memory.
        /// Typically this includes adding the Vertices to the device's input buffer
        /// </summary>
        /// <param name="device"></param>
        void Update(Device device)
        {
            /*
            System.Drawing.Font systemfont = new System.Drawing.Font("Arial", 12f, System.Drawing.FontStyle.Regular);
            FontDescription desc = new FontDescription();
            desc.
            text = new Font(device, systemfont);
             */
        }


        /// <summary>
        /// An array of Vertices defining the structure of this object.
        /// </summary>
        VertexList Vertices { get { return null; } }


        /// <summary>
        /// Is this object available for Pick selection.
        /// </summary>
        bool CanPick { get { return false; } }
        bool RayIntersects(Ray ray, out float dist)
        {
            dist = float.MaxValue;
            return false;
        }
    }
}
