using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace MiniTri
{
    public class Direct3DForm : Form
    {
        public virtual void Render() { throw new NotImplementedException(); }


        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Direct3DForm
            // 
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.KeyPreview = true;
            this.Name = "Direct3DForm";
            this.ResumeLayout(false);

        }
    }
}
