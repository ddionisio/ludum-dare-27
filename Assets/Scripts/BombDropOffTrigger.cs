using UnityEngine;
using System.Collections;

public class BombDropOffTrigger : MonoBehaviour {
    public AnimatorData anim;
    public string normal = "normal";
    public string goal = "eat";

    public void ResetData() {
        anim.Play(normal);
    }

    void OnTriggerEnter(Collider col) {
        if(col.gameObject.tag == "Bomb") {
            Player player = Player.instance;

            bool prevGoal = player.isGoal;

            player.isGoal = true;

            anim.Play(goal);

            if(!prevGoal) {
                SoundPlayerGlobal.instance.Play("goal");
            }

            BombController bomb = col.GetComponent<BombController>();
            bomb.Consume();
        }
    }
}
