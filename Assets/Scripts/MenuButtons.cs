using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    public void Restart()
    {
        SceneManager.LoadScene(0);
    }
}
