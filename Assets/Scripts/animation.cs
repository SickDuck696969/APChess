using UnityEngine;
using UnityEngine.EventSystems;

public class animation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Animation Clips")]
    public AnimationClip hoverClip;
    public AnimationClip hoverExitClip;
    public AnimationClip clickClip;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverClip != null && animator != null)
        {
            animator.Play(hoverClip.name);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverExitClip != null && animator != null)
        {
            animator.Play(hoverExitClip.name);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickClip != null && animator != null)
        {
            animator.Play(clickClip.name);
        }
    }
}
