using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public class GUIEvents : MonoBehaviour
{
	private UIDocument uiDoc;
	private RadioButtonGroup whitePlayer, blackPlayer;
	private Button startGame;
	private SliderInt whiteAILevel, blackAILevel;
	

	protected void Start()
	{
		uiDoc = GameObject.FindAnyObjectByType<UIDocument>();
		
		whitePlayer = uiDoc.rootVisualElement.Q<RadioButtonGroup>("whitePlayerSelector");
		blackPlayer = uiDoc.rootVisualElement.Q<RadioButtonGroup>("blackPlayerSelector");
		
		startGame = uiDoc.rootVisualElement.Q<Button>("Button");
		
		//same thing for slider ints
		
		whitePlayer.RegisterCallback<ClickEvent>(PlayerSelected);
		blackPlayer.RegisterCallback<ClickEvent>(PlayerSelected);
		
		startGame.RegisterCallback<ClickEvent>(StartTheGame);
	}
	
	private void StartTheGame(ClickEvent evt)
	{
		//set parameters in singleton game object
		
		SceneManager.LoadScene("GamePlay");
	}
	
	private void PlayerSelected(ClickEvent evt) 
	{
		if ((whitePlayer.value != -1) && (blackPlayer.value != -1))
		{
			startGame.visible = true;
		}
		
		whiteAILevel.visible = whitePlayer.value == 1;
		blackAILevel.visible = blackPlayer.value == 1;
	}
}