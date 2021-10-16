using UnityEngine;

public class FPController : MonoBehaviour
{
    public GameObject cam;
    public Animator anim;
    public AudioSource[] footsteps;
    public AudioSource jump;
    public AudioSource land;
    public AudioSource ammoPickupSound;
    public AudioSource healthPickupSound;
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

    private float x;
    private float z;
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
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            anim.SetBool(ARM, !anim.GetBool(ARM));

        if (Input.GetMouseButtonDown(0) && !anim.GetBool(FIRE))
        {
            anim.SetTrigger(FIRE);
            //shot.Play();
        }

        if (Input.GetKeyDown(KeyCode.R))
            anim.SetTrigger(RELOAD);

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
        }

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.AddForce(0, 300, 0);
            jump.Play();
            if (anim.GetBool(WALKING))
            {
                CancelInvoke(nameof(PlayFootStepAudio));
            }
        }

    }

    private void PlayFootStepAudio()
    {
        int n = Random.Range(1, footsteps.Length);

        var audioSource = footsteps[n];
        audioSource.Play();
        footsteps[n] = footsteps[0];
        footsteps[0] = audioSource;
    }


    private void FixedUpdate()
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
        if (Physics.SphereCast(transform.position, capsule.radius, Vector3.down, out var hitInfo,
                (capsule.height / 2f) - capsule.radius + 0.1f))
        {
            return true;
        }
        return false;
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ammo"))
        {
            Debug.Log("Ammo picked up");  
            ammoPickupSound.Play();
            Destroy(col.gameObject);
        }
        else if (col.gameObject.CompareTag("Medikit"))
        {
            Debug.Log("Medikit picked up");
            healthPickupSound.Play();
            Destroy(col.gameObject);
        }
        else if (IsGrounded())
        {
            land.Play();
            if(anim.GetBool(WALKING))
                InvokeRepeating(nameof(PlayFootStepAudio), 0, 0.4f);
        }
    }

    public void SetCursorLock(bool value)
    {
        lockCursor = value;
        if (!lockCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void UpdateCursorLock()
    {
        if (lockCursor)
            InternalLockUpdate();
    }

    public void InternalLockUpdate()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
            cursorIsLocked = false;
        else if ( Input.GetMouseButtonUp(0) )
            cursorIsLocked = true;

        if (cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!cursorIsLocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

}
