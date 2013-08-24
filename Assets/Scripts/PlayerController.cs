using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public Transform attachPoint;
    public tk2dSpriteAnimator attachSpriteAnim;
    public AnimatorData attachAnimator;
    public GameObject bomb;

    public float throwAngle = 30;

    public SpriteColorBlink[] spriteBlinks;

    private Player mPlayer;
    private PlatformerController mBody;
    private PlatformerSpriteController mBodySpriteCtrl;

    private bool mInputEnabled = false;

    public bool inputEnabled {
        get { return mInputEnabled; }
        set {
            if(mInputEnabled != value) {
                mInputEnabled = value;

                mBody.inputEnabled = mInputEnabled;

                InputManager input = Main.instance ? Main.instance.input : null;
                if(input) {
                    if(mInputEnabled) {
                        input.AddButtonCall(0, InputAction.Action, OnInputAction);
                    }
                    else {
                        input.RemoveButtonCall(0, InputAction.Action, OnInputAction);
                    }
                }
            }
        }
    }

    public PlatformerController body { get { return mBody; } }

    public void Throw(float impulse) {
        attachSpriteAnim.Play("empty");

        bomb.transform.position = attachPoint.position;
        bomb.transform.rotation = attachPoint.rotation;
        bomb.rigidbody.velocity = Vector3.zero;

        bomb.SetActive(true);

        Vector3 dir = mBodySpriteCtrl.isLeft ? -mBody.dirHolder.right : mBody.dirHolder.right;

        Quaternion rot = Quaternion.AngleAxis(throwAngle, mBodySpriteCtrl.isLeft ? -Vector3.forward : Vector3.forward);

        dir = rot * dir;
                
        bomb.rigidbody.AddForce(dir * impulse, ForceMode.Impulse);

        GravityController bombGrav = bomb.GetComponent<GravityController>();
        bombGrav.up = mBody.gravityController.up;
    }

    public void BombActive() {
        if(bomb)
            bomb.SetActive(false);

        if(attachSpriteAnim)
            attachSpriteAnim.Play("bomb");
    }

    void ResetData() {
        foreach(SpriteColorBlink blink in spriteBlinks) {
            if(blink)
                blink.enabled = false;
        }

        if(attachAnimator) {
            attachAnimator.transform.localScale = Vector3.one;
            attachAnimator.Stop();
        }

        if(mBodySpriteCtrl)
            mBodySpriteCtrl.ResetAnimation();
    }

    void OnDestroy() {
        inputEnabled = false;
    }

    void Awake() {
        mPlayer = GetComponent<Player>();
        mPlayer.spawnCallback += OnPlayerSpawn;
        mPlayer.setStateCallback += OnPlayerSetState;
        mPlayer.setBlinkCallback += OnPlayerBlink;

        mBody = GetComponentInChildren<PlatformerController>();

        mBody.player = 0;
        mBody.moveInputX = InputAction.MoveX;
        mBody.moveInputY = InputAction.MoveY;
        mBody.jumpInput = InputAction.Jump;

        mBodySpriteCtrl = mBody.GetComponent<PlatformerSpriteController>();
        mBodySpriteCtrl.flipCallback += OnFlipCallback;

        ResetData();
    }

    // Use this for initialization
    void Start() {
        inputEnabled = true;
    }

    // Update is called once per frame
    void Update() {

    }

    void OnInputAction(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            if(!bomb.activeSelf && !attachAnimator.isPlaying) {
                attachAnimator.Play("throw");
            }
        }
    }

    void OnPlayerSpawn(EntityBase ent) {
        BombActive();
    }

    void OnPlayerSetState(EntityBase ent, int state) {
        switch((Player.State)state) {
            case Player.State.Normal:
                mBodySpriteCtrl.animationActive = true;

                
                break;

            case Player.State.Hurt:
                attachAnimator.Stop();
                break;

            case Player.State.Dead:
                attachAnimator.Stop();
                break;

            case Player.State.Invalid:
                ResetData();
                break;
        }
    }

    void OnPlayerBlink(EntityBase ent, bool b) {
    }

    void OnFlipCallback(PlatformerSpriteController ctrl) {
        Transform attach = attachAnimator.transform;

        Vector3 s = attach.localScale;

        s.x = ctrl.isLeft ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);

        attach.localScale = s;
    }
}
