//Original Scripts by IIColour (IIColour_Spectrum)

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using PokemonUnity.Frontend.UI;
using PokemonUnity.Frontend.Overworld;
using PokemonUnity.Frontend.Global;

public class InteractDoorway : MonoBehaviour
{
    //private GameObject Player;
    private DialogBox Dialog;

    private AudioSource DoorAudio;

    private Light objectLight;
    private Collider hitBox;
    public bool isLocked = false;
    public bool hasLight = false;
    public bool dontFadeMusic = false;

    private Vector3 initPosition;
    private Quaternion initRotation;
    private Vector3 initScale;

    public GameObject leftDoor;
    public GameObject rightDoor;

    public int openRotationAmount = 66;
    public int closeRotationAmount = 66;
    public float doorSpeed = 0.25f;

    public AudioClip doorOpenClip;
    public AudioClip doorCloseClip;

    public bool limitEnterDirection;
    public CharacterBase.Direction allowedDirection;

    public enum EntranceStyle
    {
        STANDSTILL,
        OPEN,
        SWINGLEFT,
        SWINGRIGHT,
        SLIDE
    }

    public EntranceStyle entranceStyle;

    public bool movesForward = false;

    public string transferScene; //If blank, will transfer to the currently loaded scene
    public Vector3 transferPosition;
    public int transferDirection;
    public string examineText;
    public string lockedExamineText;

    public Texture2D fadeTex;

    public bool closeDoorAfter;

    

    private bool lockPlayerCamera = false;
    private Vector3 lockedPosition;
    public SceneActivity sceneActivity;

    // Use this for initialization
    void Start()
    {
        DoorAudio = transform.GetComponent<AudioSource>();
        sceneActivity = GameObject.Find("Global").GetComponent<GlobalScript>().sceneActivity;

        //Player = Player.player.gameObject;
        Dialog = GameObject.Find("Dialog").GetComponent<DialogBox>();

        objectLight = this.GetComponentInChildren<Light>();
        if (objectLight != null)
        {
            if (!hasLight)
            {
                objectLight.enabled = false;
            }
            else
            {
                objectLight.enabled = true;
            }
        }

        initPosition = transform.localPosition;
        initRotation = transform.localRotation;
        initScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (lockPlayerCamera)
        {
            Player.player.mainCamera.transform.position = lockedPosition;
        }
    }

    public IEnumerator interact()
    {
        if (isLocked)
        {
            if (lockedExamineText.Length > 0)
            {
                if (Player.player.setCheckBusyWith(this.gameObject))
                {
                    Dialog.DrawDialogBox();
                        //yield return StartCoroutine blocks the next code from running until coroutine is done.
                    yield return Dialog.StartCoroutine("drawText", lockedExamineText);
                    while (!Input.GetButtonDown("Select") && !Input.GetButtonDown("Back"))
                    {
                        //these 3 lines stop the next bit from running until space is pressed.
                        yield return null;
                    }
                    Dialog.UnDrawDialogBox();
                    yield return new WaitForSeconds(0.2f);
                    Player.player.unsetCheckBusyWith(this.gameObject);
                }
            }
        }
        else
        {
            if (examineText.Length > 0)
            {
                if (Player.player.setCheckBusyWith(this.gameObject))
                {
                    Dialog.DrawDialogBox();
                        //yield return StartCoroutine blocks the next code from running until coroutine is done.
                    yield return Dialog.StartCoroutine("drawText", examineText);
                    while (!Input.GetButtonDown("Select") && !Input.GetButtonDown("Back"))
                    {
                        //these 3 lines stop the next bit from running until space is pressed.
                        yield return null;
                    }
                    Dialog.UnDrawDialogBox();
                    yield return new WaitForSeconds(0.2f);
                    Player.player.unsetCheckBusyWith(this.gameObject);
                }
            }
        }
    }

