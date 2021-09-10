using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.Tilemaps.Tile;

[CreateAssetMenu(menuName = "Collider")]
public class ColliderTile : TileBase
{
    public Sprite[] m_Sprites;
    public Sprite m_Preview;

    public override void RefreshTile(Vector3Int location, ITilemap tilemap)
	{
		for (int yd = -1; yd <= 1; yd++)
			for (int xd = -1; xd <= 1; xd++)
			{
				Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
				if (HasCollider(tilemap, position))
					tilemap.RefreshTile(position);
			}
	}

	private bool HasCollider(ITilemap tilemap, Vector3Int position)
	{
		return tilemap.GetTile(position) == this;
	}

    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
    {
        int mask = HasCollider(tilemap, location + new Vector3Int(0, 1, 0)) ? 1 : 0;
        mask += HasCollider(tilemap, location + new Vector3Int(1, 0, 0)) ? 2 : 0;
        mask += HasCollider(tilemap, location + new Vector3Int(0, -1, 0)) ? 4 : 0;
        mask += HasCollider(tilemap, location + new Vector3Int(-1, 0, 0)) ? 8 : 0;
        int index = GetIndex((byte)mask);
        if (index >= 0 && index < m_Sprites.Length)
        {
            tileData.sprite = m_Sprites[index];
            tileData.color = Color.white;
          //  var m = tileData.transform;
        //    m.SetTRS(Vector3.zero, GetRotation((byte)mask), Vector3.one);
          //  tileData.transform = m;
            tileData.flags = TileFlags.LockTransform;
            tileData.colliderType = ColliderType.None;
        }
        else
        {
            Debug.LogWarning("Not enough sprites in RoadTile instance");
        }
    }

    private Quaternion GetRotation(byte mask)
    {
        switch (mask)
        {
            case 9:
            case 10:
            case 7:
            case 2:
            case 8:
                return Quaternion.Euler(0f, 0f, -90f);
            case 3:
            case 14:
                return Quaternion.Euler(0f, 0f, -180f);
            case 6:
            case 13:
                return Quaternion.Euler(0f, 0f, -270f);
        }
        return Quaternion.Euler(0f, 0f, 0f);
    }

    private int GetIndex(byte mask)
    {
        switch (mask)
        {
            case 0: return 0;
            case 3:
            case 6:
            case 9:
            case 12: return 1;
            case 1:
            case 2:
            case 4:
            case 5:
            case 10:
            case 8: return 2;
            case 7:
            case 11:
            case 13:
            case 14: return 3;
            case 15: return 4;
        }
        return -1;
    }
}
