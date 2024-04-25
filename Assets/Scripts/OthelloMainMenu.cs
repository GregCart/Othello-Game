using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public class OthelloMainMenu : MonoBehaviour
{
	public Button startGamne;
	public Slider player1;
	public Slider player2;

	protected void Start()
	{
		
	}
	
	private void StartTheGame()
	{
		//set parameters in singleton game object
		//OthelloModel.Instance.
		
		SceneManager.LoadScene("Othello Game");
	}
}