    public IEnumerator bump()
    {
        Debug.Log(Player.player.direction);
        if(Player.player.direction == allowedDirection && limitEnterDirection == true || limitEnterDirection == false)
        {
            
            if (!isLocked && !Player.player.isInputPaused())
            {
                if (Player.player.setCheckBusyWith(this.gameObject))
                {
                    if (DoorAudio != null)
                    {
                        if (!DoorAudio.isPlaying)
                        {
                            DoorAudio.volume = PlayerPrefs.GetFloat("sfxVolume");
                            playClip(doorOpenClip);
                        }
                    }

                    if (entranceStyle == EntranceStyle.SWINGRIGHT)
                    {
                        Player.player.running = false;
                        Player.player.speed = Player.player.walkSpeed;
                        Player.player.updateAnimation("walk", Player.player.walkFPS);

                        float increment = 0f;
                        float speed = 0.25f;
                        float yRotation = transform.localEulerAngles.y;
                        while (increment < 1)
                        {
                            increment += (1f / speed) * Time.deltaTime;
                            if (increment > 1)
                            {
                                increment = 1;
                            }
                            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x,
                                yRotation + (90f * increment), transform.localEulerAngles.z);
                            Player.player.mainCamera.fieldOfView = Player.player.mainCameraDefaultFOV -
                                                                           ((Player.player.mainCameraDefaultFOV /
                                                                             10f) * increment);
                            yield return null;
                        }

                        yield return new WaitForSeconds(0.2f);
                    }
                    else if (entranceStyle == EntranceStyle.SLIDE)
                    {
                        //door open
                        float i = 0f;
                        float yRotation = transform.localEulerAngles.y;
                        while (i < 1)
                        {
                            i += (1f / doorSpeed) * Time.deltaTime;
                            if (i > 1)
                            {
                                i = 1;
                            }
                            leftDoor.transform.localEulerAngles = new Vector3(leftDoor.transform.localEulerAngles.x,
                                yRotation + (openRotationAmount * i), leftDoor.transform.localEulerAngles.z);

                            rightDoor.transform.localEulerAngles = new Vector3(rightDoor.transform.localEulerAngles.x,
                                yRotation - (openRotationAmount * i), rightDoor.transform.localEulerAngles.z);

                            Player.player.mainCamera.fieldOfView = Player.player.mainCameraDefaultFOV -
                                                                           ((Player.player.mainCameraDefaultFOV /10f) * i);
                            yield return null;
                        }



                        //player movement
                        Player.player.running = false;
                        Player.player.speed = Player.player.walkSpeed;
                        Player.player.updateAnimation("walk", Player.player.walkFPS);

                        float increment = 0f;
                        float speed = 0.25f;
                        while (increment < 1)
                        {
                            increment += (1 / speed) * Time.deltaTime;
                            if (increment > 1)
                            {
                                increment = 1;
                            }
                            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y,
                                1f - (0.92f * increment));
                            Player.player.mainCamera.fieldOfView = 20f - (2f * increment);
                            yield return null;
                        }
                        yield return new WaitForSeconds(0.2f);

                        if(closeDoorAfter)
                        {
                            i = 0f;
                            yRotation = transform.localEulerAngles.y;
                            while (i < 1)
                            {
                                i += (1f / doorSpeed) * Time.deltaTime;
                                if (i > 1)
                                {
                                    i = 1;
                                }
                                leftDoor.transform.localEulerAngles = new Vector3(leftDoor.transform.localEulerAngles.x,
                                    yRotation - (closeRotationAmount * i), leftDoor.transform.localEulerAngles.z);

                                rightDoor.transform.localEulerAngles = new Vector3(rightDoor.transform.localEulerAngles.x,
                                    yRotation + (closeRotationAmount * i), rightDoor.transform.localEulerAngles.z);

                                Player.player.mainCamera.fieldOfView = Player.player.mainCameraDefaultFOV -
                                                                               ((Player.player.mainCameraDefaultFOV /10f) * i);
                                yield return null;
                            }
                        }
                    }


                    if (entranceStyle != EntranceStyle.STANDSTILL)
                    {
                        if (entranceStyle != EntranceStyle.OPEN)
                        {
                            StartCoroutine(lockCameraPosition());
                            yield return new WaitForSeconds(0.1f);
                        }
                        Player.player.forceMoveForward();
                    }

                    //fade out the scene and load a new scene
                    //sceneActivity.playerData.fadeTex = fadeTex;
                    //float fadeTime = sceneTransition.FadeOut() + 0.4f;
                    float fadeTime = ScreenFade.slowedSpeed + 0.4f;
                    //fadeCutouts for doorways not yet implemented
                    StartCoroutine(ScreenFade.main.Fade(false, ScreenFade.slowedSpeed));
                    if (!dontFadeMusic)
                    {
                        BgmHandler.main.PlayMain(null, 0);
                    }
                    yield return new WaitForSeconds(fadeTime);
                    //reset camera and doorway transforms
                    Player.player.mainCamera.transform.localPosition =
                        Player.player.mainCameraDefaultPosition;
                    Player.player.mainCamera.fieldOfView = Player.player.mainCameraDefaultFOV;
                    transform.localPosition = initPosition;
                    transform.localRotation = initRotation;
                    transform.localScale = initScale;
                    if (!string.IsNullOrEmpty(transferScene))
                    {
                        //sceneActivity.CheckLevelLoaded(transferScene, 0);
                        sceneActivity.playerData.playerPosition = transferPosition;
                        sceneActivity.playerData.playerDirection = transferDirection;
                        sceneActivity.playerData.playerForwardOnLoad = movesForward;
                        sceneActivity.playerData.fadeIn = true;
                        UnityEngine.SceneManagement.SceneManager.LoadScene(transferScene);
                    }
                    else
                    {
                        //uncheck busy with to ensure events at destination can be run.
                        Player.player.unsetCheckBusyWith(this.gameObject);
                        //transfer to current scene, no saving/loading nessecary
                        Player.player.updateAnimation("walk", Player.player.walkFPS);
                        Player.player.speed = Player.player.walkSpeed;
                        Player.player.transform.position = transferPosition;
                        //Player.player.updateDirection(transferDirection);
                        if (movesForward)
                        {
                            Player.player.forceMoveForward();
                        }
                        sceneActivity.playerData.fadeIn = true;
                        //SceneTransition.gameScene.FadeIn();
                        StartCoroutine(ScreenFade.main.Fade(true, ScreenFade.slowedSpeed));
                        yield return new WaitForSeconds(0.1f);
                        Player.player.pauseInput(0.2f);
                    }

                    

                }
            }
        }
        
    }

    private IEnumerator lockCameraPosition()
    {
        lockPlayerCamera = true;
        lockedPosition = Player.player.mainCamera.transform.position;
        yield return new WaitForSeconds(1f);
        lockPlayerCamera = false;
    }

    private void playClip(AudioClip clip)
    {
        DoorAudio.clip = clip;
        //DoorAudio.volume = PlayerPrefs.GetFloat("sfxVolume") * .3f;
        DoorAudio.Play();
    }
}