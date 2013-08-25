using UnityEngine;
using System.Collections;

public class ModalLevelSelect : UIController {
    private LevelSelectItem[] mLevels;
    private int mLevelSelectInd;
    private bool mIsInit = false;

    protected override void OnActive(bool active) {
        if(active) {
            Init();

            Main.instance.input.AddButtonCall(0, InputAction.MenuEscape, OnEsc);

            UICamera.selectedObject = mLevelSelectInd == -1 ? mLevels[0].gameObject : mLevels[mLevelSelectInd].gameObject;
        }
        else {
            Main.instance.input.RemoveButtonCall(0, InputAction.MenuEscape, OnEsc);
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    void OnEsc(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            UIModalManager.instance.ModalOpen("menu");
        }
    }

    int Compare(LevelSelectItem itm1, LevelSelectItem itm2) {
        return itm1.gameObject.name.CompareTo(itm2.gameObject.name);
    }

    void Init() {
        if(mIsInit)
            return;

        mIsInit = true;

        mLevels = GetComponentsInChildren<LevelSelectItem>(true);
        System.Array.Sort(mLevels, Compare);

        mLevelSelectInd = -1;

        int numComplete = 0;
        int numLevels = mLevels.Length;

        for(int i = 0; i < mLevels.Length; i++) {
            LevelSelectItem itm = mLevels[i];

            itm.Init();

            if(itm.completed) numComplete++;
            else if(mLevelSelectInd == -1) mLevelSelectInd = i;
        }

        SceneState.instance.SetGlobalValue("numLevels", numLevels, false);
        SceneState.instance.SetGlobalValue("numLevelComplete", numComplete, false);
    }
}
