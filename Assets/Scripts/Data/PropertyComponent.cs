namespace Data {

using UnityEngine;

// subclassing explicitly needed since generic MonoBehaviour is not allowed
// also, "properties" are not related to C# concept here; they are just abstract object property POD
public class PropertyComponent<T> : MonoBehaviour where T : new() {
    [SerializeReference]
    public T properties = new();
}

}
