using UnityEngine;
using System.Collections;

public class LevelSelectItem : MonoBehaviour {
    public string level;

    public GameObject[] completedObjs;

    private bool mCompleted;
    

    public bool completed { get { return mCompleted; } }

    public void Init() {
        bool isComplete = UserData.instance.GetInt(level, 0) > 0;

        foreach(GameObject go in completedObjs)
            go.SetActive(isComplete);

        mCompleted = isComplete;
    }

    void Awake() {
        
    }

    

    // Use this for initialization
    void Start() {
        
    }

    void OnClick() {
        Main.instance.sceneManager.LoadScene(level);
    }
}
