using UnityEngine;
using System.Collections;

public class ModalEnding : UIController {

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
        Main.instance.sceneManager.LoadScene("levelSelect");
    }
}
