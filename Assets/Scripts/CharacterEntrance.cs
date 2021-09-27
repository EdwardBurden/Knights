using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEntrance : MonoBehaviour
{
	public void BeginCharacterEntrance()
	{
		//take control from played
		//spawn things
		//move camera
		//animate stuff
	}

	public void OnCharacterEntranceAnimationEnd()
	{
		//despawn stuff 
		//reset camera
		//give player control
	}

	internal void Drop(Character character, Vector3 centerPosition)
	{
		character.transform.position = centerPosition;//wrong but try
	}
}
