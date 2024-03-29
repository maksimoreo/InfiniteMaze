using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public float transitionTime = 1;

    public void ReloadScene()
    {
        StartCoroutine(ReloadSceneCoroutine());
    }

    private IEnumerator ReloadSceneCoroutine()
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
