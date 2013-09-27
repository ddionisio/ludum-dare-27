using UnityEngine;
using System.Collections;

public class TriggerCheckpoint : MonoBehaviour {

    private PlayMakerFSM mFSM;

    public void Revert() {
        if(mFSM)
            mFSM.SendEvent(EntityEvent.Restore);
    }

    void Awake() {
        mFSM = GetComponent<PlayMakerFSM>();
    }
}
