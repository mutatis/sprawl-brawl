using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TempRevive : MonoBehaviour
{
    public void GameOver()
    {
        SceneManager.LoadScene("GameOver");
    }
}
