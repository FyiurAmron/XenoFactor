namespace Util {

using System.Collections.Generic;
using UnityEngine;

public static class IListUnityExtensions {
    public static IList<T> shuffle<T>( this IList<T> list ) {
        return list.shuffle( maxValue => Random.Range( 0, maxValue ) );
    }
}

public static class Vector3IntExtensions {
    public static void setFrom( this ref Vector3Int v3i, Vector2Int v2i ) {
        v3i.x = v2i.x;
        v3i.y = v2i.y;
    }
}

public static class MaterialExtensions {
    public static void setKeyword( this Material material, string shaderKeyword, bool value ) {
        if ( value ) {
            material.EnableKeyword( shaderKeyword );
        } else {
            material.DisableKeyword( shaderKeyword );
        }
    }

    public static void toggleKeyword( this Material material, string shaderKeyword ) {
        bool nextState = !material.IsKeywordEnabled( shaderKeyword );
        if ( nextState ) {
            material.EnableKeyword( shaderKeyword );
        } else {
            material.DisableKeyword( shaderKeyword );
        }
    }

    public static void setColor( this Material material, string shaderKeyword, Color color ) {
        material.SetColor( shaderKeyword, color );
    }
}

public static class ObjectExtensions {
    public static T orIfNull<T>( this T t, T other ) where T : Object => ( t != null ) ? t : other;

    public static bool isRealNull( this Object o ) => ReferenceEquals( o, null );

    public static T orIfRealNull<T>( this T t, T other ) => ReferenceEquals( t, null ) ? other : t;
}

public enum EulerAxis {
    X,
    Y,
    Z,
}

public static class Vector3Extensions {
    public static Vector3 vector3ForEulerAxis( EulerAxis eulerAxis ) =>
        eulerAxis switch {
            EulerAxis.X => Vector3.right,
            EulerAxis.Y => Vector3.up,
            EulerAxis.Z => Vector3.forward,
        };

    public static Quaternion quaternionFromEulerAxis( EulerAxis eulerAxis, float value ) {
        return Quaternion.AngleAxis( value, vector3ForEulerAxis( eulerAxis ) );
    }
    
    public static Quaternion asEulerToQuaternion( this Vector3 vector3,
                                                  EulerAxis first, EulerAxis second, EulerAxis third ) {
        return
            quaternionFromEulerAxis( third, vector3.z ) *
            quaternionFromEulerAxis( second, vector3.y ) *
            quaternionFromEulerAxis( first, vector3.x );
    }
}

}
