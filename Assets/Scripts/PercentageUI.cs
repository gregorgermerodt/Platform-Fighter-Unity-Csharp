using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PercentageUI : MonoBehaviour
{
    private TMP_Text redText;
    private TMP_Text greenText;

    [SerializeField] public Fighter redFighter;
    [SerializeField] public Fighter greenFighter;

    // Start is called before the first frame update
    void Start()
    {
        redText = transform.Find("Red").GetComponent<TMP_Text>();
        greenText = transform.Find("Green").GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        redText.text = "" + redFighter.percentDamage + "%";
        greenText.text = "" + greenFighter.percentDamage + "%";
    }
}