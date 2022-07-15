namespace Util {

using System.Collections.Generic;
using UnityEngine;

public static class Direction3D {
    public static readonly IReadOnlyList<Vector3Int> PLANAR_DIRECTIONS = new List<Vector3Int>() {
        NORTH, WEST, SOUTH, EAST,
    };

    public static Vector3Int ABOVE => Vector3Int.forward; // yeah, Z<=>Y twiddle
    public static Vector3Int BELOW => Vector3Int.back;
    public static Vector3Int NORTH => Vector3Int.up;
    public static Vector3Int SOUTH => Vector3Int.down;
    public static Vector3Int EAST => Vector3Int.right;
    public static Vector3Int WEST => Vector3Int.left;

    public static IList<Vector3Int> getRandomOrderedPlanarDirections()
        => new List<Vector3Int>( PLANAR_DIRECTIONS ).shuffle();
}

}
