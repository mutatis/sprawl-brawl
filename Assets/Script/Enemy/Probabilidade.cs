using UnityEngine;
using System.Collections;

public class Probabilidade : MonoBehaviour
{
    //takedown
    public Takedown[] takedown;

    //attack
    public Attack[] attack;

    //escolhe attack
    public int ChooseAttack()
    {
        float total = 0;
        int i = 0;
        foreach(Attack elem in attack)
        {
            total += elem.probalidade;
        }

        float randomPoint = Random.value * total;

        for(i = 0; i < attack.Length; i++)
        {
            if (randomPoint < attack[i].probalidade)
                return i;
            else
                randomPoint -= attack[i].probalidade;
        }

        return attack.Length - 1;
    }

    //escolhe takedown
    public int ChooseTakedown()
    {
        float total = 0;
        int i = 0;
        foreach (Takedown elem in takedown)
        {
            total += elem.probability;
        }

        float randomPoint = Random.value * total;

        for (i = 0; i < takedown.Length; i++)
        {
            if (randomPoint < takedown[i].probability)
                return i;
            else
                randomPoint -= takedown[i].probability;
        }

        return takedown.Length - 1;
    }
}

[System.Serializable]
public class Takedown
{
    public string takedown;
    [Range(0, 100)]
    public float probability;
}

[System.Serializable]
public class Attack
{
    public string attack;
    [Range(0, 100)]
    public float probalidade;
}