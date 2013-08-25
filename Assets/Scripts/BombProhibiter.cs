using UnityEngine;
using System.Collections;

public class BombProhibiter : MonoBehaviour {

    void OnTriggerEnter(Collider col) {
        if(col.gameObject.tag == "Player") {
            Player p = Player.instance;

            if(p.controller.hasAttach) {
                p.controller.Hurt(transform.up, true);
            }
        }
        else if(col.gameObject.tag == "Bomb") {
            Rigidbody body = col.rigidbody;
            body.velocity = Vector3.Reflect(body.velocity, transform.up);
        }
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
