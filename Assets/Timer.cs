using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField]TextMeshProUGUI timerText;
    [SerializeField]private float elapsedTime, remainingTime=20f;
    public static bool startTimer, moveDrone;
    private bool timerStopped = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timerStopped) return;

        if (remainingTime>0){
            remainingTime-=Time.deltaTime;
            startTimer=false;
            int remainingMins = Mathf.FloorToInt(remainingTime/60f);
            int remainingSecs = Mathf.FloorToInt(remainingTime%60f);
            timerText.text = string.Format("{0:00}:{1:00}", remainingMins, remainingSecs);
        }
        else{
            remainingTime=0;
            startTimer=true;
            timerText.color=Color.red;
        }
        if(startTimer){
            moveDrone=true;
            elapsedTime += Time.deltaTime;
            int minutes = Mathf.FloorToInt(elapsedTime/60f);
            int seconds = Mathf.FloorToInt(elapsedTime%60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    public void StopTimer()
    {
        timerStopped = true;
        moveDrone = false;
        startTimer = false;
        timerText.color = Color.green; // change color to show finish
        Debug.Log("Timer stopped at finish!");
    }
}
