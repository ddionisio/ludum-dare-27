using UnityEngine;
using System.Collections;

public class ModalStageSelect : UIController {
    public enum MoveState {
        None,
        Prev,
        Next
    }

    public UILabel title;
    public UILabel desc;

    public Color lockedColor;

    public Transform rotateTrans;
    public tk2dSpriteAnimator planetPrev;
    public tk2dSpriteAnimator planetCur;
    public tk2dSpriteAnimator planetNext;

    public float rotateDelay;

    public UILabel hiScoreLabel;
    public UILabel starMaxLabel;
    public Transform topTrans;

    private MoveState mMoveState = MoveState.None;

    protected override void OnActive(bool active) {
        if(active) {
            Main.instance.input.AddButtonCall(0, InputAction.MenuEscape, OnEsc);
            Main.instance.input.AddButtonCall(0, InputAction.MenuAccept, OnEnter);
        }
        else {
            Main.instance.input.RemoveButtonCall(0, InputAction.MenuEscape, OnEsc);
            Main.instance.input.RemoveButtonCall(0, InputAction.MenuAccept, OnEnter);
        }
    }

    protected override void OnOpen() {

        //initialize data based on current stage selected
        ResetData();

        hiScoreLabel.text = string.Format(GameLocalize.GetText("hiscore"), LevelManager.instance.totalScore);
        starMaxLabel.text = string.Format(GameLocalize.GetText("maxstars"), LevelManager.instance.totalStars);

        NGUILayoutBase.RefreshLate(topTrans);
    }

    protected override void OnClose() {
    }

    void OnEsc(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            UIModalManager.instance.ModalOpen("menu");
        }
    }

    void OnEnter(InputManager.Info dat) {
        if(dat.state == InputManager.State.Released
            && mMoveState == MoveState.None
            && LevelManager.instance.curStageData.unlocked) {
            UIModalManager.instance.ModalOpen("levelSelect");
        }
    }

    void ResetData() {
        mMoveState = MoveState.None;
        rotateTrans.rotation = Quaternion.identity;

        LevelManager lvlMgr = LevelManager.instance;

        planetPrev.Play(lvlMgr.prevStageData.planetRef);
        planetCur.Play(lvlMgr.curStageData.planetRef);
        planetNext.Play(lvlMgr.nextStageData.planetRef);

        planetPrev.Sprite.color = lvlMgr.prevStageData.unlocked ? Color.white : lockedColor;
        planetCur.Sprite.color = lvlMgr.curStageData.unlocked ? Color.white : lockedColor;
        planetNext.Sprite.color = lvlMgr.nextStageData.unlocked ? Color.white : lockedColor;

        title.text = lvlMgr.curStageData.title;
        title.color = lvlMgr.curStageData.unlocked ? Color.white : lockedColor;

        if(lvlMgr.curStageData.unlocked)
            desc.text = lvlMgr.curStageData.desc;
        else
            desc.text = string.Format(GameLocalize.GetText("stage_locked"), lvlMgr.curStageData.starRequire);

        //desc.color = lvlMgr.curStageData.unlocked ? Color.white : lockedColor;

        NGUILayoutBase.RefreshNow(transform);
    }

    void Update() {
        if(mMoveState == MoveState.None) {
            InputManager input = Main.instance.input;

            float inputX = input.GetAxis(0, InputAction.MenuX);

            if(inputX < -0.1f) {
                mMoveState = MoveState.Prev;
                StartCoroutine(DoMove());
            }
            else if(inputX > 0.1f) {
                mMoveState = MoveState.Next;
                StartCoroutine(DoMove());
            }
        }
    }

    IEnumerator DoMove() {
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        float curTime = 0.0f;

        while(true) {
            yield return wait;

            curTime += Time.fixedDeltaTime;

            if(curTime >= rotateDelay) {
                LevelManager lvlMgr = LevelManager.instance;

                switch(mMoveState) {
                    case MoveState.Prev:
                        lvlMgr.curStage = lvlMgr.prevStage;
                        break;

                    case MoveState.Next:
                        lvlMgr.curStage = lvlMgr.nextStage;
                        break;
                }

                ResetData();
                break;
            }
            else {
                Vector3 r = rotateTrans.eulerAngles;

                float t = Holoville.HOTween.Core.Easing.Quad.EaseIn(curTime, 0.0f, 1.0f, rotateDelay, 0, 0);

                switch(mMoveState) {
                    case MoveState.Prev:
                        r.z = -90.0f * t;
                        break;

                    case MoveState.Next:
                        r.z = 90.0f * t;
                        break;
                }

                rotateTrans.eulerAngles = r;
            }
        }
    }
}
