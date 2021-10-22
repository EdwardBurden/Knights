using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class TileMapGridExporter : MonoBehaviour
{
	private static string SAVE_FOLDER = "Grids";



	public TextAsset[] GetGridFiles()
	{
#if UNITY_EDITOR
		TextAsset[] textAssets = Resources.LoadAll<TextAsset>(SAVE_FOLDER);
		return textAssets;
#else
return null;
#endif
	}

	public void LoadGrid(TextAsset textGrid)
	{
		GridData gridData = JsonUtility.FromJson<GridData>(textGrid.text);
		GetComponent<TileMapGrid>().LoadGrid(gridData);
	}

	public void SaveGrid(GridData gridData)
	{

		string data = JsonUtility.ToJson(gridData);



#if UNITY_EDITOR
		string filename = $"{gridData.id}.txt";
		string path = Path.Combine("Assets/Resources", SAVE_FOLDER, filename);

		File.WriteAllText(path, data);
		//save to resources
#else
	//save to data persistant
#endif

	}
}
