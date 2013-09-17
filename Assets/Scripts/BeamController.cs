using UnityEngine;
using System.Collections;

public class BeamController : MonoBehaviour {
    public Transform beamStartPoint;
    public float beamDir = 1.0f;
    public float beamLength = 100.0f;
    public BoxCollider beamCollision;

    public Transform[] beams;
    public Transform beamEnd;

    public LayerMask collisionMask;

    public float updateDelay = 0.1f;

    private bool mStarted;

    void OnEnable() {
        if(mStarted)
            StartCoroutine(DoUpdate());
    }

    void OnDisable() {
        StopAllCoroutines();
    }

    // Use this for initialization
    void Start() {
        mStarted = true;

        StartCoroutine(DoUpdate());
    }

    IEnumerator DoUpdate() {
        WaitForSeconds wait = new WaitForSeconds(updateDelay);

        while(true) {
            yield return wait;
        }
    }
}
