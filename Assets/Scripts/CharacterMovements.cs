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
    private bool isExhausted = false;
    [SerializeField]
    private float staminaDepletionRate = 20f;
    [SerializeField]
    private float staminaReplenishRateIdle = 25f;
    [SerializeField]
    private float staminaReplenishRateMoving = 15f;
    [SerializeField]
    private float staminaReplenishRate = 20f;

    [SerializeField]
    private int grenadeFireRate = 2;
    [SerializeField]
    private float grenadeTimer = 0f;
    [SerializeField]
    private int grenadeMagazineMaxCapacity = 5;
    [SerializeField]
    private int grenadeCurrentAmmo = 5;
    [SerializeField]
    private int grenadeAmmoPerShot = 1;

    [SerializeField]
    private Grenade grenadePrefab;

    [SerializeField]
    private LayerMask layersToIgnore;
    [SerializeField]
    private WeaponSO currentWeapon;
    [SerializeField]
    private WeaponSO.ShootModes currentShootMode;
    [SerializeField]
    private int currentShootModeIndex = 0;

    [SerializeField]
    private float velocity;

    [SerializeField]
    private float shootTimer = 0f;
    [SerializeField]
    private float currentFireRate;
    [SerializeField]
    private bool isInSemiBurst = false;
    [SerializeField]
    private int bulletsFiredSemiBurst = 0;
    [SerializeField]
    private int currentWeaponAmmo;
    [SerializeField]
    private float reloadTimer = 0f;
    [SerializeField]
    private bool isReloading = false;


    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.identity;
        fpsCamera.transform.rotation = Quaternion.identity;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if(currentWeapon != null)
        {
            setCurrentValuesToCurrentWeapon();
        }
    }

    // Update is called once per frame
    void Update()
    {
        moveCamera();
        moveCharacter();
        actualiseStamina();
        //Debug.Log("stamina = " + stamina);

        if(canShootGrenade())
        {
            shootGrenade();//handles input, which is shit
        }
        grenadeTimer += Time.deltaTime;
        reloadAll();

        switchShootMode();
        shootTimer += Time.deltaTime;
        if (canShootWeapon())
        {
            shootWeapon(currentShootMode);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<WeaponModel>() != null)
        {
            currentWeapon = other.GetComponent<WeaponModel>().weaponSO;
            Destroy(other.gameObject);
            Debug.Log("Picked up a " + currentWeapon.weaponName);
            setCurrentValuesToCurrentWeapon();
        }
    }

    private void setCurrentValuesToCurrentWeapon()
    {
        currentWeaponAmmo = currentWeapon.magazineSize;
        currentShootMode = currentWeapon.modes[0];
        currentFireRate = currentWeapon.fireRate;
    }


    private void shootWeapon(WeaponSO.ShootModes mode)
    {
        switch (mode)
        {
            case WeaponSO.ShootModes.FULL:
                shootWeaponFULL();
                break;

            case WeaponSO.ShootModes.BURST:
                shootWeaponBURST();
                break;

            case WeaponSO.ShootModes.SEMI:
                shootWeaponSEMI();
                break;

            default:
                break;
        }
    }

    private void shootWeaponFULL()
    {
        if (Input.GetMouseButton(0))
        {
            fireBullet();
        }
    }

    private void shootWeaponBURST()
    {
        if (Input.GetMouseButtonDown(0))
        {
            fireBullet();
        }
    }

    private void shootWeaponSEMI()
    {
        if (isInSemiBurst)
        {
            if (bulletsFiredSemiBurst < currentWeapon.numberOfRoundsSemi)
            {
                shootWeaponInSemiBurst();
            }
            else
            {
                exitSemiBurst();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                enterSemiBurst();
                shootWeaponInSemiBurst();
            }
        }
    }


    private bool canShootWeapon()
    {
        if (shootTimer < 1f / currentFireRate)
        {
            return false;
        }
        if (isReloading)
        {
            return false;
        }

        return currentWeaponAmmo > 0;
    }

    private void enterSemiBurst()
    {
        if(!isInSemiBurst)
        {
            isInSemiBurst = true;
            currentFireRate = currentWeapon.fireRateDuringSemiBurst;
        }
    }

    private void shootWeaponInSemiBurst()
    {
        fireBullet();
        ++bulletsFiredSemiBurst;
    }

    private void exitSemiBurst()
    {
        if(isInSemiBurst)
        {
            isInSemiBurst = false;
            currentFireRate = currentWeapon.fireRate;
            bulletsFiredSemiBurst = 0;
        }
    }

    private void fireBullet()
    {
        Ray ray = new Ray(fpsCamera.transform.position, fpsCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 150f, ~layersToIgnore))
        {
            Debug.Log(hit.collider.gameObject.name);
        }
        useWeaponAmmo();
        shootTimer = 0;
    }

    private void useWeaponAmmo()
    {
        --currentWeaponAmmo;
    }

    private void switchShootMode()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            currentShootModeIndex = ++currentShootModeIndex % currentWeapon.modes.Length;
            currentShootMode = currentWeapon.modes[currentShootModeIndex];
            Debug.Log("Current shoot mode is " + currentShootMode);
        }
        
    }

    private void shootGrenade()
    {
        if (Input.GetMouseButton(1))
        {
            if (!canShootGrenade())
            {
                return;
            }

            Grenade grenade = Instantiate(grenadePrefab, transform.position + transform.forward, Quaternion.identity);
            grenade.throwDir = fpsCamera.transform.forward;
            Destroy(grenade.gameObject, 2f);

            grenadeTimer = 0f;

            useGrenadeAmmo(grenadeAmmoPerShot);
        }
        
    }

    private void reloadAll()//A Refactorer
    {
        if (!isReloading)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                grenadeCurrentAmmo = grenadeMagazineMaxCapacity;
                Debug.Log("Reloaded grenades to max!\n" + "Max grenades is = " + grenadeMagazineMaxCapacity + ", current is " + grenadeCurrentAmmo);
                currentWeaponAmmo = currentWeapon.magazineSize;
                Debug.Log("Reloaded weapon to max!\n" + "Max ammo is = " + currentWeapon.magazineSize + ", current is " + currentWeaponAmmo);

                reloadTimer = 0f;
                isReloading = true;

                exitSemiBurst();//
            }
        }
        else
        {
            reloadTimer += Time.deltaTime;
            if (reloadTimer > currentWeapon.reloadTime)//done reloading
            {
                isReloading = false;
            }
        }
    }

    private bool canShootGrenade()
    {
        if (isReloading)
        {
            return false;
        }

        return (grenadeTimer > 1f / grenadeFireRate && grenadeCurrentAmmo > 0f);
    }

    private void useGrenadeAmmo(int howMany = 1)
    {
        grenadeCurrentAmmo -= howMany;
        Mathf.Clamp(grenadeCurrentAmmo, 0, grenadeMagazineMaxCapacity);
        Debug.Log("Current grenade ammo = " + grenadeCurrentAmmo);
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
        if (isExhausted)
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
            isExhausted = true;
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
            isExhausted = false;
        }
        Mathf.Clamp(stamina, 0f, staminaMax);
    }

}

