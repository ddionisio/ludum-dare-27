using UnityEngine;
using System.Collections;

public class TriggerAnimatorPlay : MonoBehaviour {
    public AnimatorData target;
    public string take;
    public bool forcePlay;

    void Awake() {
        if(target == null)
            target = GetComponent<AnimatorData>();
    }

    void OnTriggerEnter(Collider col) {
        if(target != null) {
            if(forcePlay || !target.isPlaying)
                target.Play(take);
        }
    }
}
