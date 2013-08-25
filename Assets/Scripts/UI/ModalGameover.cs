using UnityEngine;
using System.Collections;

public class ModalGameover : UIController {
    public UIEventListener yes;
    public UIEventListener no;

    public UILabel label;

    private float mDelay = 10.0f;

    protected override void OnActive(bool active) {
        if(active) {
            yes.onClick = OnYes;
            no.onClick = OnNo;

            mDelay = 10.0f;
        }
        else {
            yes.onClick = null;
            no.onClick = null;
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    void OnYes(GameObject go) {
        Main.instance.sceneManager.LoadLastSceneStack();
    }

    void OnNo(GameObject go) {
        Main.instance.sceneManager.LoadScene("levelSelect");
    }

    void Update() {
        if(mDelay > 0.0f) {
            mDelay = Mathf.Clamp(mDelay - Time.deltaTime*0.25f, 0, 10);

            label.text = string.Format("CONTINUE? {0}", Mathf.CeilToInt(mDelay));
        }
        else
            OnNo(gameObject);
    }
}
