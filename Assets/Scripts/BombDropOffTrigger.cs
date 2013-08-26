using UnityEngine;
using System.Collections;

public class BombDropOffTrigger : MonoBehaviour {
    public tk2dSpriteAnimator anim;

    void OnTriggerEnter(Collider col) {
        if(col.gameObject.tag == "Bomb") {
            Player player = Player.instance;

            bool prevGoal = player.isGoal;

            player.isGoal = true;

            anim.Play("active");

            if(!prevGoal) {
                SoundPlayerGlobal.instance.Play("goal");
            }
        }
    }
}
