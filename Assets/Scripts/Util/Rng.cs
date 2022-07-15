namespace Util {

using System;
using Random = UnityEngine.Random;

public class Rng {
    public static float normalValue( float mu = 0, float sigma = 1 ) {
        float n = MathF.Sqrt( -2 * MathF.Log( Random.value ) )
            * MathF.Cos( 2 * MathF.PI * Random.value );

        return mu + sigma * n;
    }
}

}
