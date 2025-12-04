using UnityEngine;
using UnityEngine.InputSystem;

public class UmbrellaController : MonoBehaviour
{
    [SerializeField] Animator animator;
    bool isOpen = false;
    bool isShooting = false;
    [SerializeField] Camera fpsCamera;
    [SerializeField] float range = 100.0f;
    [SerializeField] float damage = 10.0f;
    [SerializeField] float fireRate = 10.0f;
    float fireTimer = 0.0f;
    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            isOpen = !isOpen;
            animator.SetBool("open", isOpen);
        }

        if(Input.GetMouseButton(0) && !isOpen && Time.time>=fireTimer)
        {
            fireTimer = Time.time + 1.0f / fireRate;
            Shoot();
            isShooting = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isShooting = false;
        }

        animator.SetBool("shooting", isShooting);
    }

    void Shoot()
    {
        RaycastHit hit;
        if(Physics.Raycast(fpsCamera.transform.position, fpsCamera.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);
            Target hitTarget = hit.transform.GetComponent<Target>();
            if (hitTarget!=null)
            {
                hitTarget.takeDamage(damage);
            }
        }
    }
}
