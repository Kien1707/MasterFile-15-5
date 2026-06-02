using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class VirtualCursor : MonoBehaviour
{
    public RectTransform cursor;
    public float speed = 900f;

    private Vector2 pos;

    void Start()
    {
        pos = cursor.anchoredPosition;
    }

    void Update()
    {
        // Joystick movement
        float x = Input.GetAxis("JoystickLeftX");
        float y = -Input.GetAxis("JoystickLeftY");

        Vector2 move = new Vector2(x, y) * speed * Time.deltaTime;
        pos += move;

        // Clamp
        pos.x = Mathf.Clamp(pos.x, 0, Screen.width);
        pos.y = Mathf.Clamp(pos.y, 0, Screen.height);

        cursor.anchoredPosition = pos;

        // CLICK = X BUTTON
        if (Input.GetKeyDown(KeyCode.JoystickButton2))
            ClickUI();
    }

    void ClickUI()
    {
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
