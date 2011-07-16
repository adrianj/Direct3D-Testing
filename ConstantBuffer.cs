using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D10;

namespace Direct3DLib
{
	public class ConstantBuffer
	{
		
		private Type type;
		private float floVal;
		private Matrix matVal;
		private Vector3 vecVal;
		private bool requiresUpdate = true;
		private object objVar;
		public Matrix AsMatrix
		{
			get { return (Matrix)matVal; }
			set
			{
				Matrix m = (Matrix)matVal;
				if (m != value)
				{
					matVal = value;
					requiresUpdate = true;
				}
			}
		}
		public float AsFloat
		{
			get { return (float)floVal; }
			set
			{
				float m = (float)floVal;
				if (m != value)
				{
					floVal = value;
					requiresUpdate = true;
				}
			}
		}
		public Vector3 AsVector3
		{
			get { return (Vector3)vecVal; }
			set
			{
				Vector3 m = (Vector3)vecVal;
				if (m != value)
				{
					vecVal = value;
					requiresUpdate = true;
				}
			}
		}
		public ConstantBuffer(string variableName, Effect effect, float initialValue)
		{
			type = typeof(float);
			EffectVariable var = effect.GetVariableByName(variableName); 
			floVal = (float)initialValue; 
			objVar = var.AsScalar();
			requiresUpdate = true;
		}
		public ConstantBuffer(string variableName, Effect effect, Matrix initialValue)
		{
			type = typeof(Matrix);
			EffectVariable var = effect.GetVariableByName(variableName);
			matVal = (Matrix)initialValue;
			objVar = var.AsMatrix();
			requiresUpdate = true;
		}
		public ConstantBuffer(string variableName, Effect effect, Vector3 initialValue)
		{
			type = typeof(Vector3);
			EffectVariable var = effect.GetVariableByName(variableName);
			vecVal = (Vector3)initialValue;
			objVar = var.AsVector();
			requiresUpdate = true;
		}
		/// <summary>
		/// Writes the variable to the Graphics card if it has changed (ie, requiresUpdate = true)
		/// </summary>
		/// <returns>True if the variable has been updated, implying an effect.Apply() is called for</returns>
		public bool Apply()
		{
			if (requiresUpdate)
			{
				if (objVar != null)
				{
					if (type == typeof(float))
						(objVar as EffectScalarVariable).Set(AsFloat);
					if (type == typeof(Matrix))
						(objVar as EffectMatrixVariable).SetMatrix(AsMatrix);
					if (type == typeof(Vector3))
						(objVar as EffectVectorVariable).Set(AsVector3);
					requiresUpdate = false;
					return true;
				}
			}
			return false;
		}
	}

	public class ConstantBufferHelper : IDisposable
	{
		private Effect effect;
		private EffectPass effectPass;
		//private EffectMatrixVariable worldVar;
		//private EffectMatrixVariable viewProjVar;
		//private EffectVectorVariable lightDirVar;
		//private EffectScalarVariable lightDirIntVar;
		//private EffectScalarVariable lightAmbIntVar;
		//private EffectMatrixVariable localRotVar;

		private ConstantBuffer lightAmbInt;
		private ConstantBuffer lightDirInt;
		private ConstantBuffer viewProj;
		private ConstantBuffer world;
		private ConstantBuffer lightDir;
		private ConstantBuffer localRot;
		private List<ConstantBuffer> allBuffers = new List<ConstantBuffer>();

		public Matrix World { get { return world.AsMatrix; } set { world.AsMatrix = value; } }
		public Matrix ViewProj { get { return viewProj.AsMatrix; } set { viewProj.AsMatrix = value; } }
		public Matrix LocalRotation { get { return localRot.AsMatrix; } set { localRot.AsMatrix = value; } }
		public Vector3 LightDirection { get { return lightDir.AsVector3; } set { lightDir.AsVector3 = value; } }
		public float LightDirectionalIntensity  { get { return lightDirInt.AsFloat; } set { lightDirInt.AsFloat = value; } }
		public float LightAmbientIntensity { get { return lightAmbInt.AsFloat; } set { lightAmbInt.AsFloat = value; } }

		public void Update(Device device)
		{
			// Get the shader effects.
			effect = Effect.FromString(device, Properties.Resources.RenderWithLighting, "fx_4_0");
			EffectTechnique effectTechnique = effect.GetTechniqueByIndex(0);
			effectPass = effectTechnique.GetPassByIndex(0);
			//ConstantBuffer cb = new ConstantBuffer("World", ConstantBuffer.SupportedTypes.Matrix, effect, Matrix.Identity);
			//worldVar = effect.GetVariableByName("World").AsMatrix();
			//viewProjVar = effect.GetVariableByName("ViewProj").AsMatrix();
			//localRotVar = effect.GetVariableByName("LocalRotation").AsMatrix();
			//lightDirVar = effect.GetVariableByName("LightDir").AsVector();

			world = new ConstantBuffer("World", effect, Matrix.Identity);
			allBuffers.Add(world);
			lightDir = new ConstantBuffer("LightDir", effect, new Vector3(1,1,1));
			allBuffers.Add(lightDir);
			localRot = new ConstantBuffer("LocalRotation", effect, Matrix.Identity);
			allBuffers.Add(localRot);
			viewProj = new ConstantBuffer("ViewProj", effect, Matrix.Identity);
			allBuffers.Add(viewProj);
			lightAmbInt = new ConstantBuffer("AmbientIntensity", effect, 0.3f);
			allBuffers.Add(lightAmbInt);
			lightDirInt = new ConstantBuffer("DirectionalIntensity", effect, 0.7f);
			allBuffers.Add(lightDirInt);
			//lightAmbIntVar = effect.GetVariableByName("AmbientIntensity").AsScalar();
			//lightDirIntVar = effect.GetVariableByName("DirectionalIntensity").AsScalar();
			
		}

		public void Apply()
		{
			bool doApply = false;
			foreach (ConstantBuffer cb in allBuffers)
				if (cb.Apply()) doApply = true;
			if (doApply)
				effectPass.Apply();
		}

		public void ApplyLocals()
		{
			//if (worldVar != null) worldVar.SetMatrix(World);
			//if (localRotVar != null) localRotVar.SetMatrix((LocalRotation));
			//effectPass.Apply();
		}

		public void ApplyGlobals()
		{
			//if(viewProjVar != null) viewProjVar.SetMatrix(ViewProj);
			//if (lightDirVar != null) lightDirVar.Set(LightDirection);
			//if (lightAmbIntVar != null) lightAmbIntVar.Set(LightAmbientIntensity);
			//if (lightDirIntVar != null) lightDirIntVar.Set(LightDirectionalIntensity);
			//effectPass.Apply();
		}

		public void Dispose()
		{
			if (effect != null) effect.Dispose();
		}
	}
}
