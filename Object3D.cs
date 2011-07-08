using System;
using System.Collections.Generic;
using System.Text;
using SlimDX;

namespace MiniTri
{
    /// <summary>
    /// This class encompasses any kind of object that has a 3D position, scale and a 3D rotation.
    /// So it includes Shapes, Light sources and the Camera.
    /// </summary>
    public class Object3D : NamedObject
    {
        protected Matrix mWorld = Matrix.Identity;
        /// <summary>
        /// The final World matrix after all transformations have been applied.
        /// </summary>
        public Matrix World { get { return mWorld; } set { mWorld = value; } }

        private bool mThirdPerson = false;
        /// <summary>
        /// When IsThirdPerson, camera rotates about the point specified by Location looking toward it.
        /// When not IsThirdPerson (FirstPerson) camera is looking out from the point specified by Location.
        /// </summary>
        public bool IsThirdPerson { get { return mThirdPerson; } set { mThirdPerson = value; updateWorld(IsThirdPerson); } }

        protected Vector3 mScale = new Vector3(1, 1, 1);
        /// <summary>
        /// Specifies scaling of the object.
        /// </summary>
        public Vector3 Scale
        {
            get { return mScale; }
            set { mScale = value; updateWorld(IsThirdPerson); }
        }
        protected Vector3 mLocation = new Vector3(0,0,0);
        /// <summary>
        /// Specifies movement of the object's centre.
        /// </summary>
        public Vector3 Location
        {
            get { return mLocation; }
            set { mLocation = value; updateWorld(IsThirdPerson); }
        }
        protected Vector3 mRotation = new Vector3(0, 0, 0);
        /// <summary>
        /// Specifies a rotation about the object centre.
        /// This is applied as a RotateYawPitchRoll, X refers to Pitch, Y refers to Yaw,
        /// Z refers to Roll.
        /// </summary>
        public Vector3 Rotation
        {
            get { return mRotation; }
            set {
                mRotation = new Vector3(UnwrapPhase(value.X), UnwrapPhase(value.Y), UnwrapPhase(value.Z)); updateWorld(IsThirdPerson);
            }
        }

        public float UnwrapPhase(float phase)
        {
            float mod = (float)Math.PI*2;
            float p = phase % mod;
            if (p < 0) p += mod;
            return p;
        }

        /// <summary>
        /// Default implementation applies the transformations in the order
        /// specified by translateFirst.
        /// </summary>
        /// <param name="translateFirst">false (default when no param supplied) will Rotate->Translate->Scale.
        /// true will Translate->Rotate->Scale.</param>
        protected virtual void updateWorld(bool translateFirst)
        {
            // Start with identity matrix and apply the various transformations.
            Matrix m = Matrix.Identity;
            if (translateFirst)
            {
                m = m * Matrix.Scaling(mScale);
                m = m * Matrix.Translation(mLocation);
                m = m * Matrix.RotationYawPitchRoll(mRotation.Y, mRotation.X, mRotation.Z);
            }
            else
            {
                m = m * Matrix.Scaling(mScale);
                m = m * Matrix.RotationYawPitchRoll(mRotation.Y, mRotation.X, mRotation.Z);
                m = m * Matrix.Translation(mLocation);
            }
            mWorld = m;
        }
        /// <summary>
        /// Default implementation applies the transformations in the order:
        /// Rotation -> Translation -> Scaling
        /// </summary>
        protected virtual void updateWorld() { updateWorld(false); }
    }
}
