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
	
	public void StartTheGame()
	{
		//set parameters in singleton game object
		OthelloModel.Instance.SetAiLevels((int)player1.value, (int)player2.value);
		
		SceneManager.LoadScene("Othello Game");
	}
}