using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mannequin : MonoBehaviour
{
    [SerializeField]
    private int maxHealth = 50;

    private int currentHealth = 50;

    [SerializeField]
    private Mannequin mannekinPrefab;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(currentHealth <= 0)
        {
            Die();
        }
    }

    public void takeDamage(int damage)
    {
        currentHealth -= damage;
    }

    public void Die()
    {
        Instantiate(mannekinPrefab, transform.position + Vector3.up, Quaternion.identity);
        Destroy(this.gameObject);//Moyen cool comme truc
    }


}
