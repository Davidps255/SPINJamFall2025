using UnityEngine;

public class EnemyProximityChase : MonoBehaviour
{
    public Transform player;
    public float chaseRange = 10f;
    public float speed = 4f;
    public float killDistance = 1.2f;

    void Update()
    {
        if (player == null) return;

        // Measure distance to player
        float distance = Vector3.Distance(transform.position, player.position);

        // If within chase range, move toward player
        if (distance <= chaseRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // stay on the ground
            transform.position += direction * speed * Time.deltaTime;

            // Rotate to face player
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        }

        // Kill player if close enough
        if (distance <= killDistance)
        {
            KillPlayer();
        }
    }

    void KillPlayer()
    {
        Debug.Log("Player caught!");
        Destroy(player.gameObject); // You can replace this later with respawn logic
    }

    // Optional: Draw the chase range in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
