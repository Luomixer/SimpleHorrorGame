using NUnit.Framework.Constraints;
using UnityEngine;
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float WalkSpeed;
    float Speed;
    public float groundDrag;
    public float airDrag;


    [Header("Jump settings")]
    public static float jumpPower = 15f;
    public static float jumpPeak = 0.32f;
    public static float jumpDescent = 0.26f;
    float jumpGravity = jumpPower * 2 / Mathf.Pow(jumpPeak, 2);
    float fallGravity = jumpPower * 2 / Mathf.Pow(jumpDescent, 2);
    public float coyoteTime;
    float coyoteTimeCounter;
    bool Jumped;


    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;


    public Transform orientation;

    float horizontalInput;
    float verticalInput;
    bool jumpInput;


    Vector3 moveDirection;

    Rigidbody rb;
    ConstantForce gravity;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Делает чтоб персонаж нестал бухим и упал
        rb.freezeRotation = true;

        // Set movespeed to be walkspeed
        Speed = WalkSpeed;
    }
    private void Update()
    {
        // Uses raycast to check if players is on ground
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.5f, whatIsGround);

        // Handles keyboard inputs for moving
        HandleInput();
        
        // Makes sure speed doesn't go over limit
        SpeedController();

        // Handles stuff that happends when player is not on ground
        if (grounded)
        {
            // Sets drag to be on ground
            rb.linearDamping = Mathf.Lerp(rb.linearDamping, groundDrag, 0.25f);
            // Sets coyote timer
            coyoteTimeCounter = coyoteTime;

            Jumped = false;
        }
        else
        {
            //Sets drag to be in air
            rb.linearDamping = Mathf.Lerp(rb.linearDamping, airDrag, 0.5f);
            // Sets coyote timer
            coyoteTimeCounter -= Time.deltaTime;
            coyoteTimeCounter = Mathf.Clamp(coyoteTimeCounter, 0f, coyoteTime);
        }

        HandleJump();
        
    }
    private void FixedUpdate()
    {
        MovePlayer();
        HandleGravity();
    }
    private void HandleGravity()
    {
        // Changes gravity depending on Y
        if (rb.linearVelocity.y  < 0f)
        {
            gravity.force = new Vector3(0f, fallGravity, 0f);
        }
        else
        {
            gravity.force = new Vector3(0f, jumpGravity, 0f);
        }
    }
    private void HandleJump()
    {
        // Checks if jump is possible
        if ((grounded || coyoteTimeCounter > 0f) && jumpInput && !Jumped)
        {
            Jumped = true;
            coyoteTimeCounter = 0f;
            Jump();
        }
        // Jump buffer
        if (jumpInput && grounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        jumpInput = Input.GetButtonDown("Jump");
    }
    private void MovePlayer()
    {
        // Gets move direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        // Moves the rigi body via forces depending on state
        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * Speed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * Speed * 5f, ForceMode.Force);
        }
    }
    private void SpeedController()
    {
        // Gets current velocity
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        // Checks if velocity is over movespeed
        if (flatVel.magnitude > Speed)
        {
            Vector3 limitedVel = flatVel.normalized * Speed;
            //Caps it to movespeed
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }
    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpPower, ForceMode.Impulse);
    }
}
