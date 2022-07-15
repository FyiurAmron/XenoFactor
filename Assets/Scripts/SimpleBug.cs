using UnityEngine;

public class SimpleBug : MonoBehaviour {
    private int counter;
    private readonly int counterMax = 120;
    private Vector3 currPos;
    private Vector3 startPos;
    private readonly float step = 0.01f;
    private int walkStage;

    // Start is called before the first frame update
    protected void Start() {
        startPos = transform.position;
        currPos = startPos;
    }

    // Update is called once per frame
    protected void Update() {
        switch ( walkStage ) {
            case 0:
                currPos.x += step;
                break;
            case 1:
                currPos.z += step;
                break;
            case 2:
                currPos.x -= step;
                break;
            case 3:
                currPos.z -= step;
                break;
        }

        transform.position = currPos;

        if ( counter >= counterMax ) {
            counter = 0;
            if ( walkStage >= 4 ) {
                walkStage = 0;
            } else {
                walkStage++;
            }
        } else {
            counter++;
        }
    }
}
