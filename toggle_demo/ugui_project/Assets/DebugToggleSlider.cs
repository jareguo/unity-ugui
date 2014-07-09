using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DebugToggleSlider : MonoBehaviour {

    public ToggleSlider toggleSlider;
    private Toggle toggle;

#if UNITY_EDITOR
    [MultilineAttribute(2)] 
    public string readMe = "从外部设置ToggleSlider的值，\n测试它的状态是否更新";
#endif

    void Awake () {
        toggle = GetComponent<Toggle>();
    }

	void Update () {
        toggle.isOn = toggleSlider.isOn;
	}
}
