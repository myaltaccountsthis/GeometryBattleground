using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public Animator transition;

    public void ChangeScene(string name, float seconds = 1f) {
        StartCoroutine(SceneTransition(name, seconds));
    }

    private IEnumerator SceneTransition(string sceneName, float seconds) {
        transition.SetTrigger("Start");
        
        yield return new WaitForSeconds(seconds);
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        GameTime.isPaused = false;
    }
}
