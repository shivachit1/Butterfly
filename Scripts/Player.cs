﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Player : MonoBehaviour
{
    private Rigidbody2D player;
    private Animator PlayerAnim;

    // touch position when user touches the screen
    private Vector3 touchPosition;

    // touch direction to move player around, assigned only when screen touch starts.
    private Vector3 direction;

    // For Accelerometer Starting postion
    private Vector2 startingMobilePosition = Vector3.zero;
    // speed for player movement
    private float moveSpeed=10f;

    string gameStatus="start";

    
    public HealthBar healthBar;

    
    public GameObject magnetArea;

    // Game object to call when Particles brusts is needed
    public ParticleBrustManager particleBrustManager;

    bool magnetActivated=false;
    float Timer=3;

    public UIManager UIManager;

    void Start()
    {
        
        player= GetComponent<Rigidbody2D>();
        PlayerAnim = GetComponent<Animator>();
        healthBar.SetMaxHealth();
        startingMobilePosition.x =Input.acceleration.x;
        startingMobilePosition.y =Input.acceleration.y;
        magnetArea.SetActive(false);

    }

    // Update is called on fixed rate
   void FixedUpdate()
{
       
         
         if(gameStatus=="start"){
                handleTouchMovement();

                Timer -= Time.deltaTime;
                magnetArea.transform.position = player.transform.position;
                    
                if(Timer > 0){
                        if(magnetActivated){
                            magnetActivated = false;
                            magnetArea.SetActive(true);
                        }
                }
                else {
                    magnetArea.SetActive(false);
                }


         }
         else if (gameStatus=="gameOver"){
               UIManager.ShowTimerUI(false);
                magnetArea.SetActive(false);
                PlayerAnim.SetBool("dead",true);

         }
        
       
    }
     
     
    // Acceleration sensor for movement
    private void handleMovement(){
        
        float movementX = Input.acceleration.x;
        float movementY = Input.acceleration.y-startingMobilePosition.y;
            player.velocity = new Vector2(50f*movementX,0);
            
            Debug.Log(movementX);
      
    }

   
   bool playerTouched=false;
    private void handleTouchMovement(){
        

            // touch Functionality (i.e how many places user has touch on screen independently at the moment)
            if(Input.touchCount > 0){

                // only using first touch function ignoring other touches.
                    Touch touch = Input.GetTouch(0);
                  
                        touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                         
                         if (touch.phase == TouchPhase.Moved){

                                // Raycast2D layar to check where the user has touched on the screen.
                             RaycastHit2D hitInformation = Physics2D.Raycast(touchPosition, Camera.main.transform.forward);
                            
                            if (hitInformation.collider != null) {
                                
                                // Checking if user has touch on player object 
                                GameObject touchedObject = hitInformation.transform.gameObject;
                                
                                string playerName = touchedObject.transform.name;
                                
                                if(playerName=="Player"){
                                    playerTouched=true;
                                }


                            }

                            // drag movement allowed only when user has first touch the Game object otherwise staying still
                            if(playerTouched){
                                 player.position = new Vector3(touchPosition.x,touchPosition.y+0.5f);
                            }

                         }

                        // touch phase ended, player velocy set to 0, and player touch deactivated. New touch on player object needed to activate again.
                        if(touch.phase == TouchPhase.Ended){
                            player.velocity = Vector2.zero;
                            playerTouched=false;
                        }
                    }
      
    }


void OnTriggerEnter2D(Collider2D other) {
      // if collision to rain drop, then play sound, decrease health, and call to instantiate small water drop particles  
        if(gameStatus!="gameOver"){
            
            if(other.tag=="raindrop"){
                
                bool gameContinue = healthBar.DecreaseHealth();
                particleBrustManager.showRainParticles();
               
                if(!gameContinue){
                    
                     gameStatus="gameOver";
                     SoundManagerScript.PlayDeadSound();
                     UIManager.ShowGameOverUI();
                   
                   
                }
                else{
                    SoundManagerScript.PlayWaterDropSound();
                     Destroy(other.gameObject);
                }
             
                    
            }

             // if collision to flower, then play sound, score plus, and call to instantiate small petal particles 
            else if(other.tag=="flower"){
                UIManager.AddScore();
                SoundManagerScript.PlayScoreSound();
                particleBrustManager.showFlowerParticles();
                 Destroy(other.gameObject);
            }

              // if collision to flower, then play sound, score plus, and call to instantiate small petal particles 
            else if(other.tag=="lifeUp"){
                healthBar.IncreaseHealth();
                SoundManagerScript.PlayLifeUpSound();
                particleBrustManager.showLifeUpParticles();
                Destroy(other.gameObject);
            }else  if(other.tag=="magnet"){
                Destroy(other.gameObject);
                magnetArea.SetActive(true);
                magnetActivated=true;
                Timer = 5;
                UIManager.ShowTimerUI(true);

            }else if(other.tag=="bomb"){
                gameStatus = "gameOver";
                UIManager.ShowGameOverUI();
                SoundManagerScript.PlayExplodeSound();
                particleBrustManager.showBombParticles();
                healthBar.Dead();
                 SoundManagerScript.PlayDeadSound();
                Destroy(other.gameObject);
            }else if(other.tag=="spidernet"){
                gameStatus = "gameOver";
                UIManager.ShowGameOverUI();
                Vector3 newPosition = new Vector3(other.gameObject.transform.position.x,other.gameObject.transform.position.y,10.0f);
                player.gameObject.transform.parent = other.gameObject.transform;
                player.gameObject.transform.position = newPosition;
                other.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0.0f;
                other.gameObject.GetComponent<Rigidbody2D>().velocity=Vector2.zero;
                healthBar.Dead();
                 SoundManagerScript.PlayDeadSound();
            }
            
        
        }
    
}

   
    
}
