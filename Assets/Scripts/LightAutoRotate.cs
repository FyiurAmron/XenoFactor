using UnityEngine;

public class LightAutoRotate : MonoBehaviour {
    private Vector3 rot;
    private float rotX;

    public float rotationSpeed;

    // Start is called before the first frame update
    protected void Start() {
        rot = transform.localEulerAngles;
        rotX = rot.x;
    }

    // every 0.5h of game time
    protected void FixedUpdate() {
        rotX += ( 360.0f / 24 ) / 2 * rotationSpeed;
        rot.x = rotX;
        transform.localEulerAngles = rot;
    }
}
