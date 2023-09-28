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
            SpawnRandomWeapon();
        }
    }
    private void Update()
    {
        if (!currentlySpawnedWeapon)
        {
            SpawnRandomWeapon();
        }
    }

    private void SpawnRandomWeapon()
    {
        int index = Random.Range(0, weapons.Length);
        SpawnWeapon(index);
    }

    private void SpawnWeapon(int indexToSpawn)
    {
        if (!currentlySpawnedWeapon)
        {
            currentlySpawnedWeapon = Instantiate(weapons[indexToSpawn], transform.position + Vector3.up * 2, Quaternion.identity);
            currentlySpawnedWeapon.rb.AddForce(Random.insideUnitSphere * 10, ForceMode.Impulse);
            Debug.Log("Spawned a " + currentlySpawnedWeapon.weaponSO.weaponName);
        }
    }

}


