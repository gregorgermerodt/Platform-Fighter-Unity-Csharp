using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PercentageUI : MonoBehaviour
{
    private TMP_Text redPercentText;
    private TMP_Text greenPercentText;

    private TMP_Text redScoreText;
    private TMP_Text greenScoreText;

    [SerializeField] public Fighter redFighter;
    [SerializeField] public Fighter greenFighter;

    // Start is called before the first frame update
    void Start()
    {
        redPercentText = transform.Find("RedPercent").GetComponent<TMP_Text>();
        greenPercentText = transform.Find("GreenPercent").GetComponent<TMP_Text>();

        redScoreText = transform.Find("RedScore").GetComponent<TMP_Text>();
        greenScoreText = transform.Find("GreenScore").GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        redPercentText.text = "" + redFighter.percentDamage + "%";
        greenPercentText.text = "" + greenFighter.percentDamage + "%";

        redScoreText.text = "Score: " + greenFighter.deathCount;
        greenScoreText.text = "Score: " + redFighter.deathCount;
    }
}