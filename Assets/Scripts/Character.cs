using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
	public float movementSpeed = 1f;
	Rigidbody2D rbody;


	private void Awake()
	{
		rbody = GetComponent<Rigidbody2D>();
	}


	private void Update()
	{

		Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Collider2D[] collider = Physics2D.OverlapPointAll(transform.position);
		RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
		Debug.DrawRay(this.transform.position, Vector3.forward * 100);

		if (collider.Length > 0)
		{
			Debug.Log("p");
		}
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		Camera.main.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, -120);

		Vector2 currentPos = rbody.position;
		float horizontalInput = Input.GetAxis("Horizontal");
		float verticalInput = Input.GetAxis("Vertical");
		Vector2 inputVector = new Vector2(horizontalInput, verticalInput);
		inputVector = Vector2.ClampMagnitude(inputVector, 1);
		Vector2 movement = inputVector * movementSpeed;
		Vector2 newPos = currentPos + movement * Time.fixedDeltaTime;
		rbody.MovePosition(newPos);


	}
}
