using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score
{
    public int value;

    public Score(int value)
    {
        this.value = value;
    }

    public void increaseScore(int score)
    {
        value += score;
    }
}