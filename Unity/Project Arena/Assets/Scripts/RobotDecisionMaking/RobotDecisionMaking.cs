﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class defines the common elements for every decision making component in each robot agent
/// </summary>
public abstract class RobotDecisionMaking : MonoBehaviour {

    protected char[,] char_map;
    protected float[,] numeric_map;

    protected Vector3 target;
    protected float squareSize;

    public abstract Vector3 PosToReach(List<Vector3> listFrontierPoints);

    public void SetSquareSize(float size)
    {
        squareSize = size;
    }

    public void SetNumericMap(float[,] map)
    {
        numeric_map = map;
    }

    public void SetCharMap(char[,] map)
    {
        char_map = map;
    }

}
