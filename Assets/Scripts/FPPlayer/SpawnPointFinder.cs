using UnityEngine;
using UnityEngine.SceneManagement;
public class SpawnPointFinder : MonoBehaviour
{
    [SerializeField] private Transform player;
    private GameObject spawnPoint;
    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
            spawnPoint = GameObject.Find("Spawnpoint");
            player.SetPositionAndRotation(spawnPoint.transform.position, player.transform.rotation);
            Physics.SyncTransforms();

    }
    
}
