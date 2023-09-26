using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField]
    private float throwForce;

    public Vector3 throwDir;

    [SerializeField]
    private float explosionRadius;
    [SerializeField]
    private float explosionForce;
    [SerializeField]
    private Rigidbody rb;


    // Start is called before the first frame update
    void Start()
    {
        rb.AddForce(throwDir * throwForce, ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, -1);
        foreach(Collider collider in colliders)
        {
            Rigidbody other = collider.GetComponent<Rigidbody>();
            if (other)
            {
                other.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
    }

}
