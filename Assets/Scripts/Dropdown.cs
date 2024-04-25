using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dropdown : MonoBehaviour
{
    [SerializeField] private TMP_Text selectedOption;

    public GameObject player1;
    public GameObject player2;
    public GameObject start;


    public void DropdownValue(int index)
    {
        switch (index)
        {
            case 0:
                player1.SetActive(false);
                player2.SetActive(false);
                start.SetActive(false);
                selectedOption.text = "No AI";
                break;
            case 1:
                player1.SetActive(true);
                player2.SetActive(false);
                start.SetActive(true);
                selectedOption.text = "Player 1";
                break;
            case 2:
                player1.SetActive(false);
                player2.SetActive(true);
                start.SetActive(true);
                selectedOption.text = "Player 2";
                break;
            case 3:
                player1.SetActive(true);
                player2.SetActive(true);
                start.SetActive(true);
                selectedOption.text = "Both AI";
                break;
        }
    }
}
