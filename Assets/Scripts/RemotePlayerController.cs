using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayerController : MonoBehaviour
{
    [SerializeField] private GameObject defaultModel;
    [SerializeField] private GameObject ragdoll;
    public void Die(Vector3 hitPosition, Vector3 hitDirection)
    {
        if (defaultModel == null)
        {
            return;
        }

        this.GetComponent<CapsuleCollider>().enabled = false;
        var rd = Instantiate(ragdoll, defaultModel.transform.position, defaultModel.transform.rotation);
        var rigidbodies = rd.GetComponentsInChildren<Rigidbody>();
        var closest = rigidbodies.Length > 0 ? rigidbodies[0] : null;
        foreach (var rb in rigidbodies)
        {
            if (Vector3.Distance(closest.transform.position, hitPosition) >
                Vector3.Distance(rb.transform.position, hitPosition))
            {
                closest = rb;
            }
        }
        
        Destroy(defaultModel);

        if (closest != null)
        {
            closest.AddForce(hitDirection * 100, ForceMode.VelocityChange);
        }

        StartCoroutine(DestroyAfter3s());
    }

    private IEnumerator DestroyAfter3s()
    {
        yield return new WaitForSeconds(3);
        Destroy(this.gameObject);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
