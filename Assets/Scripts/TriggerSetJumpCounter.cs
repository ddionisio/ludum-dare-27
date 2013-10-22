using UnityEngine;
using System.Collections;

public class TriggerSetJumpCounter : MonoBehaviour {
    public string checkTag = "Player";
    public int counter = 1;

    void OnTriggerEnter(Collider col) {
        if(col.gameObject.CompareTag(checkTag)) {
            PlatformerController ctrl = col.GetComponent<PlatformerController>();
            if(!ctrl.isGrounded)
                ctrl.jumpCounterCurrent = counter;
        }
    }
}
