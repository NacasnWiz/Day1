using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [SerializeField]
    private WeaponModel[] weapons;
    [SerializeField]
    WeaponModel currentlySpawnedWeapon = null;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<CharacterMovement>() != null)
        {
            spawnRandomWeapon();
        }
    }
    private void Update()
    {
        if (!currentlySpawnedWeapon)
        {
            spawnRandomWeapon();
        }
    }

    private void spawnRandomWeapon()
    {
        int index = Random.Range(0, weapons.Length);
        spawnWeapon(index);
    }

    private void spawnWeapon(int indexToSpawn)
    {
        if (!currentlySpawnedWeapon)
        {
            currentlySpawnedWeapon = Instantiate(weapons[indexToSpawn], transform.position + Vector3.up * 2, Quaternion.identity);
            currentlySpawnedWeapon.rb.AddForce(Random.insideUnitSphere * 10, ForceMode.Impulse);
            Debug.Log("Spawned a " + currentlySpawnedWeapon.weaponSO.weaponName);
        }
    }

}


