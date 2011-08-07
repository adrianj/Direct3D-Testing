using System;
using System.Drawing;
//using SlimDX;
//using SlimDX.Direct3D10;
//using SlimDX.DXGI;
//using SlimDX.Windows;
//using SlimDX.D3DCompiler;
//using Device = SlimDX.Direct3D10.Device;
//using System.Runtime.InteropServices;
using System.Windows.Forms;
using Direct3DLib;

namespace Direct3DLib_Test
{
    static class Program
    {
        

        [STAThread]
        static void Main()
        {
            NewDirect3DControlCode();
        }

        public static void NewDirect3DControlCode()
        {
            Direct3DForm form = new TestForm();
			Direct3DForm.Run(form);
			//Direct3DForm form = new BasicForm();
            //form.Text = "SlimDX - Direct3DLib";
            //MessagePump.Run(form, form.Render);
			//Console.WriteLine("Press a key to exit");
			//Console.ReadKey();
        }

    }
}