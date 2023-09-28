using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameHud : MonoBehaviour
{
    public static InGameHud instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField]
    private Image weaponImage;

    [SerializeField]
    private WeaponSO currentlyEquipedWeapon;

    [SerializeField]
    private TMP_Text currentWeaponName;
    [SerializeField]
    private TMP_Text currentWeaponAmmo;
    [SerializeField]
    private TMP_Text currentWeaponMaxAmmo;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateNewWeapon(WeaponSO so)
    {
        currentlyEquipedWeapon = so;//passage par ref donc ça pointe le même GameObject
        UpdateWeaponName();
        //UpdateWeaponSprite();
        UpdateWeaponMaxAmmo();
    }

    private void UpdateWeaponName()
    {
        currentWeaponName.text = currentlyEquipedWeapon.weaponName;
    }

    private void UpdateWeaponSprite()
    {
        weaponImage.sprite = currentlyEquipedWeapon.sprite;
    }

    public void UpdateWeaponAmmo(in int ammo)
    {
        currentWeaponAmmo.text = ammo.ToString();
    }

    public void UpdateWeaponMaxAmmo()
    {
        currentWeaponMaxAmmo.text = "/ " + currentlyEquipedWeapon.magazineSize.ToString();
    }

}
