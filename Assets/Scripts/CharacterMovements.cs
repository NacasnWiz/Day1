using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{

    [SerializeField]
    private Camera fpsCamera;
    [SerializeField]
    private float moveSpeed = 5f;
    [SerializeField]
    private float sensitivityX = 20f, sensitivityY = 20f;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private float jumpForce = 200f;

    [SerializeField]
    private bool isSprinting = false;
    [SerializeField]
    private float moveSpeedSprint = 10f;
    [SerializeField]
    private float moveSpeedBase = 5f;
    [SerializeField]
    private float stamina = 100f;
    [SerializeField]
    private float staminaMax = 100f;
    [SerializeField]
    private bool exhausted = false;
    [SerializeField]
    private float staminaDepletionRate = 20f;
    [SerializeField]
    private float staminaReplenishRateIdle = 25f;
    [SerializeField]
    private float staminaReplenishRateMoving = 15f;
    [SerializeField]
    private float staminaReplenishRate = 20f;

    [SerializeField]
    private int fireRate = 2;
    [SerializeField]
    private float timeAfterLastShot = 0f;
    [SerializeField]
    private int magazineMaxCapacity = 5;
    [SerializeField]
    private int magazineCurrentAmmo = 5;
    [SerializeField]
    private int ammoPerShot = 1;

    [SerializeField]
    private Grenade grenadePrefab;
    
    private bool wantSprint;
    private float velocity;


    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.identity;
        fpsCamera.transform.rotation = Quaternion.identity;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        moveCamera();
        moveCharacter();
        actualiseStamina();
        //Debug.Log("stamina = " + stamina);


        if (Input.GetMouseButton(0))
        {
            fireGrenade();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            reloadAmmo();
        }

        timeAfterLastShot += Time.deltaTime;

    }

    private void fireGrenade()
    {
        if (!canShoot())
        {
            return;
        }

        Grenade grenade = Instantiate(grenadePrefab, transform.position + transform.forward, Quaternion.identity);
        grenade.throwDir = fpsCamera.transform.forward;
        Destroy(grenade.gameObject, 2f);

        timeAfterLastShot = 0f;

        useAmmo(ammoPerShot);
    }

    private void reloadAmmo()
    {
        magazineCurrentAmmo = magazineMaxCapacity;
        Debug.Log("Reloaded to max ammo!\n" + "Max ammo is = " + magazineMaxCapacity + ", current ammo is " + magazineCurrentAmmo);
    }

    private bool canShoot()
    {
        return (timeAfterLastShot > (float)1 / fireRate && magazineCurrentAmmo > 0);
    }

    private void useAmmo(int howMany = 1)
    {
        magazineCurrentAmmo -= howMany;
        Mathf.Clamp(magazineCurrentAmmo, 0, magazineMaxCapacity);
        Debug.Log("Current ammo = " + magazineCurrentAmmo);
    }

    private void moveCamera()
    {
        rotateCharacter();
        fpsCamera.transform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * sensitivityY * Time.deltaTime, 0f, 0f));
    }

    private void rotateCharacter()
    {
        transform.Rotate(new Vector3(0f, Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime, 0f));
    }

    private bool canSprint()
    {
        if (exhausted)
        {
            return false;
        }
        
        return stamina > 0f;
    }

    private void moveCharacter()
    {
        Vector3 playerMovement = transform.forward * Input.GetAxisRaw("Vertical") + transform.right * Input.GetAxisRaw("Horizontal");
        velocity = playerMovement.magnitude;

        setSprinting();

        transform.position += playerMovement.normalized * moveSpeed * Time.deltaTime;        
        
        if (Input.GetButtonDown("Jump"))
        {
            characterJump();
        }
    }
    
    private void setSprinting()
    {
        isSprinting = canSprint() && Input.GetKey(KeyCode.LeftShift) ? true : false;
        moveSpeed = isSprinting ? moveSpeedSprint : moveSpeedBase;
    }

    private void characterJump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void actualiseStamina()
    {
        if (isSprinting && velocity > 0)
        {
            useStamina();
        }
        else
        {
            replenishStamina();
        }

    }

    private void useStamina()
    {
        stamina -= Time.deltaTime * staminaDepletionRate;
        if (stamina <= 0)
        {
            exhausted = true;
        }
        Mathf.Clamp(stamina, 0f, staminaMax);
    }

    private void replenishStamina()
    {
        staminaReplenishRate = velocity > 0 ? staminaReplenishRateMoving : staminaReplenishRateIdle;

        if (stamina < staminaMax)
        {
            stamina += Time.deltaTime * staminaReplenishRate;
        }
        else
        {
            exhausted = false;
        }
        Mathf.Clamp(stamina, 0f, staminaMax);
    }

}

