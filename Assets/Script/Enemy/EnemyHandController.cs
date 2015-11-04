using UnityEngine;
using System.Collections;

/* Codigo para controla a mao do inimigo
Sempre q o inimigo chamar um animacao nova chama uma funcao dessa */

public class EnemyHandController : MonoBehaviour
{
    public Animator animHand;

    public void RunHand()
    {
        animHand.SetTrigger("Run");
    }

    public void DanoHand()
    {
        animHand.SetTrigger("Dano");
    }

    public void DefesaHand()
    {
        animHand.SetTrigger("Defesa");
    }

    public void PAttackHand()
    {
        animHand.SetTrigger("Attack");
    }

    public void SprawlHand()
    {
        animHand.SetTrigger("Sprawl");
    }

    public void KillHand()
    {
        animHand.SetTrigger("Kill");
    }

    public void AttackHand()
    {
        animHand.SetTrigger("Attack");
    }

    public void IdleHand()
    {
        animHand.SetTrigger("Idle");
    }

    public void JumpHand()
    {
        animHand.SetTrigger("Jump");
    }
}