using UnityEngine;
using System.Collections;

public class ModalVictory : UIController {

    protected override void OnActive(bool active) {
        if(active) {
        }
        else {
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    void OnClick() {
        int complete = SceneState.instance.GetGlobalValue("numLevelComplete");
        int max = SceneState.instance.GetGlobalValue("numLevels");

        if(complete < max)
            Main.instance.sceneManager.LoadScene("levelSelect");
        else
            Main.instance.sceneManager.LoadScene("ending");
    }
}
