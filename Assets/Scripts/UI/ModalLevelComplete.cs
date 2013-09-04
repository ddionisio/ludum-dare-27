using UnityEngine;
using System.Collections;

public class ModalLevelComplete : UIController {
    public UILabel timeLabel;
    public UILabel timeInfoLabel;
    public UISprite timeStar;

    public UILabel deathLabel;
    public UILabel deathInfoLabel;
    public UISprite deathStar; //har-har

    public UILabel newRecordLabel;

    public UILabel starLabel;
    public UISprite[] stars;

    public UIEventListener continueBtn;
    public UIEventListener exitBtn;

    public GameObject instructionsGO;

    public void Ready() {
        UICamera.selectedObject = continueBtn.gameObject;

        instructionsGO.SetActive(true);
    }

    protected override void OnActive(bool active) {
        if(active) {
            continueBtn.onClick = OnContinue;
            exitBtn.onClick = OnExit;
        }
        else {
            continueBtn.onClick = null;
            exitBtn.onClick = null;
        }
    }

    protected override void OnOpen() {
        Init();

        Ready();
    }

    protected override void OnClose() {
    }

    void OnContinue(GameObject go) {
        LevelManager.instance.LevelGotoNext();
    }

    void OnExit(GameObject go) {
        Main.instance.sceneManager.LoadScene("levelSelect");
    }

    void Init() {
        Player player = Player.instance;
        LevelManager.LevelData lvlDat = LevelManager.instance.curLevelData;

        int newRecords = 0;

        if(lvlDat.isNewTime) {
            timeLabel.text = timeLabel.text + "*";
            newRecords++;
        }

        timeInfoLabel.text = string.Format("{0} : {1}", LevelManager.GetTimeText(player.curTime), lvlDat.parTimeText);

        if(lvlDat.isNewDeath) {
            deathLabel.text = deathLabel.text + "*";
        }

        deathInfoLabel.text = player.numDeath > 0 ? player.numDeath.ToString() : "None";

        if(lvlDat.isNewStar) {
            starLabel.text = starLabel.text + "*";
            newRecords++;
        }

        for(int i = 0; i < player.HUD.starsFilled; i++)
            stars[i].spriteName = "star_fill";

        for(int i = player.HUD.starsFilled; i < stars.Length; i++)
            stars[i].spriteName = "star_empty";

        newRecordLabel.gameObject.SetActive(newRecords > 0);

        NGUILayoutBase.RefreshNow(transform);

        instructionsGO.SetActive(false);
    }
}
