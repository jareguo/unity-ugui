using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

[AddComponentMenu("UI/ToggleSlider", 50), RequireComponent(typeof(RectTransform))]
public class ToggleSlider : MonoBehaviour {

    public string animName = "ToggleSlider";
    public float slideDistance = 100f;

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
    private bool dragging = false;

    public void Start () {
        anim = animation;
        animState = anim[animName];
        // reset effects
        percent = 0;
        Sample();
    }

    public void Toggle () {
        isOn = !isOn_;
    }

    public void OnDrag (BaseEventData baseEventData) {
        //Debug.Log(baseEventData);
        PointerEventData eventData = baseEventData as PointerEventData;
        if (eventData != null) {
            var step = eventData.delta.x / slideDistance;
            percent = Mathf.Clamp01(percent + step);
            Sample();
            dragging = true;
        }
    }

    public void OnPointerUp (BaseEventData baseEventData) {
        //Debug.Log(baseEventData);
        if (dragging) {
            isOn_ = animState.normalizedTime >= 0.5f;
            PlayEffects();
        }
        else {
            Toggle();
        }
        dragging = false;
    }

    public void Sample () {
        anim.Play(animName);
        animState.speed = 0.0f;
        anim.Sample();
    }

    // lerp to target state
    private void PlayEffects() {
        animState.speed = isOn_ ? 1.0f : -1.0f;
        percent = Mathf.Clamp01(percent);
        anim.Play(animName);
    }
}
