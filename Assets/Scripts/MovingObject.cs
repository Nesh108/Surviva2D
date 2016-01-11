using UnityEngine;
using System.Collections;

public abstract class MovingObject : MonoBehaviour
{
	
	public float moveTime = 0.1f;		// Time needed for the unit to move
	public LayerMask blockingLayer; 	// Layer on which collisions are checked

	private BoxCollider2D _boxCollider;
	private Rigidbody2D _rigid2D;
	private float _inverseMoveTime;		// For performance during calculations


	// Use this for initialization
	protected virtual void Start ()
	{
		_boxCollider = GetComponent<BoxCollider2D> ();
		_rigid2D = GetComponent<Rigidbody2D> ();
		_inverseMoveTime = 1f / moveTime;
	}

	protected bool Move (int xDir, int yDir, out RaycastHit2D hit)
	{
		Vector2 start = transform.position;
		Vector2 end = start + new Vector2 (xDir, yDir);

		// Temporarily disable to avoid clashing with component's own collider
		_boxCollider.enabled = false;

		// Check if a collision is detected
		hit = Physics2D.Linecast (start, end);

		// Re-enable collider
		_boxCollider.enabled = true;

		// If collision was detected, cannot move
		if (hit.transform != null)
			return false;

		// Else start the movement to the end location
		StartCoroutine (SmoothMovement (end));
		return true;
	}

	protected IEnumerator SmoothMovement (Vector3 end)
	{
		float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

		// Loop until the position is reached
		while (sqrRemainingDistance > float.Epsilon) {
			// Calculate the new position
			Vector3 newPosition = Vector3.MoveTowards (_rigid2D.position, end, _inverseMoveTime * Time.deltaTime);
			_rigid2D.MovePosition (newPosition);
			sqrRemainingDistance = (transform.position - end).sqrMagnitude;

			// Wait for a frame before repeating the loop
			yield return null;
		}
	}

	protected virtual void AttemptMove<T> (int xDir, int yDir)
		where T : Component
	{
		RaycastHit2D hit;
		bool canMove = Move (xDir, yDir, out hit);

		if (hit.transform == null)
			return;

		T hitComponent = hit.transform.GetComponent<T> ();

		// If a collision is detected then let the object itself (enemy or player) handle it accordingly
		if (!canMove && hitComponent != null)
			OnCantMove (hitComponent);
	}

	protected abstract void OnCantMove<T> (T component)
		where T : Component;
	
}
