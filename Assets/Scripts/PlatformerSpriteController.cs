using UnityEngine;
using System.Collections;

public class PlatformerSpriteController : MonoBehaviour {
    public tk2dSpriteAnimator anim;
    public PlatformerController controller;

    public string idleClip = "idle";
    public string moveClip = "move";
    public string upClip = "up";
    public string downClip = "down";

    private tk2dSpriteAnimationClip mIdle;
    private tk2dSpriteAnimationClip mMove;
    private tk2dSpriteAnimationClip mUp;
    private tk2dSpriteAnimationClip mDown;

    private bool mIsLeft;

    public bool isLeft { get { return mIsLeft; } }

    void Awake() {
        if(anim == null)
            anim = GetComponent<tk2dSpriteAnimator>();

        mIdle = anim.GetClipByName(idleClip);
        mMove = anim.GetClipByName(moveClip);
        mUp = anim.GetClipByName(upClip);
        mDown = anim.GetClipByName(downClip);

        if(controller == null)
            controller = GetComponent<PlatformerController>();
    }

    // Update is called once per frame
    void Update() {
        if(controller.isGrounded) {
            if(controller.moveSide != 0.0f) {
                anim.Play(mMove);
            }
            else {
                anim.Play(mIdle);
            }
        }
        else {
            Vector2 up = controller.dirHolder.up;
            Vector2 vel = controller.rigidbody.velocity;

            if(Vector2.Angle(up, vel) > 90) {
                anim.Play(mDown);
            }
            else {
                anim.Play(mUp);
            }
        }

        if(controller.localVelocity.x != 0.0f) {
            mIsLeft = controller.localVelocity.x < 0.0f;
            anim.Sprite.FlipX = mIsLeft;
        }
    }
}
