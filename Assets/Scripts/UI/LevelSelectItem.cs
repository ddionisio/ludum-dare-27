using UnityEngine;
using System.Collections;

public class LevelSelectItem : MonoBehaviour {
    public UILabel label;
    public UISprite[] stars;

    private int mLevel;

    private UIButtonKeys mButtonKeys;

    public int level { get { return mLevel; } }

    public UIButtonKeys buttonKeys {
        get {
            if(!mButtonKeys) {
                UIButtonKeys[] stuff = GetComponentsInChildren<UIButtonKeys>(true);
                if(stuff.Length > 0)
                    mButtonKeys = stuff[0];
            }

            return mButtonKeys;
        }
    }

    public void Init(int level) {
        mLevel = level;

        LevelManager lvlMgr = LevelManager.instance;

        label.text = lvlMgr.curStageData.levels[level].levelText;

        int numStars = lvlMgr.curStageData.levels[level].stars;

        for(int i = 0; i < numStars; i++) {
            stars[i].spriteName = "icons_star";
        }

        for(int i = numStars; i < stars.Length; i++) {
            stars[i].spriteName = "icons_star_empty";
        }
    }

    void Awake() {

    }



    // Use this for initialization
    void Start() {

    }

    void OnClick() {
        LevelManager.instance.curLevel = level;
        LevelManager.instance.LoadCurrentLevel();
        //Main.instance.sceneManager.LoadScene(level);
    }
}
