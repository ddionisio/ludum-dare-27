using UnityEngine;
using System.Collections;

public class BeamController : MonoBehaviour {
    public Transform beamStartPoint;
    public float beamDirSign = 1.0f;
    public float beamLength = 100.0f;
    public BoxCollider beamCollision;

    public Transform[] beams;
    public Transform beamEnd;

    public LayerMask collisionMask;

    public float updateDelay = 0.1f;

    private bool mStarted;

    public void ActivateCollision() {
        beamCollision.enabled = true;
    }

    void OnEnable() {
        if(mStarted)
            StartCoroutine(DoUpdate());
    }

    void OnDisable() {
        StopAllCoroutines();

        beamCollision.enabled = false;
    }

    void Awake() {
        beamCollision.enabled = false;
    }

    // Use this for initialization
    void Start() {
        mStarted = true;

        StartCoroutine(DoUpdate());
    }

    IEnumerator DoUpdate() {
        WaitForSeconds wait = new WaitForSeconds(updateDelay);

        while(true) {
            Vector3 startPos = beamStartPoint.transform.position;
            Vector3 dir = beamStartPoint.rotation * (Vector3.right * beamDirSign);

            Vector3 endPos;
            float dist;

            RaycastHit hit;
            if(Physics.Raycast(startPos, dir, out hit, beamLength, collisionMask)) {
                dist = hit.distance;
                endPos = hit.point;
            }
            else {
                dist = beamLength;
                endPos = startPos + dir * dist;
            }

            beamEnd.transform.position = endPos;

            for(int i = 0; i < beams.Length; i++) {
                Transform t = beams[i];
                Vector3 s = t.localScale;
                s.x = beamDirSign*dist;
                t.localScale = s;
            }

            beamCollision.transform.position = startPos;
            beamCollision.transform.rotation = beamStartPoint.rotation;

            Vector3 collCenter = beamCollision.center;
            collCenter.x = beamDirSign * dist * 0.5f;

            Vector3 collSize = beamCollision.size;
            collSize.x = dist;

            beamCollision.center = collCenter;
            beamCollision.size = collSize;

            yield return wait;
        }
    }
}
