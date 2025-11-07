using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // M�todo p�blico para cargar la primera escena
    public void changeMainMenu()
    {
        SceneManager.LoadScene("mainMenu");
    }

    // M�todo p�blico para cargar la segunda escena (ejemplo)
    public void changeArmMovement()
    {
        SceneManager.LoadScene("armInspection");
    }

    public void chargeVideoMenu ()
    {
        SceneManager.LoadScene("videoMenu");
    }

    public void chargeInstructionsMenu ()
    {
        SceneManager.LoadScene("IntstructionsScene");
    }
    public void exitApp()
    {
               Application.Quit();
    }
}
