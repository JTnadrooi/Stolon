using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;


namespace Stolon
{
	public static class DynamicTweening
	{
		/// <summary>
		/// ... (gets casted to float, so inaccuraties may exist. They are mostly just one or two off, usually insignificant.)
		/// </summary>
		/// <param name="value">The intial value.</param>
		/// <param name="target">The target value.</param>
		/// <param name="elapsedMilliseconds">Elapsed miliseconds since last frame.</param>
		/// <param name="strength">The strenghts of the function</param>
		/// <param name="smoothness">The smoothness of the function. Higher values may cause performance issues.</param>
		public static void PushDesired(ref int value, int target, int elapsedMilliseconds, float strength = 0.1f, int smoothness = 1)
		{
			float fval = value;
			PushDesired(ref fval, target, elapsedMilliseconds, strength, smoothness);
			value = (int)fval;
		}
		/// <summary>
		/// Pushes 
		/// </summary>
		/// <param name="value">The intial value.</param>
		/// <param name="target">The target value.</param>
		/// <param name="elapsedMilliseconds">Elapsed miliseconds since last frame.</param>
		/// <param name="strength">The strenghts of the function</param>
		/// <param name="smoothness">The smoothness of the function. Higher values may cause performance issues.</param>
		public static void PushDesired(ref float value, float target, int elapsedMilliseconds, float strength = 0.001f, int smoothness = 1)
		{
			//float fValue = (int)(object)value!; // speechless. (update; not relevant anymore but I though it was rather comical.)
			float init = value;
			float delta = target - value;
			value += MathF.Pow(delta, smoothness) * MathF.Pow(strength, smoothness);
			value = System.Math.Clamp(value, MathF.Min(target, init), MathF.Max(target, init));
		}
		/// <summary>
		/// Push a subunitary float (1 to 0) to 1 if <paramref name="push"/> is true, else, it pulls it back to 0 with said <paramref name="strength"/> and <paramref name="smoothness"/>
		/// if <paramref name="invert"/> is true, this whole operation gets inverted.
		/// </summary>
		/// <param name="value">The initial value.</param>
		/// <param name="push"></param>
		/// <param name="elapsedMilliseconds">Elapsed miliseconds since last frame.</param>
		/// <param name="strength">The strenght of the function.</param>
		/// <param name="smoothness">The smoothness of the function, higher values can cause performace issues.</param>
		/// <param name="invert">If the method function inverted.</param>
		public static void PushSubunitary(ref float value, bool push, int elapsedMilliseconds, float strength = 0.1f, int smoothness = 1, bool invert = false) // 0 to 1
		{
			strength *= 16.0f / (elapsedMilliseconds == 0 ? 16 : elapsedMilliseconds);

			float delta = (push ? 1 : 0) - value;
			value += MathF.Abs(System.MathF.Pow(delta, smoothness)) * strength * (push ? 1 : -1);
			value = System.Math.Clamp(value, 0f, 1f);

			value = invert ? 1f - value : value;
		}
	}
}
