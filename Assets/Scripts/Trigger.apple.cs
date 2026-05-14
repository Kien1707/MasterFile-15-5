using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

public class TriggerApple : MonoBehaviour
{
    private Animator animator ;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (animator != null)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Debug.Log("Enter key pressed!");  // ← Add this
        
        if (animator != null)
        {
            Debug.Log("Animator found! Triggering...");  // ← Add this
            animator.SetTrigger("Trigger.apple");
        }
        else
        {
            Debug.LogError("Animator is NULL!");  // ← Add this
        }
                animator.SetTrigger("Trigger.apple");
            }
        }
    }
}
