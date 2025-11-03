using UnityEngine;
using UnityEngine.SceneManagement;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private void Start() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void GoToScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitApp() {
        Application.Quit();
    }
}
