using UnityEngine;
using System.Collections;

public class PlatformController : MonoBehaviour {
    public string[] tags;
    public LayerMask layerMask;

    public float velocityAngleDiff = 89.0f;
    //public float normalAngleDiff = 90.0f;

    public enum Dir {
        Up,
        Down,
        Left,
        Right
    }

    public Dir dir;
    public float ofs = 0.01f;

    Vector3 mDir;

    bool CheckTags(GameObject go) {
        foreach(string tag in tags) {
            if(go.tag == tag)
                return true;
        }

        return false;
    }

    void SetDir() {
        switch(dir) {
            case Dir.Up:
                mDir = Vector3.up;
                break;
            case Dir.Down:
                mDir = -Vector3.up;
                break;
            case Dir.Left:
                mDir = -Vector3.right;
                break;
            case Dir.Right:
                mDir = Vector3.right;
                break;
        }
    }

    void Awake() {
        SetDir();
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void FixedUpdate() {
#if UNITY_EDITOR
        SetDir();
#endif

        Vector3 wDir = transform.rotation*mDir;

        RaycastHit[] hits = rigidbody.SweepTestAll(wDir, ofs);

        foreach(RaycastHit hit in hits) {
            GameObject go = hit.collider.gameObject;
            Rigidbody body = go.rigidbody;
            //Vector3 up = go.transform.up;

            if(((1 << go.layer) & layerMask) != 0 && CheckTags(go)) {// && Vector3.Angle(up, hit.normal) >= normalAngleDiff) {
                Vector3 vel = rigidbody.GetPointVelocity(hit.point);
                if(vel != Vector3.zero) {
                    if(velocityAngleDiff == 0 || body.velocity == Vector3.zero || Vector3.Angle(go.transform.up, vel) >= velocityAngleDiff) {
                        body.MovePosition(go.transform.position + vel * Time.fixedDeltaTime);
                        //body.velocity += vel;
                    }
                }
            }
        }
    }
}
