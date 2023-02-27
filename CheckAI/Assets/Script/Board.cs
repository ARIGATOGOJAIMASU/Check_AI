using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    protected int player = 1;

    public virtual Move[] GetMoves(PlayerCheck playerCheck)
    {
        return new Move[0];
    }

    public virtual Board MakeMove(Move m)
    {
        return new Board();
    }

    public virtual bool InGameOver()
    {
        return true;
    }

    public virtual int GetCurrentPlayer()
    {
        return player;
    }

    public virtual float Evaluate(int player)
    {
        return Mathf.NegativeInfinity;
    }

    public virtual float Evaluate()
    {
        return Mathf.NegativeInfinity;
    } 
}
