using UnityEngine;

namespace Data {

using System.Collections.Generic;

[CreateAssetMenu(  menuName = "App/PrefabMap", order = 0 )]
public class PrefabMap : ScriptableObject {
    public List<PrefabEntry> prefabEntries;

    public GameObject get( string prefabName ) { // O(n) linear search, should be enough for small sets
        foreach ( PrefabEntry prefabEntry in prefabEntries ) {
            if ( prefabEntry.name == prefabName ) {
                return prefabEntry.prefab;
            }
        }

        return null;
    }
}

}
