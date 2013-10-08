using UnityEngine;
using System.Collections;

public class TriggerLockPlatformerDrag : MonoBehaviour {
    public string[] checkTags = { "Player" };
    public float drag;

    bool MatchTag(GameObject go) {
        for(int i = 0, max = checkTags.Length; i < max; i++) {
            if(go.CompareTag(checkTags[i]))
                return true;
        }

        return false;
    }

    void OnTriggerEnter(Collider col) {
        if(MatchTag(col.gameObject)) {
            PlatformerController ctrl = col.GetComponent<PlatformerController>();
            ctrl.lockDrag = true;
            col.rigidbody.drag = drag;
        }
    }

    void OnTriggerExit(Collider col) {
        if(MatchTag(col.gameObject)) {
            PlatformerController ctrl = col.GetComponent<PlatformerController>();
            ctrl.lockDrag = false;
        }
    }
}
