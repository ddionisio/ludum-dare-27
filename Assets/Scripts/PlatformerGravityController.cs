using UnityEngine;
using System.Collections;

public class PlatformerGravityController : GravityController {

    protected override void ApplyUp() {
        if(orientUp) {
            if(!mIsOrienting) {
                Vector2 tup = transform.up;
                Vector2 toUp = up;
                float side = M8.MathUtil.CheckSideSign(tup, toUp);
                mRotateTo = transform.rotation * Quaternion.Euler(0, 0, side*Vector2.Angle(tup, toUp));
                StartCoroutine(OrientUp());
            }
        }
    }
}
