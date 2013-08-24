using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public tk2dSpriteAnimator attachSpriteAnim;
    public AnimatorData attachAnimator;

    private PlatformerController mBody;

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

    }

    void OnDestroy() {
        inputEnabled = false;
    }

    void Awake() {
        mBody = GetComponentInChildren<PlatformerController>();

        mBody.player = 0;
        mBody.moveInputX = InputAction.MoveX;
        mBody.moveInputY = InputAction.MoveY;
        mBody.jumpInput = InputAction.Jump;
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
            if(!attachAnimator.isPlaying) {
                attachAnimator.Play("throw");
            }
        }
    }
}
