using UnityEngine;
using System.Collections;

public class LevelSelectItem : MonoBehaviour {
    public string level;
    public string completeKey;

    void Awake() {
    }

    // Use this for initialization
    void Start() {

    }

    void OnClick() {
        Main.instance.sceneManager.LoadScene(level);
    }
}
