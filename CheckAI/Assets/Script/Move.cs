using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    //public enum Priority { NONE, SUCCES};

    public int x;
    public int y;
    public bool success;
    public int removeX;
    public int removeY;
    public PieceDraughts piece;
    public bool NextRemove = false;
}
