using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public float maxRotationDegrees = 1f;

    public Animator animator;

    public MazeRenderer generator;
    public Transform player3dModel;
    public Transform cameraTransform;
    public Vector3 cameraOffset;

    private Rigidbody2D body;
    private Vector2 input;

    private Vector2 realPosition;
    private string playingAnimation;

    // Start is called before the first frame update
    void Start()
    {
        realPosition = GetRealPosition();
        body = GetComponent<Rigidbody2D>();
        playingAnimation = "Idle";
    }

    // Update is called once per frame
    void Update()
    {
        input = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );

        Vector3 worldPosition = new Vector3(transform.position.x, 0, transform.position.y);

        // Update player position
        player3dModel.transform.position = worldPosition;

        // Update player rotation
        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 lookTowards = new Vector3(input.x, 0f, input.y);
            Quaternion finalRotation = Quaternion.LookRotation(lookTowards);
            Quaternion partialRotation = Quaternion.RotateTowards(player3dModel.transform.rotation, finalRotation, maxRotationDegrees * Time.deltaTime);
            player3dModel.transform.rotation = partialRotation;

            if (playingAnimation == "Idle")
            {
                animator.Play("Run");
                playingAnimation = "Run";
            }
        }
        else
        {
            if (playingAnimation == "Run")
            {
                animator.Play("Idle");
                playingAnimation = "Idle";
            }
        }

        // Update camera
        cameraTransform.position = worldPosition + cameraOffset;
        cameraTransform.LookAt(player3dModel.transform);
    }

    void FixedUpdate()
    {
        body.velocity = input * speed;

        Vector2 previousRealPosition = realPosition;
        realPosition = GetRealPosition();

        generator.OnPlayerPositionChanged(previousRealPosition, realPosition);
    }

    // public void OnGUI()
    // {
    //     GUI.Label(new Rect(10, 10, 400, 100),
    //         "realPosition: " + realPosition.ToString()
    //     );
    // }

    public void ResetPosition()
    {
        transform.position = Vector3.zero;
        realPosition = Vector2.zero;
        input = Vector2.zero;
    }

    private Vector2 GetRealPosition()
    {
        return new Vector2(
            transform.position.x + 0.5f,
            transform.position.y + 0.5f
        );
    }
}
