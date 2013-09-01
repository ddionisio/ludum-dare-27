using UnityEngine;
using System.Collections;

public class LevelSelectItem : MonoBehaviour {
    public int level;
    public UILabel label;
    public UISprite[] stars;

    public void Init() {
        
    }

    void Awake() {
        
    }

    

    // Use this for initialization
    void Start() {
        
    }

    void OnClick() {
        //Main.instance.sceneManager.LoadScene(level);
    }
}
