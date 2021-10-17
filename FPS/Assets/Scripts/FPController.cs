using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class FPController : MonoBehaviour
{
    public GameObject cam;
    public Animator anim;
    public AudioSource[] footsteps;
    public AudioSource jump;
    public AudioSource land;
    public AudioSource ammoPickupSound;
    public AudioSource triggerSound;
    public AudioSource reloadSound;
    public AudioSource healthPickupSound;
    public AudioSource deathSound;
    private float speed = 0.1f;
    private float Xsensitivity = 2;
    private float Ysensitivity = 2;
    private float MinimumX = -90;
    private float MaximumX = 90;
    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Quaternion cameraRot;
    private Quaternion characterRot;

    private bool cursorIsLocked = true;
    private bool lockCursor = true;
    private bool playingWalking;
    private bool previouslyGrounded = true;

    private float x;
    private float z;

    private int ammo;
    private int maxAmmo = 50;
    private int ammoClip;
    private int ammoClipMax = 10;

    private float health;
    private float maxHealth = 100f;
    private bool isAlive = true;

    private static readonly int ARM = Animator.StringToHash("arm");
    private static readonly int FIRE = Animator.StringToHash("fire");
    private static readonly int RELOAD = Animator.StringToHash("reload");
    private static readonly int WALKING = Animator.StringToHash("walking");

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        cameraRot = cam.transform.localRotation;
        characterRot = transform.localRotation;

        health = maxHealth;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            anim.SetBool(ARM, !anim.GetBool(ARM));

        if (Input.GetMouseButtonDown(0) 
            && anim.GetBool(ARM)
            && !anim.GetBool(FIRE))
        {
            if (ammoClip > 0)
            {
                anim.SetTrigger(FIRE);
                ammoClip -= 1;
                Debug.Log($"Remaining ammo in a clip: {ammoClip}, spare ammo: {ammo}");
            }
            else
            {
                if (ammo == 0)
                {
                    triggerSound.Play();
                }
                else
                {
                    ReloadHandler();
                }
            }
            //shot.Play();
        }

        if (Input.GetKeyDown(KeyCode.R) 
            && anim.GetBool(ARM) 
            && ammo > 0 
            && ammoClip < ammoClipMax)
        {
            ReloadHandler();
        }

        if (Mathf.Abs(x) > 0 || Mathf.Abs(z) > 0)
        {
            if (!anim.GetBool(WALKING) && IsGrounded())
            {
                anim.SetBool(WALKING, true);
                InvokeRepeating(nameof(PlayFootStepAudio), 0, 0.4f);
            }
        }
        else if (anim.GetBool(WALKING))
        {
            anim.SetBool(WALKING, false);
            CancelInvoke(nameof(PlayFootStepAudio));
            playingWalking = false;
        }

        bool grounded = IsGrounded();
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(0, 300, 0);
            jump.Play();
            if (anim.GetBool(WALKING))
            {
                CancelInvoke(nameof(PlayFootStepAudio));
                playingWalking = false;
            }
        }
        else if (!previouslyGrounded && grounded)
        {
            land.Play();
        }

        previouslyGrounded = grounded;
    }

    private void PlayFootStepAudio()
    {
        int n = Random.Range(1, footsteps.Length);

        var audioSource = footsteps[n];
        audioSource.Play();
        footsteps[n] = footsteps[0];
        footsteps[0] = audioSource;
        playingWalking = true;
    }

    private void ReloadHandler()
    {
        var reloadedAmount = Mathf.Min(ammo, ammoClipMax - ammoClip);
        ammo -= reloadedAmount;
        ammoClip += reloadedAmount;
        anim.SetTrigger(RELOAD);
        reloadSound.Play();
    }

    private void FixedUpdate()
    {
        if (isAlive)
        {
            float yRot = Input.GetAxis("Mouse X") * Ysensitivity;
            float xRot = Input.GetAxis("Mouse Y") * Xsensitivity;

            cameraRot *= Quaternion.Euler(-xRot, 0, 0);
            characterRot *= Quaternion.Euler(0, yRot, 0);

            cameraRot = ClampRotationAroundXAxis(cameraRot);

            transform.localRotation = characterRot;
            cam.transform.localRotation = cameraRot;

            x = Input.GetAxis("Horizontal") * speed;
            z = Input.GetAxis("Vertical") * speed;

            transform.position += cam.transform.forward * z + cam.transform.right * x; //new Vector3(x * speed, 0, z * speed);
        }

        UpdateCursorLock();
    }

    private Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

    private bool IsGrounded()
    {
        return Physics.SphereCast(transform.position, capsule.radius, Vector3.down, out var hitInfo,
            (capsule.height / 2f) - capsule.radius + 0.1f);
    }

    private void OnCollisionEnter(Collision col)
    {
        if (IsGrounded())
        {
            if (anim.GetBool(WALKING) && !playingWalking)
            {
                InvokeRepeating(nameof(PlayFootStepAudio), 0, 0.4f);
                playingWalking = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var second = other.gameObject;
        if (second.CompareTag("Ammo") && ammo < maxAmmo)
        {
            ammo = Mathf.Clamp(ammo + 10, 0, maxAmmo);
            Debug.Log($"Ammo picked up: {ammo}");
            ammoPickupSound.Play();
            Destroy(second);
        }
        else if (second.CompareTag("Medikit") && health < maxHealth)
        {
            health = Mathf.Clamp(health + 10, 0, maxHealth);
            Debug.Log($"Medikit picked up: {health:n2}");
            healthPickupSound.Play();
            Destroy(second);
        }
        
    }

    private void OnTriggerStay(Collider other)
    {
        var second = other.gameObject;
        
        if (isAlive && second.CompareTag("Lava"))
        {
            health = Mathf.Clamp(health - (10 * Time.deltaTime), 0, 100);
            Debug.Log($"Health: {health:n2}");
            if (health <= 0)
            {
                DeathHandler();
            }
        }
    }

    public void SetCursorLock(bool value)
    {
        lockCursor = value;
        
        if (lockCursor) return;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void DeathHandler()
    {
        if (isAlive)
        {
            isAlive = false;
            deathSound.Play();
            Debug.Log("You died");
        }
    }

    private void UpdateCursorLock()
    {
        if (lockCursor)
            InternalLockUpdate();
    }

    private void InternalLockUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            cursorIsLocked = false;
        else if (Input.GetMouseButtonUp(0))
            cursorIsLocked = true;

        switch (cursorIsLocked)
        {
            case true:
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                break;
            case false:
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }
    }
}