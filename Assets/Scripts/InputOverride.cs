using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class InputOverride : MonoBehaviour
{
    // ============================
    //  PUBLIC STATIC OVERRIDES
    // ============================
    public static Func<string, float> AxisOverride;
    public static Func<KeyCode, bool> KeyDownOverride;
    public static Func<KeyCode, bool> KeyOverride;
    public static Func<KeyCode, bool> KeyUpOverride;
    public static Func<float> ScrollOverride;

    void Awake()
    {
        AxisOverride = TranslateAxis;
        KeyDownOverride = TranslateKeyDown;
        KeyOverride = TranslateKey;
        KeyUpOverride = TranslateKeyUp;
        ScrollOverride = TranslateScroll;
    }

    // ============================
    //  AXIS TRANSLATION
    // ============================
    float TranslateAxis(string axis)
    {
        switch (axis)
        {
            case "Horizontal":
                return Input.GetAxis("JoystickLeftX");
            case "Vertical":
                return Input.GetAxis("JoystickLeftY") * -1f;
            case "Mouse X":
                return Input.GetAxis("JoystickRightX");
            case "Mouse Y":
                return Input.GetAxis("JoystickRightY") * -1f;
        }
        return Input.GetAxis(axis);
    }

    // ============================
    //  KEY TRANSLATION
    // ============================
    bool TranslateKeyDown(KeyCode key)
    {
        if (key == KeyCode.E) return Input.GetKeyDown(KeyCode.JoystickButton2); // X
        if (key == KeyCode.F) return Input.GetKeyDown(KeyCode.JoystickButton1); // B
        if (key == KeyCode.Space) return Input.GetKeyDown(KeyCode.JoystickButton3); // Y
        if (key == KeyCode.LeftShift) return Input.GetKeyDown(KeyCode.JoystickButton4); // LB
        return Input.GetKeyDown(key);
    }

    bool TranslateKey(KeyCode key)
    {
        if (key == KeyCode.E) return Input.GetKey(KeyCode.JoystickButton2);
        if (key == KeyCode.F) return Input.GetKey(KeyCode.JoystickButton1);
        if (key == KeyCode.Space) return Input.GetKey(KeyCode.JoystickButton3);
        if (key == KeyCode.LeftShift) return Input.GetKey(KeyCode.JoystickButton4);
        return Input.GetKey(key);
    }

    bool TranslateKeyUp(KeyCode key)
    {
        if (key == KeyCode.E) return Input.GetKeyUp(KeyCode.JoystickButton2);
        if (key == KeyCode.F) return Input.GetKeyUp(KeyCode.JoystickButton1);
        if (key == KeyCode.Space) return Input.GetKeyUp(KeyCode.JoystickButton3);
        if (key == KeyCode.LeftShift) return Input.GetKeyUp(KeyCode.JoystickButton4);
        return Input.GetKeyUp(key);
    }

    // ============================
    //  SCROLL WHEEL TRANSLATION
    // ============================
    float TranslateScroll()
    {
        float scroll = 0f;

        if (Input.GetAxis("LT") > 0.2f) scroll = +1f;
        if (Input.GetAxis("RT") > 0.2f) scroll = -1f;

        return scroll;
    }

    // ============================
    //  CLICK UI = A BUTTON
    // ============================
    public static void ClickUIWithA(RectTransform cursor)
    {
        if (!Input.GetKeyDown(KeyCode.JoystickButton0)) return; // A button

        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = cursor.position;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        foreach (var r in results)
        {
            Button b = r.gameObject.GetComponent<Button>();
            if (b != null)
            {
                b.onClick.Invoke();
                break;
            }
        }
    }
}
