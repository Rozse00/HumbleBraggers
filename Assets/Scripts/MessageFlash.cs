using UnityEngine;
using TMPro;
using UnityEngine.UI;

//turns on an text-image pair, used as an alert; currently only used in the rebinding process
public class MessageFlash : MonoBehaviour
{
    //references to the text and accomponying image
    public TMP_Text dupeText;
    private Image dupeImage;
    private Color imgColor;

    public float holdTime; //time to stay at 100% opacity
    public float fadeTime; //time from 100% to 0% opacity
    private float timeLeft; //current t

    void Start()
    {
        dupeImage = GetComponent<Image>();
        imgColor = dupeImage.color;

        dupeImage.color = new Color(imgColor.r, imgColor.g, imgColor.b, 0);
        dupeText.color = new Color(1f, 1f, 1f, 0);

        timeLeft = 0;
    }

    //Initiates the message
    public void Flash()
    {
        timeLeft = holdTime + fadeTime;
        Debug.Log("flash start: " + timeLeft);

        dupeImage.color = imgColor;
        dupeText.color = Color.white;
    }

    //clears/cancels the sequence
    public void Reset() { timeLeft = 0.001f; }

    void Update()
    {
        if (timeLeft > 0)
        {
            //Debug.Log("time left: " + timeLeft);
            if (timeLeft <= fadeTime)
            {
                float A = timeLeft / fadeTime;
                dupeImage.color = new Color(imgColor.r, imgColor.g, imgColor.b, A);
                dupeText.color = new Color(1f, 1f, 1f, A);
            }

            timeLeft -= Time.deltaTime;
        }
    }
}
