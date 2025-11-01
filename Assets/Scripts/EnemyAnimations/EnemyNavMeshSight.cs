using UnityEngine;
using UnityEngine.AI;

public class EnemyNavMeshSight : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    private NavMeshAgent agent;

    [Header("Detection Settings")]
    public float proximityRange = 8f;
    public float sightMultiplier = 2f;
    public float fieldOfView = 90f; // degrees
    public float heightOffset = 1.5f; // "eye" height
    public float killDistance = 1.2f;

    private bool isChasing = false;
    private AudioSource audioSource;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        float sightRange = proximityRange * sightMultiplier;

        // If player is within proximity OR visible in sight
        if (distance <= proximityRange || CanSeePlayer(sightRange))
        {
            //isChasing = true;
            if (!isChasing)
            {
                isChasing = true;
                if (audioSource != null && !audioSource.isPlaying)
                {
                    audioSource.Play();
                }
            }
            agent.SetDestination(player.position);
        }
        else if (isChasing && distance > sightRange)
        {
            // Lost player beyond sight range
            isChasing = false;
            agent.ResetPath();
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        // Kill if close enough
        if (distance <= killDistance)
        {
            KillPlayer();
        }
    }

    bool CanSeePlayer(float range)
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle < fieldOfView * 0.5f)
        {
            RaycastHit hit;
            Vector3 eyePosition = transform.position + Vector3.up * heightOffset;

            if (Physics.Raycast(eyePosition, directionToPlayer.normalized, out hit, range))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    void KillPlayer()
    {
        Debug.Log("Player caught! (Kill logic to be added later)");
        // We'll implement kill logic next step.
    }

    void OnDrawGizmosSelected()
    {
        // Visualize proximity and sight range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, proximityRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, proximityRange * sightMultiplier);
    }
}
