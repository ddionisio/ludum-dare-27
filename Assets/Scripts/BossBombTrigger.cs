using UnityEngine;
using System.Collections;

public class BossBombTrigger : MonoBehaviour {
    void OnTriggerEnter(Collider col) {
        if(col.gameObject.tag == "Bomb") {
            Player player = Player.instance;

            if(player.state != (int)Player.State.Hurt && player.state != (int)Player.State.Dead) {
                BombController bomb = col.GetComponent<BombController>();
                bomb.Consume(false);
            }
            else {
                //player is hurt or dead, bounce bomb back towards player
                Vector3 dir = player.controller.body.transform.position - col.transform.position;
                dir.Normalize();

                col.rigidbody.velocity = dir * col.rigidbody.velocity.magnitude;
            }
        }
    }
}
