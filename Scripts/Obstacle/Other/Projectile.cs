using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float velocity;
    [SerializeField] private float lifeTime;

    private Transform player;

    void Start()
    {
        player = FindObjectOfType<PlayerController>().transform;

        StartCoroutine(LifeCounter());
    }

    void FixedUpdate()
    {
        transform.LookAt(player, Vector3.right);

        Vector3 moveStep = transform.forward * velocity * Time.fixedDeltaTime;
        transform.position += moveStep;
    }

    /// <summary>
    /// Destroys the projectile when it ends its life time.
    /// </summary>
    private IEnumerator LifeCounter()
    {
        yield return new WaitForSeconds(lifeTime);

        Destroy(gameObject);
    }

    // Invoked method: Projectile/Module/OnTriggerEvent
    /// <summary>
    /// Callbacks when the projectile hits something.
    /// </summary>
    /// <param name="other">The collider that was hit by the projectile.</param>
    public void OnTriggeredEnter(Collider2D other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("Shield")) return;

        Destroy(gameObject);
    }
}
