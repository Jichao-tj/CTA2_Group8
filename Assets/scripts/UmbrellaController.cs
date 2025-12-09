using System.Linq;
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
    [SerializeField] float fireRate = 4f; // shots per second
    float nextTimeToFire = 0f;
    [SerializeField] ParticleSystem muzzleFlash;
    [SerializeField] AudioSource fireSound;
    private AnimationClip fireClip;
    [SerializeField] GameObject shield;
    void Start()
    {
        // Grab the clip once
        fireClip = animator.runtimeAnimatorController.animationClips
            .First(c => c.name == "fire");

        float animLength = fireClip.length;
        float desiredTime = 1f / fireRate;
        animator.speed = animLength / desiredTime;
    }

    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            isOpen = !isOpen;
            animator.SetBool("open", isOpen);
        }

        if(Input.GetMouseButton(0) && !isOpen)
        {
            if (Time.time >= nextTimeToFire)
            {
                nextTimeToFire = Time.time + (1f / fireRate);
                Shoot();
            }

            isShooting = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isShooting = false;
        }

        animator.SetBool("shooting", isShooting);
        shield.SetActive(isOpen);
    }

    void Shoot()
    {
        muzzleFlash.Play();
        fireSound.Play();
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
