using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponModel : MonoBehaviour
{
    public WeaponSO weaponSO;

    public int currentAmmo;
    
    [SerializeField]
    public Rigidbody rb;

    private void Start()
    {
        currentAmmo = weaponSO.magazineSize;
    }

}
