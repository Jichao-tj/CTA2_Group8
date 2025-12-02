using UnityEngine;
using UnityEngine.InputSystem;

public class UmbrellaController : MonoBehaviour
{
    [SerializeField] Animator animator;
    bool isOpen = false;

    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            isOpen = !isOpen;
            animator.SetBool("open", isOpen);
        }
    }
}
