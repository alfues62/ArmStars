using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Método público para cargar la primera escena
    public void changeMainMenu()
    {
        SceneManager.LoadScene("mainMenu");
    }

    // Método público para cargar la segunda escena (ejemplo)
    public void changeArmMovement()
    {
        SceneManager.LoadScene("armMovement");
    }

    public void chargeVideoMenu ()
    {
        SceneManager.LoadScene("videoMenu");
    }
    public void exitApp()
    {
               Application.Quit();
    }
}
