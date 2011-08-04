using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Direct3DLib
{
	/// <summary>
	/// 
	/// </summary>
    public class Direct3DForm : Form
    {
        public virtual void Render() { throw new NotImplementedException(
			"Make sure the Direct3DForm.Render() method calls the Render() methods of the Direct3DControl"
			); }

		public static void Run(Direct3DForm form)
		{
			SlimDX.Windows.MessagePump.Run(form, form.Render);
		}

        private void InitializeComponent()
        {
			/*
            this.SuspendLayout();
            // 
            // Direct3DForm
            // 
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.KeyPreview = true;
            this.Name = "Direct3DForm";
            this.ResumeLayout(false);
			 */
        }
    }
}
