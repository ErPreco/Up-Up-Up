using UnityEngine;

public class CoinScript : MonoBehaviour
{
    [SerializeField] private GameObject coinValuePrefab;

    /// <summary>
    /// Gets the PlayerController component searching through the parents of the collided object.
    /// </summary>
    /// <param name="transform">The transform of the collided object.</param>
    /// <returns>The PlayerController component of the player root.</returns>
    private PlayerController GetPlayerController(Transform transform)
    {
        PlayerController controller = transform.GetComponent<PlayerController>();
        if (controller == null)
        {
            return GetPlayerController(transform.parent);
        }

        return controller;
    }

    // Invoked method: Coin/Collider/OnTriggerEvent
    /// <summary>
    /// Callbacks when the player passes through the coin.
    /// </summary>
    /// <param name="other">The collider that has passed through the coin.</param>
    public void OnTriggeredEnter(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController controller = GetPlayerController(other.transform);
        controller.CollectCoin();

        // TODO: activate animation
        Destroy(transform.GetChild(0).gameObject);
        Destroy(Instantiate(coinValuePrefab, other.transform.position, Quaternion.identity), 1);
    }
}

public class Coin
{
    public GameObject Prefab { get; set; }
    public GameObject Object { get; set; }

    public Vector3 Position { get; set; }

    public bool IsDestroyed { get; set; }

    public Coin(Vector3 position)
    {
        Position = position;
    }
}