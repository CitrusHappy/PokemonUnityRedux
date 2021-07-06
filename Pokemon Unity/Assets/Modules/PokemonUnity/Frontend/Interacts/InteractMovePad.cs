//Original Scripts by IIColour (IIColour_Spectrum)

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using PokemonUnity.Frontend.UI;
using PokemonUnity.Frontend.Overworld;
using PokemonUnity.Frontend.Global;

public class InteractMovePad : MonoBehaviour
{
    //private GameObject Player;

    private AudioSource padAudio;
    public AudioClip stepSound;
    public bool limitEnterDirection;
    public CharacterBase.Direction allowedDirection;
    public InteractMovePad detectPad;

    public enum PadStyle
    {
        WALK,
        SPIN,
        SLIDE
    }

    public PadStyle padStyle;
    public CharacterBase.Direction finishDirection;



    // Use this for initialization
    void Start()
    {
        padAudio = transform.GetComponent<AudioSource>();
    }


    public IEnumerator bump()
    {
        Debug.Log(Player.player.direction);

        if(Player.player.direction == allowedDirection && limitEnterDirection == true || limitEnterDirection == false)
        {
            if (!Player.player.isInputPaused() && detectPad != null)
            {
                if (Player.player.setCheckBusyWith(this.gameObject))
                {
                    if (padAudio != null)
                    {
                        if (!padAudio.isPlaying)
                        {
                            padAudio.volume = PlayerPrefs.GetFloat("sfxVolume");
                            playClip(stepSound);
                        }
                    }

                    if (padStyle == PadStyle.WALK)
                    {
                        //player walks towards until reaches another pad
                    }
                    else if (padStyle == PadStyle.SLIDE)
                    {
                        //player slides foward with no animation
                    }
                    else if (padStyle == PadStyle.SPIN)
                    {
                        //player spins around clockwise during transport
                    }


                    //uncheck busy with to ensure events at destination can be run.
                    Player.player.unsetCheckBusyWith(this.gameObject);
                    Player.player.updateAnimation("walk", Player.player.walkFPS);
                    Player.player.speed = Player.player.walkSpeed;


                    //Player.player.forceMoveForward();
                    float increment = 0f;
                    float speed = 0.25f;
                    while (increment < 1 && detectPad == null)
                    {
                        increment += (1 / speed) * Time.deltaTime;
                        if (increment > 1)
                        {
                            increment = 1;
                        }
                        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y,
                            1f - (0.92f * increment));
                        Player.player.mainCamera.fieldOfView = 20f - (2f * increment);

                        //raycast to check for new pad
                        //set detect pad if found
                        yield return null;
                    }

                    

                    yield return new WaitForSeconds(0.1f);
                    Player.player.pauseInput(0.2f);
                }
            }
        }
        
    }

    private void playClip(AudioClip clip)
    {
        padAudio.clip = clip;
        padAudio.Play();
    }
}