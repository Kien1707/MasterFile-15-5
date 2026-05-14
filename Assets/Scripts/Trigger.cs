using System.Collections ;
using System.Collections.Generic ;
using UnityEngine ;

public class Trigger : MonoBehaviour
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
                animator.SetTrigger("Trigger");
            }
        }
    }
}
