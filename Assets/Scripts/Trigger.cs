using UnityEngine;

public class Trigger : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (animator == null) return;

        // F (keyboard) OR B (controller override)
        bool pressed =
            Input.GetKeyDown(KeyCode.F) ||          // keyboard F
            InputOverride.KeyDownOverride(KeyCode.F); // controller B

        if (pressed)
        {
            animator.SetTrigger("Trigger");
        }
    }
}
