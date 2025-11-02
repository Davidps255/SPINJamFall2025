using System.Collections;
using Farmer;
using UnityEngine;
using UnityEngine.AI;

public class EnemyRoamingMesh : MonoBehaviour
{
    [Header("References")]
    public GameObject player;
    private NavMeshAgent agent;

    [Header("Detection Settings")]
    public float proximityRange = 8f;
    public float sightRange = 8f;
    public float sightMultiplier = 2f;
    public float fieldOfView = 90f;
    public float heightOffset = 1.5f;
    public float killDistance = 1.2f;
    public float corruptionDMG = 20f;
    public bool roamer = true;

    [Header("Roaming Settings")]
    public float roamRadius = 10f;
    public float roamDelay = 3f;

    private bool isChasing = false;
    private float roamTimer;
    private AudioSource audioSource;



    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        PickNewRoamDestination();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        // --- DETECTION ---
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
            agent.SetDestination(player.transform.position);
        }
        else if (isChasing && distance > sightRange)
        {
            // Lost player
            isChasing = false;
            roamTimer = 0f;
            if (audioSource != null && audioSource.isPlaying)
            {
                StartCoroutine(FadeOut(audioSource, 5));
            }
        }

        // --- ROAMING ---
        if (!isChasing && roamer)
        {
            roamTimer += Time.deltaTime;
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                if (roamTimer >= roamDelay)
                {
                    PickNewRoamDestination();
                    roamTimer = 0f;
                }
            }
        }

        // --- KILL CHECK ---
        if (distance <= killDistance)
        {
            KillPlayer();
        }
    }

    bool CanSeePlayer(float range)
    {
        Vector3 directionToPlayer = player.transform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle < fieldOfView * 0.5f)
        {
            RaycastHit hit;
            Vector3 eyePosition = transform.position + Vector3.up * heightOffset;

            if (Physics.Raycast(eyePosition, directionToPlayer.normalized, out hit, range))
            {
                if (hit.transform.CompareTag("Player"))
                    return true;
            }
        }
        return false;
    }

    void PickNewRoamDestination()
    {
        if (roamer)
        {
            Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
            randomDirection += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
    }

    void KillPlayer()
    {
        player.GetComponent<FPController>().Corruption += corruptionDMG;
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, proximityRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, proximityRange * sightMultiplier);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, roamRadius);
    }

    public IEnumerator FadeOut(AudioSource audioSource, float fadeTime) {
        float startVolume = audioSource.volume;
        while (audioSource.volume > 0) {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }
}
