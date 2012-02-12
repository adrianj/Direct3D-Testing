A Project for testing C# Direct3D Programming using SlimDX.

Specifically Direct3D 10, Shader version 4.0.  (incompatible with Windows XP)

The active project is Direct3DExtensions and associated Test project.  The intent is to keep a logical seperation between the various parts of a 3D control. These are:
* Device and Swapchain (D3DDevice.cs) - for most projects this won't need much changing.
* Effect (Effect.cs) - this is strongly tied to the FX shader language files.
* Geometry (Geometry.cs) - defines the collection of Mesh objects that describe objects in the world.
* CameraInput (CameraInput.cs) - Handles the positioning of the camera and user input.

Each interface has a basic implementation that can be extended to add more application specific features, eg, new things added to the FX file(s), custom geometry, additional user input commands, etc.


ImageTiler is a project for building high resolution images from a collection of smaller tiles.  This is useful for creating detailed maps which can be applied as textures for example.


Below is information about Direct3DLib - which is now obsolete:

Includes a basic Direct3DEngine, where all shapes are defined as Vertices with Position, Normal and Colour.
This engine is used within a UserControl called Direct3DControl for integrating into simple windows apps, albeit with some special methods to run the for message pump.
A UserControl called Earth3DControl makes use of the Direct3DControl to provide a mapped terrain surface.
