using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed;
    public float planetFaceSpeed;

    public float groundDrag;

    [Header("Jump Stuff")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundMask;
    bool isGrounded;
    public float groundDistance = 0.2f;

    //Direction Stuff
    public Transform orientation;
    Vector3 moveDirection;

    //Inputs
    float horizontalInput;
    float verticalInput;

    //Rigidbody
    Rigidbody rb;

    //Raycast Distance
    public float range = 20f;

    //Cam
    public Camera cam;

    //Gravity Stuff
    public Transform gravitySingularity;
    public float singularityMass;
    public float gravitationalForce;
    public Vector3 direction;
    public float distance;
    MeshGenerator meshGenerator;
    Vector3 gravityUp;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        meshGenerator = FindObjectOfType<MeshGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        //Ground Check
        //isGrounded = Physics.Raycast(transform.position, Vector3.down, (playerHeight * 0.5f) + groundDistance, groundMask);
        isGrounded = Physics.Raycast(transform.position, gravityUp, (playerHeight * 0.5f) + groundDistance, groundMask);

        PlayerInput();
        SpeedControl();

        //Add Drag
        if (isGrounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    void FixedUpdate()
    {
        PlayerMove();

        Gravity();
    }

    void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //When Jumping
        if (Input.GetKey(jumpKey) && readyToJump && isGrounded)
        {
            readyToJump = false;

            Jump();

            Invoke("ResetJump", jumpCooldown);
        }
    }

    void PlayerMove()
    {
        //Player Movement

        //Calculates the movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //If the Player is on the Ground
        if (isGrounded)
            rb.AddForce(moveDirection.normalized * speed * 10, ForceMode.Force);

        //In the Air
        else if(!isGrounded)
            rb.AddForce(moveDirection.normalized * speed * 10 * airMultiplier, ForceMode.Force);
    }

    void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        //If the velocity is to HIGH, then DECREASE the SPEED
        if (flatVel.magnitude > speed)
        {
            Vector3 limitedVel = flatVel.normalized * speed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    void Jump()
    {
        //Reset the Y Velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void ResetJump()
    {
        readyToJump = true;
    }

    void Gravity()
    {
        //Character Gravity

        //Finds the Direction
        direction = (transform.position - (gravitySingularity.position + new Vector3(meshGenerator.radius, meshGenerator.radius, meshGenerator.radius))).normalized;

        //Finds the Distance
        distance = (transform.position - (gravitySingularity.position + new Vector3(meshGenerator.radius, meshGenerator.radius, meshGenerator.radius))).magnitude;

        //Gets the Force
        gravitationalForce = -((rb.mass * singularityMass) / distance);

        //Applies the gravity
        rb.AddForce(direction * gravitationalForce);



        //Character Changes

        //Changes Orientation of the Player
        //float xRad = Mathf.Acos(direction.x) - 90; //Minus 90 because the player is long ways up at the top, which is 90 deg
        //float xDeg = (xRad / Mathf.PI) * 180;

        //float yRad = Mathf.Asin(direction.y);
        //float yDeg = (yRad / Mathf.PI) * 180;

        //transform.Rotate(xDeg, 0, 0);

        //float zRad = Mathf.Acos(direction.z);
        //float zDeg = (zDeg / 180) * Mathf.PI;

        //StartCoroutine(FacePlanet());
        FacePlanet();
    }

    private void FacePlanet()
    {
        gravityUp = (gravitySingularity.position + new Vector3(meshGenerator.radius, meshGenerator.radius, meshGenerator.radius) - transform.position).normalized;
        Vector3 localUp = -transform.up;

        Quaternion targetRotation = Quaternion.FromToRotation(localUp, gravityUp) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 50f * Time.deltaTime);

        //float time = 0;

        //while (time < 1)
        //{
        //    transform.rotation = Quaternion.Slerp(transform.rotation, gravityUp, time);

        //    time += Time.deltaTime * planetFaceSpeed;

        //    yield return null;
        //}
    }

    void PlayerInteract()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        //Player Interactions
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, range))
            {
                Interactable interactable = hit.collider.GetComponent<Interactable>();

                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }
    }
}
