using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class OthelloMainMenu : MonoBehaviour
{
	public Button startGamne;
	public Slider player1;
	public Slider player2;

	void Start()
	{
		
	}

    private void Update()
    {
        if ((player1.isActiveAndEnabled && player1.value > 0) || (player2.isActiveAndEnabled && player2.value > 0) || (!player1.isActiveAndEnabled && !player2.isActiveAndEnabled))
		{
			startGamne.enabled = true;
		} else
		{
			startGamne.enabled = false;
		}
    }

    public void StartTheGame()
	{
		//set parameters in singleton game object
		OthelloModel.Instance.SetAiLevels((int)player1.value, (int)player2.value);

		OthelloModel.Instance.ResetBoard();
		
		SceneManager.LoadScene("Othello Game");
	}
}