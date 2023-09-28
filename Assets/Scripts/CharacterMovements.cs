using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{

    [SerializeField]
    private Camera fpsCamera;
    private float moveSpeed = 5f;
    [SerializeField]
    private int sensitivityX = 250, sensitivityY = -250;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private float jumpForce = 200f;

    private bool isSprinting = false;
    [SerializeField]
    private float moveSpeedSprint = 10f;
    [SerializeField]
    private float moveSpeedBase = 5f;
    private float currentStamina = 100f;
    [SerializeField]
    private float staminaMax = 100f;
    private bool isExhausted = false;
    [SerializeField]
    private float staminaDepletionRate = 20f;
    [SerializeField]
    private float staminaReplenishRateIdle = 25f;
    [SerializeField]
    private float staminaReplenishRateMoving = 15f;
    [SerializeField]
    private float staminaReplenishRate = 20f;

    private Vector3 velocityXY;

    [SerializeField]
    private int grenadeFireRate = 2;
    private float grenadeTimer = 0f;
    [SerializeField]
    private int grenadeMagazineMaxCapacity = 5;
    private int grenadeCurrentAmmo = 5;
    [SerializeField]
    private int grenadeAmmoPerShot = 1;

    [SerializeField]
    private Grenade grenadePrefab;

    [SerializeField]
    private LayerMask layersToIgnore;
    private WeaponSO currentWeapon;
    private WeaponSO.ShootModes currentShootMode;
    private int currentShootModeIndex = 0;

    private float shootTimer = 0f;
    [SerializeField]
    private float currentFireRate;
    [SerializeField]
    private bool isInSemiBurst = false;

    private int bulletsFiredSemiBurst = 0;
    [SerializeField]
    public int currentWeaponAmmo;//public for the HUD

    private float reloadTimer = 0f;
    [SerializeField]
    private bool isReloading = false;

    private Rigidbody hitRigidbodyGG = null;
    private Vector3 targetPosition;
    private float basePullForce = 10f;
    private float pullDistance = 10f;


    //private InGameHud HUD;//Double dépendance pas top


    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.identity;
        fpsCamera.transform.rotation = Quaternion.identity;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if(currentWeapon != null)
        {
            SetCurrentValuesToCurrentWeapon();
        }

        sensitivityX = PlayerPrefs.GetInt("Sensitivity");
        sensitivityY = - PlayerPrefs.GetInt("Sensitivity");

        Time.timeScale = 1f;
    }

    // Update is called once per frame
    void Update()//I want every input inside Update() method.
    {

        MoveCamera();
        RotateCharacter();
        MoveCharacter();
        ActualiseStamina();
        //Debug.Log("stamina = " + stamina);

        grenadeTimer += Time.deltaTime;
        //shootGrenade();//handles input, which is shit architechture

        ReloadAll();

        SwitchShootMode();

        shootTimer += Time.deltaTime;
        ShootWeapon(currentShootMode);


        if (Input.GetMouseButtonDown(1))
        {
            GravityGunGrab();
        }


        if (Input.GetMouseButton(1))
        {
            if(hitRigidbodyGG != null)
            {
                GravityGunPull();
            }
        }

        if (Input.GetMouseButton(2))
        {
            if (hitRigidbodyGG != null)
            {
                GravityGunFusRohDah();
                GravityGunRelease();
            }
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (hitRigidbodyGG != null)
            {
                GravityGunRelease();
            }
        }

    }


    private void GravityGunGrab()
    {
        if (Physics.Raycast(new Ray(transform.position, fpsCamera.transform.forward), out RaycastHit hit, Mathf.Infinity, ~(1 - LayerMask.NameToLayer("Player"))))
        {
            hitRigidbodyGG = hit.rigidbody;
            if (hitRigidbodyGG != null)
            {
                hitRigidbodyGG.useGravity = false;
                pullDistance = (hitRigidbodyGG.transform.position - transform.position).magnitude;
            }
        }
    }

    private void GravityGunPull()
    {
        pullDistance += Input.GetAxis("Mouse ScrollWheel") * 10f;
        pullDistance = Mathf.Clamp(pullDistance, 2f, 30f);

        targetPosition = fpsCamera.transform.forward * pullDistance + fpsCamera.transform.position;
        Vector3 objectToTarget = targetPosition - hitRigidbodyGG.transform.position;
        //Vector3 objectToTargetNormalized = objectToTarget.normalized;
        hitRigidbodyGG.velocity = (objectToTarget * basePullForce);
    }

    private void GravityGunRelease()
    {
        hitRigidbodyGG.useGravity = true;
        hitRigidbodyGG = null;
    }

    private void GravityGunFusRohDah()
    {
        hitRigidbodyGG.AddForce(fpsCamera.transform.forward * 100f, ForceMode.Impulse);
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<WeaponModel>() != null)
        {
            EquipWeapon(other.GetComponent<WeaponModel>());//Voir si possible d'éviter le double .GetComponent<>
            Destroy(other.gameObject);
        }
    }

    private void EquipWeapon(WeaponModel weapon)
    {
        if(weapon != null)
        {
            currentWeapon = weapon.weaponSO;
            Debug.Log("Picked up a " + currentWeapon.weaponName);
            currentWeaponAmmo = weapon.currentAmmo;
            InGameHud.instance.UpdateWeaponAmmo(currentWeaponAmmo);
            SetCurrentValuesToCurrentWeapon();
            InGameHud.instance.UpdateNewWeapon(weapon.weaponSO);
        }
    }

    private void SetCurrentValuesToCurrentWeapon()
    {
        currentShootMode = currentWeapon.modes[0];
        currentFireRate = currentWeapon.fireRate;
    }

    private void ShootWeapon(WeaponSO.ShootModes mode)
    {
        if (!CanShootWeapon())
        { return; }
            switch (mode)
        {
            case WeaponSO.ShootModes.FULL:
                ShootWeaponFULL();
                break;

            case WeaponSO.ShootModes.BURST:
                ShootWeaponBURST();
                break;

            case WeaponSO.ShootModes.SEMI:
                ShootWeaponSEMI();
                break;

            default:
                break;
        }
    }

    private void ShootWeaponFULL()
    {
        if (Input.GetMouseButton(0))
        {
            FireBullet();
        }
    }

    private void ShootWeaponBURST()
    {
        if (Input.GetMouseButtonDown(0))
        {
            FireBullet();
        }
    }

    private void ShootWeaponSEMI()
    {
        if (isInSemiBurst)
        {
            if (bulletsFiredSemiBurst < currentWeapon.numberOfRoundsSemi)
            {
                ShootWeaponInSemiBurst();
            }
            else
            {
                ExitSemiBurst();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                EnterSemiBurst();
                ShootWeaponInSemiBurst();
            }
        }
    }

    private bool CanShootWeapon()
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

    private void EnterSemiBurst()
    {
        if(!isInSemiBurst)
        {
            isInSemiBurst = true;
            currentFireRate = currentWeapon.fireRateDuringSemiBurst;
        }
    }

    private void ShootWeaponInSemiBurst()
    {
        FireBullet();
        ++bulletsFiredSemiBurst;
    }

    private void ExitSemiBurst()
    {
        if(isInSemiBurst)
        {
            isInSemiBurst = false;
            currentFireRate = currentWeapon.fireRate;
            bulletsFiredSemiBurst = 0;
        }
    }

    private void FireBullet()
    {
        Ray ray = new Ray(fpsCamera.transform.position, fpsCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 150f, ~layersToIgnore))
        {
            Mannequin other = hit.collider.GetComponent<Mannequin>();
            if (other != null)
            {
                other.takeDamage((int)currentWeapon.damages);
            }
            Debug.Log(hit.collider.gameObject.name);
        }
        UseWeaponAmmo();
        shootTimer = 0;
    }

    private void UseWeaponAmmo()
    {
        InGameHud.instance.UpdateWeaponAmmo(--currentWeaponAmmo);
    }

    private void SwitchShootMode()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            currentShootModeIndex = ++currentShootModeIndex % currentWeapon.modes.Length;
            currentShootMode = currentWeapon.modes[currentShootModeIndex];
            Debug.Log("Current shoot mode is " + currentShootMode);
        }
        
    }

    private void ShootGrenade()
    {
        if (Input.GetMouseButton(1))
        {
            if (!CanShootGrenade())
            {
                return;
            }

            Grenade grenade = Instantiate(grenadePrefab, transform.position + transform.forward, Quaternion.identity);
            grenade.throwDir = fpsCamera.transform.forward;
            Destroy(grenade.gameObject, 2f);

            grenadeTimer = 0f;

            UseGrenadeAmmo(grenadeAmmoPerShot);
        }
        
    }

    private void ReloadAll()
    {
        if (!isReloading)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                grenadeCurrentAmmo = grenadeMagazineMaxCapacity;
                Debug.Log("Reloaded grenades to max!\n" + "Max grenades is = " + grenadeMagazineMaxCapacity + ", current is " + grenadeCurrentAmmo);
                currentWeaponAmmo = currentWeapon.magazineSize;
                Debug.Log("Reloaded weapon to max!\n" + "Max ammo is = " + currentWeapon.magazineSize + ", current is " + currentWeaponAmmo);
                InGameHud.instance.UpdateWeaponAmmo(currentWeaponAmmo);

                reloadTimer = 0f;
                isReloading = true;

                ExitSemiBurst();//
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

    private bool CanShootGrenade()
    {
        if (isReloading)
        {
            return false;
        }

        return (grenadeTimer > 1f / grenadeFireRate && grenadeCurrentAmmo > 0f);
    }

    private void UseGrenadeAmmo(int howMany = 1)
    {
        grenadeCurrentAmmo -= howMany;
        Mathf.Clamp(grenadeCurrentAmmo, 0, grenadeMagazineMaxCapacity);
        Debug.Log("Current grenade ammo = " + grenadeCurrentAmmo);
    }

    private void MoveCamera()
    {
        fpsCamera.transform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * sensitivityY * Time.deltaTime, 0f, 0f));
    }

    private void RotateCharacter()
    {
        transform.Rotate(new Vector3(0f, Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime, 0f));
    }

    private bool CanSprint()
    {
        if (isExhausted)
        {
            return false;
        }
        
        return currentStamina > 0f;
    }

    private void MoveCharacter()
    {
        Vector3 inputs = Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")), 1f);
        SetSprinting();
        velocityXY = transform.rotation * inputs * moveSpeed;
        rb.velocity = new Vector3(velocityXY.x, rb.velocity.y, velocityXY.z);

        //transform.position += playerMovement.normalized * currentMoveSpeed * Time.deltaTime;        

        if (Input.GetButtonDown("Jump"))
        {
            CharacterJump();
        }
    }
    
    private void SetSprinting()
    {
        isSprinting = CanSprint() && Input.GetKey(KeyCode.LeftShift) ? true : false;
        moveSpeed = isSprinting ? moveSpeedSprint : moveSpeedBase;
    }

    private void CharacterJump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void ActualiseStamina()
    {
        if (isSprinting && velocityXY.magnitude > 0)
        {
            UseStamina();
        }
        else
        {
            ReplenishStamina();
        }

    }

    private void UseStamina()
    {
        currentStamina -= Time.deltaTime * staminaDepletionRate;
        if (currentStamina <= 0)
        {
            isExhausted = true;
        }
        Mathf.Clamp(currentStamina, 0f, staminaMax);
    }

    private void ReplenishStamina()
    {
        staminaReplenishRate = velocityXY.magnitude > 0 ? staminaReplenishRateMoving : staminaReplenishRateIdle;

        if (currentStamina < staminaMax)
        {
            currentStamina += Time.deltaTime * staminaReplenishRate;
        }
        else
        {
            isExhausted = false;
        }
        Mathf.Clamp(currentStamina, 0f, staminaMax);
    }

}

