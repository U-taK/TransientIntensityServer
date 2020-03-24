using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IPportGetter : MonoBehaviour {

    [SerializeField]
    Text samplerate;
    [SerializeField]
    Text sampleLength;

    public Scene scene;
    public static string IP;
    public static string SampleRate;
    public static string SampleLength;
	// Use this for initialization
	void Start () {
        DontDestroyOnLoad(this);
        scene = SceneManager.GetActiveScene();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    public void Reload()
    {
        SampleRate = samplerate.text;
        SampleLength = sampleLength.text;

        SceneManager.LoadScene("Main");
        //SceneManager.LoadScene("SharingMeasurement");
    }
}
