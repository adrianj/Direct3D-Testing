using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D10;
using SlimDX;

namespace Direct3DLib
{
    public interface IRenderable
    {
        /// <summary>
        /// Draws the object to the screen.
        /// Typically this includes multiplying the object's World matrix
        /// with the provided cameraViewProj matrix and setting this in the
        /// device's "WorldViewProj" constant TransformVariable. Next are 
        /// multiple calls to device.Draw or Draw variants.
        /// </summary>
        /// <param name="device">The prepared 3D Graphics device</param>
        /// <param name="cameraViewProj">The current view*projection of 
        /// the camera.</param>
        void Render(Device device, ShaderHelper helper);

        /// <summary>
        /// Prepares the object in Graphics memory.
        /// Typically this includes adding the Vertices to the device's input buffer
        /// </summary>
        /// <param name="device"></param>
        void Update(Device device, Effect effect);


        /// <summary>
        /// An array of Vertices defining the structure of this object.
        /// </summary>
        VertexList Vertices { get; }


        /// <summary>
        /// This object's world matrix, not including any camera view or projection,
        /// ie, only the object's local location/rotation/scale.
        /// </summary>
        Matrix World { get; }

		/// <summary>
		/// The Index of the texture to apply to this shape.
		/// Set to -1 to use a simple diffuse color instead of a texture.
		/// </summary>
		int TextureIndex { get; }

        /// <summary>
        /// Is this object available for Pick selection.
        /// </summary>
        bool CanPick { get; }
        bool RayIntersects(Ray ray, out float dist);
    }
}
