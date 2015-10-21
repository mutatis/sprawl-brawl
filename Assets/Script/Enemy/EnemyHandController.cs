using UnityEngine;
using System.Collections;

/* Codigo para controla a mao do inimigo
Sempre q o inimigo chamar um animacao nova chama uma funcao dessa */

public class EnemyHandController : MonoBehaviour
{
    public Animator anim;

    public void RunHand()
    {
        anim.SetTrigger("Run");
    }

    public void DanoHand()
    {
        anim.SetTrigger("Dano");
    }

    public void DefesaHand()
    {
        anim.SetTrigger("Defesa");
    }

    public void PAttackHand()
    {
        anim.SetTrigger("Pattack");
    }

    public void SprawlHand()
    {
        anim.SetTrigger("Sprawl");
    }

    public void KillHand()
    {
        anim.SetTrigger("Kill");
    }

    public void AttackHand()
    {
        anim.SetTrigger("Attack");
    }

    public void IdleHand()
    {
        anim.SetTrigger("Idle");
    }

    public void JumpHand()
    {
        anim.SetTrigger("Jump");
    }
}