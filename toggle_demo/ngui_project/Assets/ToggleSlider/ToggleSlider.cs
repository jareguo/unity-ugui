using UnityEngine;
using System.Collections;

[AddComponentMenu("NGUI/Interaction/ToggleSlider", 50)]
public class ToggleSlider : MonoBehaviour {

    public float slideDistance = 100f;
    public UIWidget handle;

#if UNITY_EDITOR
    [MultilineAttribute(2)] 
    public string readMe = "ToggleSlider的控制类，\n负责处理事件、播放动画等";
#endif

    private bool isOn_ = false;
    public bool isOn {
        get {
            return isOn_;
        }
        set {
            if (isOn_ == value) {
                return;
            }
            isOn_ = value;
            PlayEffects();
        }
    }

    private float percent {
        get {
            return animState.normalizedTime;
        }
        set {
            animState.normalizedTime = value;
        }
    }

    //private float curPercent = 0.0f;
    private Animation anim;
    private AnimationState animState;

    public void Awake () {
        anim = animation;
        animState = anim[anim.clip.name];
    }

    public void Start () {
        // reset effects
        percent = 0;
        Sample();
        // setup event
        UIEventListener.Get(handle.gameObject).onDrag += OnDrag;
        UIEventListener.Get(handle.gameObject).onClick += OnPointerUp;
        UIEventListener.Get(gameObject).onClick += OnPointerUp;
    }

    public void Toggle () {
        if (UIToggle.current != null) {
            isOn = UIToggle.current.value;
        }
        else { 
            isOn = !isOn_;
        }
    }

    public void OnDrag (GameObject go, Vector2 delta) {
        //Debug.Log("OnDrag");
        var step = delta.x / slideDistance;
        percent = Mathf.Clamp01(percent + step);
        Sample();
    }

    public void OnDragEnd () {
        isOn_ = animState.normalizedTime >= 0.5f;
        PlayEffects();
    }

    public void OnPointerUp (GameObject go) {
        Toggle();
    }

    public void Sample () {
        anim.Play(anim.clip.name);
        animState.speed = 0.0f;
        anim.Sample();
    }

    // lerp to target state
    private void PlayEffects() {
        animState.speed = isOn_ ? 1.0f : -1.0f;
        percent = Mathf.Clamp01(percent);
        anim.Play(anim.clip.name);
    }
}
