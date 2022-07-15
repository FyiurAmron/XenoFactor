namespace Util {

using System;
using UnityEngine;

public class LinearAlgebra {
    public static int calcTotalDiamondWeight( int centerVal ) {
        int sum = centerVal;
        if ( centerVal > 1 ) {
            foreach ( int x in 1..( ( centerVal + 1 ) / 2 ) ) {
                sum += 8 * x * ( centerVal - x );
            }
        }

        if ( centerVal % 2 == 0 ) {
            sum += centerVal * centerVal;
        }

        return sum;
    }

    public static float[,] generateDiamondSmoothingKernel( int radius ) {
        int kernelSize = 2 * radius + 1;
        float[,] kernel = new float[kernelSize, kernelSize];
        int centerVal = radius + 1;
        float totalWeight = calcTotalDiamondWeight( centerVal );

        foreach ( int x in ..kernelSize ) {
            foreach ( int y in ..kernelSize ) {
                int val = centerVal - Math.Abs( x - radius ) - Math.Abs( y - radius );
                kernel[x, y] = ( val <= 0 )
                    ? 0
                    : val / totalWeight;
            }
        }

        return kernel;
    }

    // should be equal to generateDiamondSmoothingKernel(0)
    public static float[,] generateIdentityKernel() {
        return new float[,] { { 1 } };
    }

    public static int[,] convolve( Vector2Int size, int[,] src, float[,] kernel ) {
        int kLenX = kernel.GetLength( 0 ),
            kLenZ = kernel.GetLength( 1 );
        int[,] ret = new int[size.x, size.y];
        foreach ( int bx in ..size.x ) {
            foreach ( int by in ..size.y ) {
                float sum = 0;
                foreach ( int kx in ..kLenX ) {
                    foreach ( int ky in ..kLenZ ) {
                        sum += kernel[kx, ky] * src[bx + kx, by + ky];
                    }
                }

                ret[bx, by] = Mathf.RoundToInt( sum );
            }
        }

        return ret;
    }
}

}
