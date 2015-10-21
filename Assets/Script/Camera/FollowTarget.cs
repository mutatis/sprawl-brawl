using UnityEngine;

/* Camera seguindo o player */

public class FollowTarget : MonoBehaviour
{
    public Transform target; // Reference to the player.
	public Vector3 offset;   // The offset at which the Health Bar follows the player.
    public float smoothTime = 0.3f; //Makes this behaviour smooth
    private float xPosition; //wanted X position
    private float yPosition; //wanted Y position
    private Vector3 velocity = Vector3.zero; //A reference value used by SmoothDamp that tracks this object velocity
	
    /// <summary>
    /// SmoothDamp is used in FixedUpdate to avoid glitchs caused by non-linear equation
    /// </summary>
	void FixedUpdate ()
	{
        if (PlayerController.player.velX > 0)
        {
            xPosition = target.position.x + offset.x;
            yPosition = offset.y;
            transform.position = Vector3.SmoothDamp(transform.position, new Vector3(xPosition, yPosition, transform.position.z), ref velocity, smoothTime);
        }
	}
}