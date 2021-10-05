using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "ScriptableObject/GridPalette")]
public class GridPalette : ScriptableObject
{
	public RoomTile roomTile;

	public TileBase voidTile;

	public CorridorTile corridorTile;

	public ColliderTile colliderTile;
	public WallTile wallTile;
}
