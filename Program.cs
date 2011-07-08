
using System;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using SlimDX.Windows;
using SlimDX.D3DCompiler;
using Device = SlimDX.Direct3D10.Device;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Direct3DControl
{
    static class Program
    {
        

        [STAThread]
        static void Main()
        {
            NewDirect3DControlCode();
            //OriginalDirect3DControlCode();
        }

        public static void NewDirect3DControlCode()
        {
            Direct3DForm form = new TestForm();
            form.Text = "SlimDX - Direct3DControl Direct3D 11 Sample";
            //Direct3DEngine engine = new Direct3DEngine(form);
            MessagePump.Run(form, form.Render);
        }

        public static void OriginalDirect3DControlCode()
        {
            /*
            var form = new RenderForm("SlimDX - Direct3DControl Direct3D 11 Sample");
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(form.ClientSize.Width, form.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            Device device;
            SwapChain swapChain;
            //Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.Debug, desc, out device, out swapChain);



            //Factory factory = swapChain.GetParent<Factory>();
            //factory.SetWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);

            //Texture2D backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            //var renderView = new RenderTargetView(device, backBuffer);
            var bytecode = ShaderBytecode.CompileFromFile("Direct3DControl.fx", "fx_5_0", ShaderFlags.None, EffectFlags.None);
            Effect effect = null;// = new Effect(device, bytecode);
            var technique = effect.GetTechniqueByIndex(0);
            var pass = technique.GetPassByIndex(0);
            var layout = new InputLayout(device, pass.Description.Signature, new[] {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0) 
            });

            // Modified this code to be more generic with Structs.
            
            var stream = new DataStream(3 * 32, true, true);
            stream.WriteRange(new[]{
                new Vector4(0.0f, 0.5f, 0.5f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4(0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                new Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f) 
            });

            stream.Position = 0;

            var vertices = new SlimDX.Direct3D10.Buffer(device, stream, new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = 3 * Marshal.SizeOf(typeof(Vertex)),
                Usage = ResourceUsage.Default
            });
            stream.Dispose();


            //device.ImmediateContext.OutputMerger.SetTargets(renderView);
            //device.ImmediateContext.Rasterizer.SetViewports(new Viewport(0, 0, form.ClientSize.Width, form.ClientSize.Height, 0.0f, 1.0f));



            MessagePump.Run(form, () =>
            {
                //device.ImmediateContext.ClearRenderTargetView(renderView, Color.White);

                //device.ImmediateContext.InputAssembler.InputLayout = layout;
                //device.ImmediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                //device.ImmediateContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, Marshal.SizeOf(typeof(Vertex)), 0));

                for (int i = 0; i < technique.Description.PassCount; ++i)
                {
                    //pass.Apply(device.ImmediateContext);
                   // device.ImmediateContext.Draw(3, 0);
                }

                //swapChain.Present(0, PresentFlags.None);
            });

            bytecode.Dispose();
            vertices.Dispose();
            layout.Dispose();
            effect.Dispose();
            //renderView.Dispose();
            //backBuffer.Dispose();
            device.Dispose();
            //swapChain.Dispose();
             */
        }
             
    }
}