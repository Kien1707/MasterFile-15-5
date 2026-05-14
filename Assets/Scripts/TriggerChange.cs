using System.Collections;
using UnityEngine;

public class TriggerChange : MonoBehaviour
{
    private Animator animator;

    [Header("Object To Appear")]
    public GameObject newObject;

    [Header("Timing")]
    public float delayAfterAnimation = 0.5f;
    public float fadeDuration = 2f;

    [Header("Animation")]
    public string triggerName = "Trigger";

    private bool triggered = false;
    private Renderer[] renderers;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (newObject != null)
        {
            renderers = newObject.GetComponentsInChildren<Renderer>();

            // 👉 ANTI POP: tắt render ngay từ đầu
            SetRenderers(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !triggered)
        {
            triggered = true;

            if (animator != null)
                animator.SetTrigger(triggerName);

            StartCoroutine(Flow());
        }
    }

    IEnumerator Flow()
    {
        // 1. wait animation
        yield return new WaitForSeconds(delayAfterAnimation);

        // 2. bật render nhưng vẫn invisible
        SetRenderers(true);
        SetAlpha(0f);

        // 3. fade in
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            float e = Mathf.SmoothStep(0f, 1f, t);

            SetAlpha(e);

            yield return null;
        }

        SetAlpha(1f);
    }

    void SetRenderers(bool state)
    {
        if (renderers == null) return;

        foreach (var r in renderers)
            r.enabled = state;
    }

    void SetAlpha(float alpha)
    {
        if (renderers == null) return;

        foreach (var r in renderers)
        {
            foreach (var mat in r.materials)
            {
                if (mat.HasProperty("_Color"))
                {
                    Color c = mat.color;
                    c.a = alpha;
                    mat.color = c;
                }
            }
        }
    }
}