using UnityEngine;
using System.Collections;

public class PlayerBodyController : MonoBehaviour {

    private Player mPlayer;

    void Awake() {
        mPlayer = transform.parent.GetComponent<Player>();
    }

    void OnProjectileHit(Projectile.HitInfo info) {
        Vector2 dir = (transform.position - info.projectile.transform.position).normalized;

        mPlayer.controller.Hurt(dir, true);
    }
}
