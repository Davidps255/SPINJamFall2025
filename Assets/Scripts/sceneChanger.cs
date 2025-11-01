using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class sceneChanger : MonoBehaviour
{
    public string loadscene;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) {
            SceneManager.LoadScene(loadscene); }
    }
}
