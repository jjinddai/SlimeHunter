using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    [SerializeField] private Text pressText;
    [SerializeField] private float fadingInterval;
    private float elapsedTime;
    private bool isFadeIn = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (isFadeIn)
        {
            float alpha = Mathf.Lerp(0.0f, 1.0f, elapsedTime / fadingInterval);
            SetTextAlpha(alpha);

            if (elapsedTime > fadingInterval)
            {
                isFadeIn = false;
                elapsedTime = 0.0f;
            }
        }
    }

    private void SetTextAlpha(float alpha)
    {
        Color color = pressText.color;
        color.a = alpha;
        pressText.color = color;
    }
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Main");
    }
}
