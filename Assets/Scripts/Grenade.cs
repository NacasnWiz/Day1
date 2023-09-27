using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField]
    private float throwForce;
    [SerializeField]
    private Rigidbody rb;

    public Vector3 throwDir;

    [Header ("Explosion")]
    [SerializeField]
    private float explosionRadius;
    [SerializeField]
    private float explosionForce;

    [SerializeField]
    private LayerMask layersToIgnore;



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
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, ~layersToIgnore);
        //On peut aussi mettre '~8' à la place de '~(1 << LayerMask.NameToLayer("Player"))'

        foreach (Collider collider in colliders)
        {
            Rigidbody other = collider.GetComponent<Rigidbody>();
            if (other)
            {
                other.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
        }
    }

}
