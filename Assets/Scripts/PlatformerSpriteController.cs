using UnityEngine;
using System.Collections;

public class PlatformerSpriteController : MonoBehaviour {
    public delegate void Callback(PlatformerSpriteController ctrl);

    public tk2dSpriteAnimator anim;
    public PlatformerController controller;

    public string idleClip = "idle";
    public string moveClip = "move";
    public string upClip = "up";
    public string downClip = "down";
    public string upGlowClip = "upGlow";
    public string downGlowClip = "downGlow";
    public string wallStickClip = "wall";
    public string wallJumpClip = "wallJump";

    public ParticleSystem wallStickParticle;

    public event Callback flipCallback;

    private tk2dSpriteAnimationClip mIdle;
    private tk2dSpriteAnimationClip mMove;
    private tk2dSpriteAnimationClip mUp;
    private tk2dSpriteAnimationClip mDown;
    private tk2dSpriteAnimationClip mUpGlow;
    private tk2dSpriteAnimationClip mDownGlow;
    private tk2dSpriteAnimationClip mWallStick;
    private tk2dSpriteAnimationClip mWallJump;

    private bool mIsLeft;
    private bool mAnimationActive = true;

    public bool isLeft { get { return mIsLeft; } }
    public bool animationActive { get { return mAnimationActive; } set { mAnimationActive = value; } }

    public void ResetAnimation() {
        mAnimationActive = true;
        mIsLeft = false;
        if(anim && anim.Sprite)
            anim.Sprite.FlipX = false;

        wallStickParticle.loop = false;
        wallStickParticle.Stop();
    }

    void OnDestroy() {
        flipCallback = null;
    }

    void Awake() {
        if(anim == null)
            anim = GetComponent<tk2dSpriteAnimator>();

        mIdle = anim.GetClipByName(idleClip);
        mMove = anim.GetClipByName(moveClip);
        mUp = anim.GetClipByName(upClip);
        mDown = anim.GetClipByName(downClip);
        mUpGlow = anim.GetClipByName(upGlowClip);
        mDownGlow = anim.GetClipByName(downGlowClip);
        mWallStick = anim.GetClipByName(wallStickClip);
        mWallJump = anim.GetClipByName(wallJumpClip);

        if(controller == null)
            controller = GetComponent<PlatformerController>();
    }

    // Update is called once per frame
    void Update() {
        if(mAnimationActive) {
            bool left = mIsLeft;

            if(controller.isJumpWall) {
                anim.Play(mWallJump);

                left = controller.localVelocity.x < 0.0f;
            }
            else if(controller.isWallStick) {
                if(wallStickParticle.isStopped) {
                    wallStickParticle.Play();
                }

                wallStickParticle.loop = true;

                anim.Play(mWallStick);

                left = M8.MathUtil.CheckSide(controller.wallStickCollide.normal, controller.dirHolder.up) == M8.MathUtil.Side.Right;

            }
            else {
                wallStickParticle.loop = false;

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
                        anim.Play(controller.jumpCounterCurrent < controller.jumpCounter ? mDownGlow : mDown);
                    }
                    else {
                        anim.Play(controller.jumpCounterCurrent < controller.jumpCounter ? mUpGlow : mUp);
                    }
                }

                if(controller.moveSide != 0.0f) {
                    left = controller.moveSide < 0.0f;
                }
            }

            if(mIsLeft != left) {
                mIsLeft = left;

                anim.Sprite.FlipX = mIsLeft;

                if(flipCallback != null)
                    flipCallback(this);
            }
        }
    }
}
