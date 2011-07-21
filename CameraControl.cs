using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using SlimDX;
using System.Drawing;

namespace Direct3DLib
{
    public class CameraControl : Object3D
    {
        private float mZClipNear = 0.1f;
        public float ZClipNear { get { return mZClipNear; } set { mZClipNear = value; updateWorld(); } }
        
        private float mZClipFar = 100;
        public float ZClipFar { get { return mZClipFar; } set { mZClipFar = value; updateWorld(); } }

        public float Pan { get { return mRotation.Y; } set { mRotation.Y = UnwrapPhase(value); updateWorld(); } }

        public Matrix View { get; set; }
        public Matrix Proj { get; set; }

        private const float MAX_TILT = (float)Math.PI - 0.001f;
        public float Tilt
        {
            get { return mRotation.X; }
            set
            {
                if (value > MAX_TILT) value = MAX_TILT;
                else if (value < -MAX_TILT) value = -MAX_TILT;
                mRotation.X = value;
                updateWorld();
            }
        }
        
        private const float MIN_ZOOM = 0.01f;
        public float Zoom
        {
            get { return mScale.Z; }
            set
            {
                if (value < MIN_ZOOM) mScale.Z = MIN_ZOOM;
                else mScale.Z = value;
                updateWorld();
            }
        }

        public void Initialize()
        {
            Pan = (float)Math.PI / 4;
            Tilt = -(float)Math.PI/2;
            Zoom = 5.0f;
            mLocation = new Vector3(0, 0, 0);
            updateWorld();
        }

        public void Translate(float x, float y, float z)
        {
            float zz = (float)Math.Cos(Pan) * z + (float)Math.Sin(Pan) * -x;
            float yy = y;
            float xx = (float)Math.Cos(Pan) * x - (float)Math.Sin(Pan) * -z;
            Vector3 newLoc = new Vector3(zz, y, xx);
            mLocation += newLoc;
            updateWorld();
        }

        private Control mParent;
        public CameraControl(Control con) : base()
        {
            mParent = con;
            this.Name = "Camera";
            Initialize();
        }

        
        public event EventHandler CameraChanged;
        private void FireCameraChangedEvent()
        {
            if (CameraChanged != null) CameraChanged(this, new EventArgs());
        }

        /// <summary>
        /// The updateWorld method is different to other Shapes, since I wish to Translate first
        /// and then Rotate. Also Scale is meaningless.
        /// </summary>
        protected override void updateWorld()
        {
            Matrix m = Matrix.Identity;
            float y = -(float)Math.Sin(Tilt / 2);
            float x = -(float)Math.Cos(Pan) * (float)Math.Cos(Tilt / 2);
            float z = -(float)Math.Sin(Pan) * (float)Math.Cos(Tilt / 2);
            Vector3 eye = new Vector3(x * Zoom, y * Zoom, z * Zoom) + Location;
            View = Matrix.LookAtLH(eye, Location, new Vector3(0, 1, 0));

            Proj = Matrix.PerspectiveFovLH(
                (float)Math.PI * 0.5f,
                (float)mParent.Width / (float)mParent.Height,
                ZClipNear, ZClipFar);
            m = m * View;
            m = m * Proj;
            if (m != World)
            {
                mWorld = m;
                FireCameraChangedEvent();
            }
        }

        
    }
}
