using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{

	// Use this for initialization
	IEnumerator Start ()
    {
        AudioSource sorc = GetComponent<AudioSource>();

        sorc.Play();
        Debug.Log("Racing Day");
        yield return new WaitForSeconds(sorc.clip.length);

        StartCoroutine(LoadLevel());
	}
	

    IEnumerator LoadLevel()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("Load Level 1");
        SceneManager.LoadScene("Level 1");
    }
}
