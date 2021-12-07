using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{

    public Cinemachine.CinemachineVirtualCamera cvCam;
    public Camera mainCamera;

    public Image playerLifeImg;

    public Canvas myCanvas;

    [SyncVar]
    public string playerId;

    bool knockback = false;
    float knockbackTime;

    public float health;
    public float maxHealth;

    public float speed;

    public float atkDelay;
    
    [SyncVar]
    public float countAtkDelay;

    Rigidbody2D rb2D;

    Animator animator;


    float lastX, lastY;
    float hAxis, vAxis;

    // Start is called before the first frame update
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        if (isLocalPlayer) {
            playerId = "Player" + NetworkClient.localPlayer.netId;
            gameObject.name = playerId;
            cvCam.gameObject.SetActive(true);
            mainCamera.gameObject.SetActive(true);
            myCanvas.gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer) { 
            hAxis = Input.GetAxis("Horizontal");
            vAxis = Input.GetAxis("Vertical");

            if (Mathf.Abs(hAxis) >= 0.000000001) {
                lastX = hAxis;
                lastY = 0;
            }
            if (Mathf.Abs(vAxis) >= 0.000000001)
            {
                lastY = vAxis;
                lastX = 0;
            }
        
            if (countAtkDelay > atkDelay)
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    countAtkDelay = 0;
                    if (lastX > 0)
                    {
                        PlayAnimation("atk-right");
                    }
                    else if (lastX < 0)
                    {
                        PlayAnimation("atk-left");
                    }
                    else if (lastY > 0)
                    {
                        PlayAnimation("atk-up");
                    }
                    else if (lastY < 0)
                    {
                        PlayAnimation("atk-down");
                    }
                }
            }
            else {
                countAtkDelay += Time.fixedDeltaTime;
            }

            if (knockback == true) {
                knockbackTime += Time.fixedDeltaTime;
                if (knockbackTime > 1) {
                    knockback = false;
                    knockbackTime = 0;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (isLocalPlayer) { 
            Vector2 newPos = new Vector2(hAxis, vAxis).normalized * speed * Time.fixedDeltaTime;
            if(knockback == false) { 
                rb2D.velocity = newPos;
        
                animator.SetFloat("Y", lastY);

                animator.SetFloat("X", lastX);
            }
        }
    }

    string currentAnimation;
    void PlayAnimation(string animation) {
        if (currentAnimation == animation)
            return;
        currentAnimation = animation;
        animator.Play(currentAnimation);
    }

    public void PlayMovimentoAnimation() {
        PlayAnimation("Movimento");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "AtkPlayer")
        {
            //Knockback(collision.gameObject.transform.position);
            CMDDamage(gameObject);
        }
    }

    [Command]
    void CMDDamage(GameObject target) {
        NetworkIdentity identidadeOponente = target.GetComponent<NetworkIdentity>();
        RPCDamage(identidadeOponente.connectionToClient);
    }

    [TargetRpc]
    public void RPCDamage(NetworkConnection target) {
        if (!isServer) return;
        PlayerController playerTarget = target.identity.GetComponent<PlayerController>();
        playerTarget.TakeDamage(1);
    }

    public void TakeDamage(int amount) {
        health -= amount;
        playerLifeImg.fillAmount = health / maxHealth;
        if (health < 1)
        {
            PlayAnimation("Death");
        }
    }

    public void DestroyGameObject() {
        gameObject.SetActive(false);
    }



    void Knockback(Vector3 pos) {
        knockback = true;
        Vector2 dir = transform.position - pos;
        Debug.DrawRay(pos,dir,Color.magenta,3);
        rb2D.AddForce(dir * 100);
    }

}
