using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingTile : MonoBehaviour
{
	private float time;
	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		time += Time.deltaTime;
		this.transform.position = new Vector3(Mathf.Sin(time), this.transform.position.y, this.transform.position.z);
	}
}
