using UnityEngine;
using System.Collections;

public class StarItem : MonoBehaviour {
    public string fillRef = "star_fill";

    private AnimatorData mAnim;
    private UISprite mSprite;
    private string mEmptyRef;

    public void ResetData() {
        mSprite.spriteName = mEmptyRef;
    }

    public void Fill() {
        mSprite.spriteName = fillRef;
        mAnim.Play("collect");
    }

    void Awake() {
        mAnim = GetComponent<AnimatorData>();
        mSprite = GetComponent<UISprite>();

        mEmptyRef = mSprite.spriteName;
    }
}
