using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FPController : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 0.1f;
    [SerializeField] private float runSpeed = 30f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bloodSplatter;
    [SerializeField] private GameObject uiBlood;
    [SerializeField] private GameObject canvas;
    [SerializeField] private AudioClip[] takeDamageSounds;
    public GameObject cam;
    public GameObject stevePrefab;
    public Slider healthBar;
    public TextMeshProUGUI ammoTMP;
    public TextMeshProUGUI clipTMP;
    public Animator anim;
    public AudioSource[] footsteps;
    public AudioSource jump;
    public AudioSource land;
    public AudioSource ammoPickupSound;
    public AudioSource triggerSound;
    public AudioSource reloadSound;
    public AudioSource healthPickupSound;
    public AudioSource takeHitSound;
    public AudioSource deathSound;
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

    private int ammo = 50;
    private int maxAmmo = 50;
    private int ammoClip = 10;
    private int ammoClipMax = 10;

    private float health = 100f;
    private float maxHealth = 100f;
    private bool isAlive = true;
    private bool runPressed;

    private float canvasHeight;
    private float canvasWidth;

    private static readonly int ARM = Animator.StringToHash("arm");
    private static readonly int FIRE = Animator.StringToHash("fire");
    private static readonly int RELOAD = Animator.StringToHash("reload");
    private static readonly int WALKING = Animator.StringToHash("walking");
    private static readonly int Death = Animator.StringToHash("Death");
    private static readonly int Dance = Animator.StringToHash("Dance");

    // Start is called before the first frame update
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        cameraRot = cam.transform.localRotation;
        characterRot = transform.localRotation;

        TakeDamage(-100f);
        RemoveAmmoFromClip(0);
        RemoveAmmo(0);

        var canvasRect = canvas.GetComponent<RectTransform>().rect;
        canvasWidth = canvasRect.width;
        canvasHeight = canvasRect.height;
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            anim.SetBool(ARM, !anim.GetBool(ARM));

        runPressed = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetMouseButtonDown(0) 
            && GameStats.canShoot 
            && anim.GetBool(ARM))
        {
            if (ammoClip > 0)
            {
                ShootHandler();
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

    public void TakeHit(float amount)
    {
        PlayTakeHitSound();
        HandleBloodSplatter();
        TakeDamage(amount);
    }

    private void HandleBloodSplatter()
    {
        var instantiatedBlood = Instantiate(uiBlood, canvas.transform, true);
        var offset = 50;
        var randomX = Random.Range(0 + offset, canvasWidth - offset);
        var randomY = Random.Range(0 + offset, canvasHeight - offset);
        // Debug.Log($"New blood splatter coords: x: {randomX}, y: {randomY}");
        instantiatedBlood.transform.position = new Vector3(randomX, randomY, 0f);
        // instantiatedBlood.GetComponent<RectTransform>().SetPositionAndRotation(new Vector3(randomX,randomY, 0f), Quaternion.identity);
        Destroy(instantiatedBlood, 2.2f);
    }

    private void TakeDamage(float amount)
    {
        health = Mathf.Clamp(health - amount, 0, maxHealth);
        healthBar.value = Mathf.RoundToInt(health);
        
        if (health <= 0)
        {
            Vector3 pos = new Vector3(transform.position.x,
                Terrain.activeTerrain.SampleHeight(transform.position),
                transform.position.z);

            GameObject steve = Instantiate(stevePrefab, pos, transform.rotation);
            steve.GetComponent<Animator>().SetTrigger(Death);
            GameStats.gameOver = true;
            Debug.Log($"Health after attack: {health:n2}");
            Destroy(gameObject);
        }
    }

    private void RemoveAmmoFromClip(int amount)
    {
        ammoClip = Mathf.Clamp(ammoClip - amount, 0, ammoClipMax);
        clipTMP.text = ammoClip.ToString();
    }

    private void RemoveAmmo(int amount)
    {
        ammo = Mathf.Clamp(ammo - amount, 0, maxAmmo);
        ammoTMP.text = ammo.ToString();
    }

    private void PlayTakeHitSound()
    {
        takeHitSound.clip = takeDamageSounds[Random.Range(0, takeDamageSounds.Length)];
        takeHitSound.Play();
    }

    private void ShootHandler()
    {
        GameStats.canShoot = false;
        anim.SetTrigger(FIRE);
        RemoveAmmoFromClip(1);
        // Debug.Log($"Remaining ammo in a clip: {ammoClip}, spare ammo: {ammo}");

        if (Physics.Raycast(firePoint.position, firePoint.forward, out var hitInfo, 200))
        {
            GameObject hitZombie = hitInfo.collider.gameObject;
            if (hitZombie.CompareTag("Zombie"))
            {
                GameObject blood = Instantiate(bloodSplatter, hitInfo.point, Quaternion.identity);
                blood.transform.LookAt(transform.position);
                Destroy(blood, 0.1f);
                hitZombie.GetComponent<ZombieController>().DeathHandler();
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
        playingWalking = true;
    }

    private void ReloadHandler()
    {
        var reloadedAmount = Mathf.Min(ammo, ammoClipMax - ammoClip);
        RemoveAmmo(reloadedAmount);
        RemoveAmmoFromClip(-reloadedAmount);
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

            var speed = (runPressed ? runSpeed : walkSpeed) * Time.deltaTime;
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
            if (!anim.GetBool(WALKING) || playingWalking) return;
            
            InvokeRepeating(nameof(PlayFootStepAudio), 0, 0.4f);
            playingWalking = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var second = other.gameObject;
        if (second.CompareTag("Ammo") && ammo < maxAmmo)
        {
            RemoveAmmo(-10);
            Debug.Log($"Ammo picked up: {ammo}");
            ammoPickupSound.Play();
            Destroy(second);
        }
        else if (second.CompareTag("Medikit") && health < maxHealth)
        {
            TakeDamage(-10f);
            Debug.Log($"Medikit picked up: {health:n2}");
            healthPickupSound.Play();
            Destroy(second);
        }
        else if (second.CompareTag("Home"))
        {
            Vector3 pos = new Vector3(transform.position.x,
                Terrain.activeTerrain.SampleHeight(transform.position),
                transform.position.z);

            GameObject steve = Instantiate(stevePrefab, pos, transform.rotation);
            steve.GetComponent<Animator>().SetTrigger(Dance);
            GameStats.gameOver = true;
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        var second = other.gameObject;
        
        if (isAlive && second.CompareTag("Lava"))
        {
            TakeDamage(10 * Time.deltaTime);
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
        else if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
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