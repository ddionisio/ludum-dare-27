using UnityEngine;
using System.Collections;

public class TriggerLockPlatformerDrag : MonoBehaviour {
    public string[] checkTags = { "Player" };
    public float drag;

    private PlatformerController mController;

    bool MatchTag(GameObject go) {
        for(int i = 0, max = checkTags.Length; i < max; i++) {
            if(go.CompareTag(checkTags[i]))
                return true;
        }

        return false;
    }

    void OnDisable() {
        if(mController)
            mController.lockDrag = false;
    }

    void OnTriggerEnter(Collider col) {
        if(MatchTag(col.gameObject)) {
            mController = col.GetComponent<PlatformerController>();
            mController.lockDrag = true;
            col.rigidbody.drag = drag;
        }
    }

    void OnTriggerExit(Collider col) {
        if(MatchTag(col.gameObject)) {
            PlatformerController ctrl = col.GetComponent<PlatformerController>();
            if(mController == ctrl) {
                ctrl.lockDrag = false;
                mController = null;
            }
        }
    }
}
