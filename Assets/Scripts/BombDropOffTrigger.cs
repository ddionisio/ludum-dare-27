using UnityEngine;
using System.Collections;

public class BombDropOffTrigger : MonoBehaviour {
    public tk2dSpriteAnimator anim;

    void OnTriggerEnter(Collider col) {
        if(col.gameObject.tag == "Bomb") {
            Player player = Player.instance;
            player.isGoal = true;

            anim.Play("active");
        }
    }
}
