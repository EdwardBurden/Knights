using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
	private void Start()
	{
		StartCoroutine(TEST());
	}

	public IEnumerator TEST()
	{
		yield return new WaitForSeconds(2.0f);
		GameManager gameManager = FindObjectOfType<GameManager>();
		gameManager.OpenDungeonScene();
	}
}
