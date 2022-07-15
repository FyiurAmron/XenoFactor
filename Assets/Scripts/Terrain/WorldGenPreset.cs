namespace Terrain {

using UnityEngine;
using Util;

[CreateAssetMenu( menuName = "App/TerrainGenPreset", order = 0 )]
public class WorldGenPreset : ScriptableObject {
    public Vector3Int size;

    public IntRange rockHeightRange;
    public IntRange rockPeakHeightRange;
    public IntRange rockPeakLengthRange;
    public int rockPeakCount;
    public int rockSmoothRadius;

    public IntRange soilHeightRange;
    public int soilSmoothRadius;
    public FloatRange initialSoilAbsorbedRange;

    public float treeChance;
    public float treeDeadChance;
    public float treeSizeFactor;

    public int plantHeightAbsoluteMax;
}

}
