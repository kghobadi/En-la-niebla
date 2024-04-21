using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ThirdPersonController : MonoBehaviour
{
    //Player movment variables. 
    CharacterController player;
    public float currentSpeed, walkSpeed, runSpeedMax;
    public float startingHeight, runTime;
    private Vector3 targetPosition;
    public bool isMoving, inBoat; // for point to click
    float clickTimer;
    public GameObject walkingPointer;

    //Camera ref variables
    AudioSource cameraAudSource;
    CameraController camControl;

    //vars for footstep audio
    public AudioSource playerSource;
    public AudioClip[] footsteps;
    public AudioClip[] paddles;
    public float walkStepTotal = 1f, runStepTotal = 0.5f;
    float footStepTimer = 0;
    int currentStep = 0, currentPaddle = 0;

    //set publicly to tell this script what raycasts can and can't go thru
    public LayerMask mask;
    public LayerMask boatMask;

    //dictionary to sort nearby audio sources by distance 
    Dictionary<AudioSource, float> soundCreators = new Dictionary<AudioSource, float>();

    //listener range
    public float listeningRadius;
    //store this mouse pos
    Vector3 lastPosition;

    //player sprites on land
    public GameObject currentAnimation, idle, walkingFB, walkingL, walkingR;

    //player sprites in Boat
    public GameObject boat, boatIdle, boatIdleRight, boatIdleLeft, 
        paddleRightFwd, paddleRightBkwd, paddleLeftFwd, paddleLeftBkwd;
    public Rigidbody boatBody;
    public float boatSpeedX, boatSpeedZ;
    public float velocityMax, torqueMax;
    float paddleIdleTimer, holdPaddle = 1f;

    //UI walking
    Image symbol; // 2d sprite renderer icon reference
    AnimateUI symbolAnimator;
    public Sprite[] walkingSprites; // walking feet cursor
    int currentWalk = 0;
    public bool walkingSpritesOn, touchingSomething, boatVariablesSet, boatRotating;

    void Start()
    {
        //walking UI
        symbol = GameObject.FindGameObjectWithTag("Symbol").GetComponent<Image>(); //searches for InteractSymbol
        symbolAnimator = symbol.GetComponent<AnimateUI>();
       

        symbol.sprite = walkingSprites[currentWalk];
        symbolAnimator.animationSprites = walkingSprites;
        walkingSpritesOn = true;
        playerSource = GetComponent<AudioSource>();

        //cam refs
        cameraAudSource = Camera.main.GetComponent<AudioSource>();
        camControl = Camera.main.GetComponent<CameraController>();

        //set starting points for most vars
        player = GetComponent<CharacterController>();
        targetPosition = transform.position;

        //turn off walking sprites at start
        ChangeAnimState(idle);

        //set current speed and startingHeight
        startingHeight = transform.position.y;
        currentSpeed = walkSpeed;

    }

    void Update()
    {
        if (!inBoat)
        {
            LandMovement();
        }

        // BOAT stuff
        else
        {
            BoatMovement();

        }
    }

    void LandMovement()
    {
        //set back to walking sprites
        if (!walkingSpritesOn && !touchingSomething)
        {
            transform.SetParent(null);
            symbol.sprite = walkingSprites[currentWalk];
            symbolAnimator.animationSprites = walkingSprites;
            symbolAnimator.active = false;
            camControl.zoomedOut = true;
            camControl.inBoat = false;
            boatVariablesSet = false;
            walkingSpritesOn = true;
        }

        //click to move to point
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            clickTimer += Time.deltaTime;
            if (clickTimer > runTime && currentSpeed < runSpeedMax)
            {
                currentSpeed += Time.deltaTime * 5;
            }

            if (Physics.Raycast(ray, out hit, 100, mask))
            {
                //if we hit the ground & height is in range, move the character to that position
                if (hit.transform.gameObject.tag == "Ground")
                {
                    walkingPointer.transform.position = hit.point;
                    targetPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
                    isMoving = true;

                }

                //if we hit an interactable object AND we are far from it, the player should auto walk towards it
                else if (Vector3.Distance(transform.position, hit.transform.position) > 5 &&
                (hit.transform.gameObject.tag == "Animal"))
                {
                    targetPosition = new Vector3(hit.point.x + 2, transform.position.y, hit.point.z + 2);
                    walkingPointer.transform.position = new Vector3(targetPosition.x, targetPosition.y - 1, targetPosition.z);
                    isMoving = true;
                }
                else
                {
                    isMoving = false;
                }
            }
        }

        //On mouse up, we check clickTimer to see if we are walking to that point or stopping the character from running 
        if (Input.GetMouseButtonUp(0))
        {
            playerSource.PlayOneShot(footsteps[currentStep]);
            //increment footstep audio
            if (currentStep < (footsteps.Length - 1))
            {
                currentStep++;
            }
            else
            {
                currentStep = 0;
            }
            //if this is true, can start running
            if (clickTimer < runTime)
            {
                isMoving = true;
                clickTimer = 0;
                currentSpeed = walkSpeed;
                //set walk sprite
                if (currentWalk < (walkingSprites.Length - 1))
                {
                    currentWalk++;
                }
                else
                {
                    currentWalk = 0;
                }
                symbol.sprite = walkingSprites[currentWalk];
                symbolAnimator.active = false;
                walkingPointer.SetActive(true);
            }
            else
            {
                symbolAnimator.active = false;
                isMoving = false;
                clickTimer = 0;
                currentSpeed = walkSpeed;
            }
        }

        //Check if we are moving and transition animation controller
        if (isMoving)
        {
            PlayerWalk();

            footStepTimer += Time.deltaTime;

            if (currentSpeed > 12)
            {
                //play footstep sound
                if (footStepTimer > runStepTotal)
                {
                    playerSource.PlayOneShot(footsteps[currentStep]);
                    //increment footstep audio
                    if (currentStep < (footsteps.Length - 1))
                    {
                        currentStep += 1;
                    }
                    else
                    {
                        currentStep = 0;
                    }
                    footStepTimer = 0;
                }
                //animate ui
                walkingPointer.SetActive(false);
                symbolAnimator.active = true;
            }
            else
            {
                //play footstep sound
                if (footStepTimer > walkStepTotal)
                {
                    playerSource.PlayOneShot(footsteps[currentStep]);
                    //increment footstep audio
                    if (currentStep < (footsteps.Length - 1))
                    {
                        currentStep += Random.Range(0, (footsteps.Length - currentStep));
                    }
                    else
                    {
                        currentStep = 0;
                    }
                    footStepTimer = 0;
                }
            }


        }
        //this timer only plays the idle animation if we are not moving. still a little buggy
        else
        {
            footStepTimer = 0;
            walkingPointer.SetActive(false);

            ChangeAnimState(idle);
        }


        lastPosition = transform.position;
    }

    //For stereophyta, you will need this chunk and the playpaddlesound function below
    void BoatMovement()
    {
        //what happens when the player is in the boat?
        if (!boatVariablesSet)
        {
            transform.SetParent(boat.transform);
            transform.localPosition = new Vector3(0, 0, -1);
            boatBody.isKinematic = false;
            camControl.zoomedOut = false;
            camControl.inBoat = true;
            boatVariablesSet = true;
        }

        paddleIdleTimer += Time.deltaTime;

        if (paddleIdleTimer > holdPaddle)
        {
            ChangeAnimState(boatIdle);
        }

        Debug.Log(boatBody.velocity);

        //click to move to point
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            //raycast info
            if (Physics.Raycast(ray, out hit, 100, boatMask))
            {
                if (hit.transform.gameObject.tag == "Water")
                {
                    //directions to move in the water

                    //grab the Pure boat rotation using quaternion method
                    //grab the boats velocity
                    
                    //the whole setup of the player/cam view locked on its rotation makes this way of coding the raycasts useless
                    //needs to be in relation to the player & the boats shared rotation
                    //in essence, the player becomes the boat, and the camera follows that

                    //forward right paddle
                    if (hit.point.x > transform.position.x && hit.point.z > transform.position.z)
                    {
                        //Add torque on negative x 
                        if(boatBody.velocity.magnitude < velocityMax)
                        {
                            boatBody.AddTorque(0, -boatSpeedX, 0);
                        }
                        //push the boat forward to its relative rotation
                        if(boatBody.angularVelocity.magnitude < torqueMax)
                        {
                            boatBody.AddRelativeForce(0, 0, boatSpeedZ);
                        }

                        ChangeAnimState(paddleRightFwd);
                    }

                    //forward left paddle
                    else if (hit.point.x < transform.position.x && hit.point.z > transform.position.z)
                    {
                        //Add torque on positive x 
                        if (boatBody.velocity.magnitude < velocityMax)
                        {
                            boatBody.AddTorque(0, boatSpeedX, 0);
                        }
                        //push the boat forward to its relative rotation
                        if (boatBody.angularVelocity.magnitude < torqueMax)
                        {
                            boatBody.AddRelativeForce(0, 0, boatSpeedZ);
                        }

                        ChangeAnimState(paddleLeftFwd);
                    }

                    //backward right paddle
                    else if (hit.point.x > transform.position.x && hit.point.z < transform.position.z)
                    {
                        //Add torque on positive x 
                        if (boatBody.velocity.magnitude < velocityMax)
                        {
                            boatBody.AddTorque(0, boatSpeedX, 0);
                        }
                        //push the boat backward to its relative rotation
                        if (boatBody.angularVelocity.magnitude < torqueMax)
                        {
                            boatBody.AddRelativeForce(0, 0, -boatSpeedZ);
                        }

                        ChangeAnimState(paddleRightBkwd);
                    }

                    //backward left paddle
                    else if (hit.point.x < transform.position.x && hit.point.z < transform.position.z)
                    {
                        //Add torque on negative x 
                        if (boatBody.velocity.magnitude < velocityMax)
                        {
                            boatBody.AddTorque(0, -boatSpeedX, 0);
                        }
                        //push the boat backward to its relative rotation
                        if (boatBody.angularVelocity.magnitude < torqueMax)
                        {
                            boatBody.AddRelativeForce(0, 0, -boatSpeedZ);
                        }

                        ChangeAnimState(paddleLeftBkwd);

                        //OLD 
                        //adds force at position
                        //Vector3 force = new Vector3(-boatSpeedX, 0, -boatSpeedZ);
                        //Vector3 position = hit.point;
                        //boatBody.AddForceAtPosition(force, position);
                    }

                    paddleIdleTimer = 0;
                    PlayPaddleSound();
                }

                //when in boat next to ground, exit boat
                else if (hit.transform.gameObject.tag == "Ground" && Vector3.Distance(transform.position, hit.point) < 10 && paddleIdleTimer > 1)
                {
                    inBoat = false;
                    transform.position = new Vector3(hit.point.x, hit.point.y + 1.5f, hit.point.z);
                    boatBody.isKinematic = true;
                    ChangeAnimState(idle);
                }
            }
        }
    
    }

    void PlayPaddleSound()
    {
        //count through paddle sound array
        if (currentPaddle < paddles.Length)
        {
            currentPaddle++;
        }
        else
        {
            currentPaddle = 0;
        }

        //play one shot of current sound
        playerSource.PlayOneShot(paddles[currentPaddle]);
    }
   
    //Movement function which relies on vector3 movetowards. when we arrive at target, stop moving.
    void PlayerWalk()
    {
        //first calculate rotation and look
        targetPosition = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);

        float currentDist = Vector3.Distance(transform.position, targetPosition);

        //if statements to decide which walking sprites to show based on targetPosition
        if(targetPosition.x < transform.position.x && targetPosition.z > transform.position.z)
        {
            //walking to the left
            ChangeAnimState(walkingL);
        }
        else if (targetPosition.x > transform.position.x && targetPosition.z > transform.position.z)
        {
            //walking forward
            ChangeAnimState(walkingFB);
        }
        else if (targetPosition.x < transform.position.x && targetPosition.z < transform.position.z)
        {
            //walking backward
            ChangeAnimState(walkingFB);
        }
        else if (targetPosition.x > transform.position.x && targetPosition.z < transform.position.z)
        {
            //walking to the right
            ChangeAnimState(walkingR);
        }

        //this is a bit finnicky with char controller so may need to continuously set it 
        if (currentDist >= 0.5f)
        {
            transform.LookAt(targetPosition);

            //then set movement
            Vector3 movement = new Vector3(0, 0, currentSpeed);

            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, transform.localEulerAngles.z);

            movement = transform.rotation * movement;

            //Actually move
            player.Move(movement * Time.deltaTime);

            player.Move(new Vector3(0, -0.5f, 0));
        }
        else
        {
            isMoving = false;
           
        }
    }

    public void ChangeAnimState (GameObject desiredAnim)
    {
        if(desiredAnim != currentAnimation)
        {
            //turn everything off
            idle.SetActive(false);
            walkingFB.SetActive(false);
            walkingL.SetActive(false);
            walkingR.SetActive(false);
            boatIdle.SetActive(false);
            boatIdleRight.SetActive(false);
            boatIdleLeft.SetActive(false);
            paddleRightFwd.SetActive(false);
            paddleRightBkwd.SetActive(false);
            paddleLeftFwd.SetActive(false);
            paddleLeftBkwd.SetActive(false);

            //set active this anim
            desiredAnim.SetActive(true);
            currentAnimation = desiredAnim;
        }
    }

    
}
