using UnityEngine;

public class EarthSpin : MonoBehaviour
{
	public Controller ctr;

	//The Earth does 360.985 degrees in a day which is ~0.004 degrees per sec.
	private float speed = 0.004f;

	// Update is called once per frame
	void Update()
	{
		transform.Rotate(Vector3.up, -speed * ctr.speedMultiplier * Time.deltaTime);
	}
}
