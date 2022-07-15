using System.Collections.Generic;
using UnityEngine;

namespace Util {

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
    public static T orIfNull<T>( this T t, T other ) where T : Object {
        return ( t != null ) ? t : other;
    }
    
    public static bool isRealNull( this Object o ) {
        return ReferenceEquals( o, null );
    }
    
    public static T orIfRealNull<T>( this T t, T other ) {
        return ReferenceEquals( t, null ) ? other : t;
    }
}

}
