using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class WallRuleTile : RuleTile<WallRuleTile.Neighbor>
{
	public bool customField;
	TileGridPlacer tileGridPlacer;
	public class Neighbor : RuleTile.TilingRule.Neighbor
	{
		public const int Floor = 3;
		public const int NotFloor = 4;
		public const int Corridor = 5;
	}


	public override bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, ref Matrix4x4 transform)
	{
		return base.RuleMatches(rule, position, tilemap, ref transform);
	}

	//USE JUST FLOOR FOR NOW
	public override bool RuleMatch(int neighbor, TileBase tile, Vector3Int position)
	{
		switch (neighbor)
		{
			case Neighbor.Floor: return CheckFloor(position);
			case Neighbor.NotFloor: return !CheckFloor(position);
			case Neighbor.Corridor: return CheckCorridor(position);
		}
		return base.RuleMatch(neighbor, tile, position);
	}



	private bool CheckFloor(Vector3Int position)
	{
		if (tileGridPlacer == null)
		{
			tileGridPlacer = FindObjectOfType<TileGridPlacer>();
		}
		return tileGridPlacer.HasRoomTile(position);
	}

	private bool CheckCorridor(Vector3Int position)
	{
		if (tileGridPlacer == null)
		{
			tileGridPlacer = FindObjectOfType<TileGridPlacer>();
		}
		return tileGridPlacer.HasCorrdiorTile(position);
	}

}