using UnityEngine;
using System.Collections;

public class DebugToggleSlider : MonoBehaviour {

    public ToggleSlider toggleSlider;
    private UIToggle toggle;

#if UNITY_EDITOR
    [MultilineAttribute(2)] 
    public string readMe = "从外部设置ToggleSlider的值，\n测试它的状态是否更新";
#endif

    void Awake () {
        toggle = GetComponent<UIToggle>();
    }

	void Update () {
        UIToggle.current = toggle;
        toggle.value = toggleSlider.isOn;
        UIToggle.current = null;
	}
}
