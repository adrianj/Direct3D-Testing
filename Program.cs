
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

namespace Direct3DLib
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
            form.Text = "SlimDX - Direct3DLib";
            MessagePump.Run(form, form.Render);
        }

    }
}