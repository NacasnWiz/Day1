using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "ScriptableObects/Weapon")]
public class WeaponSO : ScriptableObject
{
    public string weaponName;
    public enum ShootModes
    {
        SEMI,
        FULL,
        BURST
    }

    public ShootModes[] modes;

    public int magazineSize;
    public float fireRate;
    public float reloadTime;
    public float damages;
    public float fireRateDuringSemiBurst;
    public int numberOfRoundsSemi;
    public float burstTimer;

}
