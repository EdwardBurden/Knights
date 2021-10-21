using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
	public Character characterPrefab;
	public CharacterEntrance CharacterEntrancePrefab;


	private void Awake()
	{
		DontDestroyOnLoad(this.gameObject);
	}

	void Start()
	{
		StartCoroutine(OpenMenuScene());//temp
	}

	private IEnumerator OpenMenuScene()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Menu");
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
		OnMenuSceneLoaded();
	}

	public void OpenDungeonScene()
	{

		StartCoroutine(OpenDungeon());

	}

	private IEnumerator OpenDungeon()
	{
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Dungeon");
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
		if (IsSceneActive("Menu"))
		{
			asyncLoad = SceneManager.UnloadSceneAsync("Menu");
			while (!asyncLoad.isDone)
			{
				yield return null;
			}
		}
		OnDungeonSceneLoaded();
	}

	private bool IsSceneActive(string name)
	{

		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			Scene scene = SceneManager.GetSceneAt(i);
			if (scene.name == name)
			{
				return true;
			}
		}
		return false;
	}

	public void OnMenuSceneLoaded()
	{
		//StartCoroutine(CloseOpeningScene());
	}

	public void OnDungeonSceneLoaded()
	{
		TileGridManager tileGridManager = FindObjectOfType<TileGridManager>();
		tileGridManager.CreateNewMap();
		Vector3 cellPositon = tileGridManager.startRoomPosition;

		CharacterEntrance characterEntrance = Instantiate(CharacterEntrancePrefab);
		Character character = Instantiate(characterPrefab);
		characterEntrance.Drop(character, cellPositon);
	}

}
