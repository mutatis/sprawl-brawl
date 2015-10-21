using UnityEngine;
using System.Collections;

/* Codigo q toma conta do PLayer */

public class PlayerController : MonoBehaviour 
{
	public static PlayerController player;

    public PlayerAudioController audioController;

	public Animator anim;

	public Rigidbody2D rig;
    
	public GameObject obj;

    public bool isEsquiva;
	public bool stop;
	public bool fight;
	public bool prepareAttack;
	public bool esquiva;
    public bool isAttack = true;

    public int life;

	public float velX = 3;

	float temp;
    float dist;

    Enemy enemy;

	bool attackPower;
	bool attack;
    bool delayEsquiva;

	public float lifeEnemy;

	void Awake()
	{
		player = this;
	}

	void Start()
	{
		temp = velX;
		anim.SetFloat ("VelX", velX);
	}

	void Update ()
	{
        /*if (obj != null)
        {
            dist = Vector3.Distance(transform.position, obj.transform.position);
            if(dist < 1.5f)
            {
                obj.GetComponent<Enemy>().obj = null;
                obj = null;
            }
        }*/

        if(obj != null)
        {
            velX = 0;
			lifeEnemy = obj.GetComponent<Enemy>().life;
        }

        if (rig.velocity.x < 0)
		{
			rig.velocity = new Vector2((rig.velocity.x + 0.05f), 0);
		}
		else
		{
			rig.velocity = new Vector2(0, 0);
		}

		if(life <= 0)
		{
			Morreu();
		}

		if(!esquiva && !attack && !attackPower && !stop)
		{
			velX = temp;
			anim.SetTrigger("Run");
            if (obj == null)
            {
                transform.Translate(velX * Time.deltaTime, 0, 0);
            }
		}
        else
        {
            velX = 0;
        }

        if (isAttack)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) && !esquiva && !attack)
            {
                velX = 0;
                prepareAttack = true;
                StartCoroutine("HeavyAttack");
            }
            else if(Input.GetKey(KeyCode.RightArrow))
            {

            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && !attack && !delayEsquiva && !prepareAttack)
            {
                isAttack = false;
                Defesa();
                anim.SetTrigger("Base");
                esquiva = true;
                delayEsquiva = true;
                StartCoroutine("Delay");
				StopCoroutine("HeavyAttack");
                audioController.Sprawl();
            }
            else if (Input.GetKeyUp(KeyCode.RightArrow) && !esquiva && !attackPower)
            {
                isAttack = false;
                prepareAttack = false;
                if (!fight)
                {
                    stop = false;
                }
                if(obj == null)
                {
                   // audioController.Soco();
                }
                Attack();
                anim.SetTrigger("Attack");
                attack = true;
                StopCoroutine("HeavyAttack");
            }
            /*else if(fight)
		    {
			    velX = temp / 2;
			    anim.SetTrigger("Run");
			    transform.Translate(velX * Time.deltaTime, 0, 0);
		    }*/
        }

        // se mudar de ideia sobre o ataca forte ta aqui sem ser automatico;
		/*if(Input.GetKeyUp(KeyCode.RightArrow))
		{
			if(!fight)
			{
				stop = false;
            }
			if(attackPower)
			{
				AttackF();
				anim.SetTrigger("HeavyAttack");
				StopCoroutine ("HeavyAttack");
				attackPower = false;
			}
		}*/
	}

	//temp  o de intervalo entre a esquiva para nao ficar usando direto
    IEnumerator Delay()
    {
        yield return new WaitForSeconds(2f);
        delayEsquiva = false;
    }

	//tomo dano	
    public void Dano()
    {
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        StartCoroutine("Normal");
    }

	//fim do dano
    IEnumerator Normal()
    {
        yield return new WaitForSeconds(0.3f);
        gameObject.GetComponent<SpriteRenderer>().color = Color.white;
    }

	//nao ataca enquanto toma dano
    public void StopDano()
    {
        isAttack = false;
    }

	//morreu
	void Morreu()
	{
		Application.LoadLevel("GameOver");
	}

	//entro em combate nao vai pra frente
	public void StopPlayer()
	{
		velX = 0;
		anim.SetFloat ("VelX", velX);
		fight = true;
		anim.SetTrigger("Idle");
		stop = true;
	}

	//saiu do combate anda normal
	public void ReturnPlayerMov()
	{
		velX = temp;
		anim.SetFloat ("VelX", velX);
		fight = false;
		anim.SetTrigger("Run");
		stop = false;
	}

	//entro em combate
	public void IsFight()
	{
		if(fight)
		{
			anim.SetTrigger("Idle");
		}
	}

	//soco normal
	void Attack()
	{
		if(obj != null)
		{
			enemy = obj.GetComponent<Enemy> ();
			if(enemy.selectAttack != 2)
            {
                enemy.anim.SetTrigger("Dano");
                enemy.life -= 2;
			}
            else if(enemy.selectAttack == 2)
            {
                audioController.BlockEnemy();
            }
		}
	}

    public void Soquei()
    {
        //verifica se pode dar dano
        if (obj != null)
        {
            if (enemy.selectAttack != 2)
            {
                enemy.Dano();
            }
        }
    }

	//chute forte
	void AttackF()
	{
		if(obj != null)
		{
			enemy = obj.GetComponent<Enemy> ();
            //se der soco forte e nao esta defendendo, da  dano normal;
			if(enemy.selectAttack == 1)
			{
				enemy.Dano();
				enemy.life -= 4;
			}
            //se der soco forte com a defesa, dano reduzido mas da stun;
            else if(enemy.selectAttack == 2)
            {
                enemy.Stun();
                enemy.life -= 1;
            }
            else
            {
                enemy.Stun();
                enemy.life -=4;
            }
		}
	}

	//defendeu
	void Defesa()
	{
		velX = 0;
		anim.SetFloat ("VelX", velX);
	}

	//tempo de carga do chute forte
	IEnumerator HeavyAttack()
	{
        yield return new WaitForSeconds(0.2f);
        anim.SetTrigger("Carrega");
		yield return new WaitForSeconds (0.8f);
		prepareAttack = false;
		attackPower = true;
		yield return new WaitForSeconds(0.5f);
        //temporario
        AttackF();
		anim.SetTrigger("HeavyAttack");
		StopCoroutine ("HeavyAttack");
		attackPower = false;
    }

	//paro chute forte no meio
	public void StopHeavyAttack()
    {
        attackPower = false;
        isAttack = true;
    }

	public void StopAttack()
	{
		attack = false;
        isAttack = true;
    }

	//esquiva com sucesso
	public void Esquivei()
	{
        isAttack = true;
        velX = temp;
		anim.SetFloat ("VelX", velX);
		esquiva = false;
	}

    void OnTriggerEnter2D (Collider2D other)
    {
        if(other.gameObject.tag == "Enemy")
        {
            obj = other.gameObject;
            other.gameObject.GetComponent<Enemy>().obj = gameObject;
			if(obj.GetComponent<Enemy>().escolha != 0 && obj.GetComponent<Enemy>().selectTakedown != 0)
			{
				obj.GetComponent<Enemy>().Combat();
			}
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            obj = null;
            other.gameObject.GetComponent<Enemy>().obj = null;
        }
    }
}