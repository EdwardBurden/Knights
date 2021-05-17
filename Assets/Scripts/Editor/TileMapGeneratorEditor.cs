using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileMapGenerator))]
public class TileMapGeneratorEditor : Editor
{


	void OnEnable()
	{

	}

	public override void OnInspectorGUI()
	{
		base.DrawDefaultInspector();
		serializedObject.Update();
	
		if (GUILayout.Button("Generate"))
		{
			TileMapGenerator tileMap = target as TileMapGenerator;
			tileMap.Generate();
		}
	}
}
