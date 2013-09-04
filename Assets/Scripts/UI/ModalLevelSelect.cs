using UnityEngine;
using System.Collections;

public class ModalLevelSelect : UIController {
    public UILabel title;

    private LevelSelectItem[] mLevels;
    private int mLevelSelectInd;
    private bool mIsInit = false;

    protected override void OnActive(bool active) {
        if(active) {
            UICamera.selectedObject = mLevelSelectInd == -1 ? mLevels[0].gameObject : mLevels[mLevelSelectInd].gameObject;

            StartCoroutine(NGUILayoutBase.RefreshLate(transform));
        }
        else {

        }
    }

    protected override void OnOpen() {
        Init();
    }

    protected override void OnClose() {
    }

    int Compare(LevelSelectItem itm1, LevelSelectItem itm2) {
        return itm1.gameObject.name.CompareTo(itm2.gameObject.name);
    }

    void Init() {
        if(!mIsInit) {
            mLevels = GetComponentsInChildren<LevelSelectItem>(true);
            System.Array.Sort(mLevels, Compare);

            mIsInit = true;
        }

        LevelManager lvlMgr = LevelManager.instance;

        int lastLevelUnlocked = 0;

        int numLevels = lvlMgr.curStageData.levels.Length;
        int numItemLevels = mLevels.Length;

        for(int i = 0; i < numLevels; i++) {
            LevelSelectItem itm = mLevels[i];
            itm.Init(i);

            if(i == 0 || lvlMgr.curStageData.levels[i - 1].completed) {
                lastLevelUnlocked = i;
            }
        }

        //hide excess level items
        for(int i = numLevels; i < numItemLevels; i++) {
            LevelSelectItem itm = mLevels[i];
            itm.gameObject.SetActive(false);
        }

        //determine connections
        mLevels[0].buttonKeys.selectOnUp = mLevels[numLevels - 1].buttonKeys;
        mLevels[numLevels - 1].buttonKeys.selectOnDown = mLevels[0].buttonKeys;

        for(int i = 0; i < numLevels; i++) {
            if(i > 0)
                mLevels[i].buttonKeys.selectOnUp = mLevels[i - 1].buttonKeys;

            if(i < numLevels - 1)
                mLevels[i].buttonKeys.selectOnDown = mLevels[i + 1].buttonKeys;
        }

        mLevelSelectInd = lastLevelUnlocked < numLevels - 1 ? lastLevelUnlocked : 0;

        title.text = lvlMgr.curStageData.title;
    }
}
