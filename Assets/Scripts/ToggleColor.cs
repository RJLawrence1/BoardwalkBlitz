// TOGGLECOLOR.CS  -  put it on a Toggle.
// Colors the checkbox by its VALUE: one color when it's ON, another when it's OFF.
// (The Toggle's own "Color Tint" colors are for hovering/clicking, not on/off.)

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleColor : MonoBehaviour
{
    public Graphic box;                                   // the checkbox square (Background)
    public Color onColor = new Color(0.2f, 1f, 0.2f);     // green when ON
    public Color offColor = new Color(1f, 0.2f, 0.15f);   // red when OFF

    Toggle toggle;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(_ => Refresh());   // recolor every time it flips
    }

    void Start()
    {
        Refresh();   // set the right color once the saved setting is loaded
    }

    void Refresh()
    {
        if (box != null) box.color = toggle.isOn ? onColor : offColor;
    }
}