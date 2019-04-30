using UnityEngine;


/// <summary>
/// Provides fractional brownian motion functions for 1D and 3D spaces.
/// Provides functions to map the output of the noise functions (0.0 - 1.0) to another range of values (e.g. 40.0 - 80.0).
/// </summary>
public class Utils
{
	static readonly int maxHeight = 150;
	static readonly float smooth = 0.01f;
	static readonly int octaves = 4;
	static readonly float persistence = 0.5f;

	public static int GenerateStoneHeight(float x, float z)
	{
		float height = Map(0,maxHeight-5, 0, 1, fBM(x * smooth * 2, z * smooth * 2, octaves + 1, persistence));
		return (int) height;
	}

	public static int GenerateHeight(float x, float z)
	{
		float height = Map(0,maxHeight, 0, 1, fBM(x * smooth, z * smooth, octaves, persistence));
		return (int) height;
	}

    /// <summary>
    /// 3D Fractional Brownian Motion
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="sm"></param>
    /// <param name="oct"></param>
    /// <returns></returns>
    public static float fBM3D(float x, float y, float z, float sm, int oct)
    {
        float XY = fBM(x * sm, y* sm, oct, 0.5f);
        float YZ = fBM(y * sm, z* sm, oct, 0.5f);
        float XZ = fBM(x * sm, z* sm, oct, 0.5f);

        float YX = fBM(y * sm, x * sm, oct, 0.5f);
        float ZY = fBM(z * sm, y * sm, oct, 0.5f);
        float ZX = fBM(z * sm, x * sm, oct, 0.5f);

        return (XY+YZ+XZ+YX+ZY+ZX)/6.0f;
    }

	static float Map(float newMin, float newMax, float originMin, float originMax, float value)
    {
        return Mathf.Lerp (newMin, newMax, Mathf.InverseLerp (originMin, originMax, value));
    }

    /// <summary>
    /// 1D Fractional Brownian Motion based on Perline Noise.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <param name="oct"></param>
    /// <param name="pers"></param>
    /// <returns></returns>
    static float fBM(float x, float z, int oct, float pers)
    {
        float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;
        float offset = 32000f;
        for(int i = 0; i < oct ; i++) 
        {
                total += Mathf.PerlinNoise((x + offset) * frequency, (z + offset) * frequency) * amplitude;

                maxValue += amplitude;

                amplitude *= pers;
                frequency *= 2;
        }

        return total/maxValue;
    }
}
