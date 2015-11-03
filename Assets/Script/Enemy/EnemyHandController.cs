using UnityEngine;
using System.Collections;

/* Codigo para controla a mao do inimigo
Sempre q o inimigo chamar um animacao nova chama uma funcao dessa */

public class EnemyHandController : MonoBehaviour
{
    public Animator animHand;

    public GameObject obj;

    public void RunHand()
    {
        obj.SetActive(false);
        animHand.SetTrigger("Run");
    }

    public void DanoHand()
    {
        obj.SetActive(true);
        animHand.SetTrigger("Dano");
    }

    public void DefesaHand()
    {
        obj.SetActive(true);
        animHand.SetTrigger("Defesa");
    }

    public void PAttackHand()
    {
        obj.SetActive(false);
        animHand.SetTrigger("Pattack");
    }

    public void SprawlHand()
    {
        obj.SetActive(true);
        animHand.SetTrigger("Sprawl");
    }

    public void KillHand()
    {
        obj.SetActive(true);
        animHand.SetTrigger("Kill");
    }

    public void AttackHand()
    {
        obj.SetActive(false);
        animHand.SetTrigger("Attack");
    }

    public void IdleHand()
    {
        obj.SetActive(true);
        animHand.SetTrigger("Idle");
    }

    public void JumpHand()
    {
        obj.SetActive(true);
        animHand.SetTrigger("Jump");
    }
}