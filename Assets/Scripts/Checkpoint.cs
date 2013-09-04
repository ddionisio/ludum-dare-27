using UnityEngine;
using System.Collections;

public class Checkpoint : MonoBehaviour {
    public Transform point;

    private AnimatorData mAnim;

    public void ResetState() {
        mAnim.Play("default");
        collider.enabled = true;
    }

    void OnTriggerEnter(Collider c) {
        if(c.gameObject.tag == "Player") {
            Player player = Player.instance;
            player.SetCheckPoint(this);

            mAnim.Play("active");
            collider.enabled = false;
        }
    }

    void Awake() {
        mAnim = GetComponent<AnimatorData>();
    }
}
