using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(TileMapGridExporter))]
public class TileMapGridExporterEditor : Editor
{
	private TextAsset[] gridFiles;

	private TileMapGridExporter exporter => target as TileMapGridExporter;
	void OnEnable()
	{
		gridFiles = exporter.GetGridFiles();
	}

	public override void OnInspectorGUI()
	{
		base.DrawDefaultInspector();
		serializedObject.Update();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Grids");
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Save", GUILayout.Width(80)))
		{
			TileMapGrid tileMapGrid = exporter.GetComponent<TileMapGrid>();
			exporter.SaveGrid(tileMapGrid.gridData);
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginVertical();
		foreach (var item in gridFiles)
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(item.name, GUILayout.MaxWidth(200)))
			{
				exporter.LoadGrid(item);
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();

	}
}
