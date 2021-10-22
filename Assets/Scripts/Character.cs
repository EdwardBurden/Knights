using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Character : MonoBehaviour
{
	public float movementSpeed = 0.01f;
	Rigidbody2D rbody;
	private float targetDepth = -120;

	public LayerMask target;

	private void Awake()
	{
		rbody = GetComponent<Rigidbody2D>();
		targetDepth = transform.position.z;
		//Physics.IgnoreLayerCollision(0, 8);
	}


	private void Update()
	{
		Collider2D[] collider = Physics2D.OverlapPointAll(transform.position, target);
		Debug.DrawRay(this.transform.position, Vector3.forward * 100);

		if (collider.Length > 0)
		{
			Collider2D hit = collider.OrderBy(x => Vector3.Distance(x.gameObject.transform.position, this.transform.position)).FirstOrDefault();
			Tilemap tilemap = hit.GetComponent<Tilemap>();
			if (tilemap != null)
			{
			//	return;//todo remove only for testing
				targetDepth = hit.transform.position.z;
				Vector3Int cell = tilemap.WorldToCell(transform.position);
				Tilemap wall = FindObjectOfType<TileGridPlacer>().wallMap;

				tilemap.SetTileFlags(cell, TileFlags.None);
				tilemap.SetColor(cell, Color.red);
				for (int i = 0; i < 10; i++)
				{
					for (int j = 0; j < 10; j++)
					{
						Vector3Int cellPos = cell - new Vector3Int(i, j, 0);
						wall.SetTileFlags(cellPos, TileFlags.None);
						wall.SetColor(cellPos, new Color(1,1,1,0.1f));
					}
				}
				//FindObjectOfType<TileMapGenerator>().ChangeActiveLayer(hit.GetComponent<Tilemap>());
			}
		}

	}

	// Update is called once per frame
	void FixedUpdate()
	{


		Vector2 currentPos = rbody.position;
		float horizontalInput = Input.GetAxis("Horizontal");
		float verticalInput = Input.GetAxis("Vertical");
		Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
		inputVector = Vector2.ClampMagnitude(inputVector, 1);
		Vector2 movement = inputVector * movementSpeed;
		Vector2 newPos = currentPos + movement * Time.fixedDeltaTime;
		//rbody.MovePosition(newPos);


		rbody.MovePosition(new Vector3(newPos.x, newPos.y, Mathf.Lerp(transform.position.z, targetDepth, Time.deltaTime)));
		Camera.main.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, transform.position.z - 10);
	}
}
