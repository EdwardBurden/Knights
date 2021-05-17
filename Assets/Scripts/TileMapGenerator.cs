using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapGenerator : MonoBehaviour
{
	public StartGridPosition startGridPosition = StartGridPosition.Top;
	public Tilemap floorTileMap;
	public Tilemap wallTileMap;
	public TileBase groundTile;
	public TileBase wallTile;
	public TileBase blockWalltile;

	public int width;
	public int height;

	private void Awake()
	{
		Generate();
	}


	public Vector2Int GetStartPosition()
	{
		switch (startGridPosition)
		{
			case StartGridPosition.Top:
				return new Vector2Int(-width, -height);
			case StartGridPosition.Center:
				return new Vector2Int(-width / 2, -height / 2);
			case StartGridPosition.Bottom:
				return new Vector2Int(0, 0);
			default:
				return Vector2Int.zero;
		}

	}



	public void Generate()
	{
		Vector2Int position = GetStartPosition();


		floorTileMap.ClearAllTiles();
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				Vector3Int gridPosition = new Vector3Int(i + position[0], j + position[1], 0);
				Color color = new Color(gridPosition.x, gridPosition.y, 0, 1);
				floorTileMap.SetTile(gridPosition, groundTile);

				floorTileMap.SetTileFlags(new Vector3Int(i, j, 0), TileFlags.None);
				floorTileMap.SetColor(gridPosition, color);
			}
		}

		/*wallTileMap.ClearAllTiles();
		for (int i = -1; i < width + 1; i++)
		{
			for (int j = -1; j < height + 1; j++)
			{
				if (i == -1 || j == -1 || i == width || j == height)
				{
					wallTileMap.SetTile(new Vector3Int(i, j, 0), wallTile);
					Tile tileBase = wallTileMap.GetTile(new Vector3Int(i, j, 0)) as Tile;
					wallTileMap.SetTileFlags(new Vector3Int(i, j, 0), TileFlags.None);
					wallTileMap.SetColor(new Vector3Int(i, j, 0), new Color((float)i / (float)width, (float)j / (float)height, 0, 1));
				}
			}
		}
		*/

	}

	private void Update()
	{
		/*if (Input.GetMouseButtonDown(0))
		{
			Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector3Int gridPos = tilemap.WorldToCell(pos);

			TileBase tile = tilemap.GetTile(gridPos);
			if (tile != null)
			{
				Debug.Log($"Pos {pos} Grid {gridPos} Tile {tile.name}");
			}
		}*/
	}
}
