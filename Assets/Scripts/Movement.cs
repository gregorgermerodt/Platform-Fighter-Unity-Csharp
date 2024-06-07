using UnityEngine;

public class Movement : MonoBehaviour
{
    private CharacterController charController;  // Reference zum Character Controller
    public float playerSpeed = 6.0f;            // Geschwindigkeit des Spielers
    public float gravityAcceleration = -9.81f;
    Vector3 gravityVelocity;
    public Transform groundCheck;               // Referenz f�r Objekt am Fu�e des Spielers
    public float groundDistance = 1f;         // Radius von der unsichtbaren Sph�re die um den Mittelpunkt des
                                                // groundCheck Objekts projiziert wird
    public LayerMask groundMask;                // Bestimmt auf welche Objekte die Sph�re checken soll
    public float jumpHeight = 3.0f;
    public static bool isGrounded;

    private void Start()
    {
        charController = GetComponent<CharacterController>();
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && gravityVelocity.y < 0)
        {
            gravityVelocity.y = -2.0f;
        }

        Vector2 inputVector = new Vector2(0.0f, 0.0f);

        if (Input.GetKey(KeyCode.W))
            inputVector += new Vector2(0.0f, 1.0f);
        if (Input.GetKey(KeyCode.A))
            inputVector += new Vector2(-1.0f, 0.0f);
        if (Input.GetKey(KeyCode.S))
            inputVector += new Vector2(0.0f, -1.0f);
        if (Input.GetKey(KeyCode.D))
            inputVector += new Vector2(1.0f, 0.0f);
        if (Input.GetButtonDown("Jump") && isGrounded)
            gravityVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityAcceleration);

        float elapsedTime = 0f;
        float timeToElapse = 5f;
        while (elapsedTime < timeToElapse)
        {
            //Debug.Log(Time.deltaTime);
            elapsedTime += Time.deltaTime;
        }

        Vector3 moveDir = new Vector3(inputVector.x, 0.0f, inputVector.y).normalized;

        if(moveDir.magnitude >= 0.1f)
        {
            charController.Move(playerSpeed * Time.deltaTime * moveDir);
            float rotationSpeed = 10.0f;
            transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotationSpeed);
        }
        gravityVelocity.y += gravityAcceleration * Time.deltaTime;
        charController.Move(gravityVelocity * Time.deltaTime);
    }
}
