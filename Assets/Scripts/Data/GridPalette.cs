using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObject/GridPalette")]
public class GridPalette : ScriptableObject
{
	public FloorTile roomTile;

	public TileBase voidTile;

	public FloorTile corridorTile;

	public ColliderTile colliderTile;
}
