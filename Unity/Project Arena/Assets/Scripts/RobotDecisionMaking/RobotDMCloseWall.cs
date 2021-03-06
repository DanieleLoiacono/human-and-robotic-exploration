﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class defines a decision making process where the point chosen is the one belonging to the closest frontier to the player.
/// The point is the closest one to a wall
/// </summary>
public class RobotDMCloseWall : RobotDecisionMaking {

    private float distance;
    private float tempDistance;
    //private float wallX;
    //private float wallZ;

    private int frontierXCoord;
    private int frontierZcoord;
    private int index;

    private List<Vector3> closestWall = new List<Vector3>();
    private List<Vector3> closestFrontier = new List<Vector3>();
    private List<Vector3> currentZone;
    //private List<Vector3> frontier;
    private List<Vector3> possibleCloseWall = new List<Vector3>();
    private List<Vector3> possibleFrontierCell;
    private List<List<Vector3>> possibleFrontier = new List<List<Vector3>>();
    private List<Vector3> wallFound;
    private List<Vector3> wallList;
    private List<Vector3> newPossibleFrontierCell = new List<Vector3>();
    private List<List<Vector3>> filteredFrontierZones = new List<List<Vector3>>();
    private List<List<Vector3>> frontierZones;


    private Vector3 frontierCell;
    private Vector3 wallCell;

    protected void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
    }

    /// <summary>
    /// This method chooses the frontier point that the robot agent has to reach do advance in exploration
    /// </summary>
    /// <param name="listFrontierPoints">The list of the possible frontier points reacheable</param>
    /// <returns></returns>
    public override Vector3 PosToReach(List<Vector3> listFrontierPoints)
    {
        //for (int i = 0; i < listFrontierPoints.Count; i++)
        //{
        //    Debug.Log(listFrontierPoints[i].x.ToString() + ", " + listFrontierPoints[i].z.ToString());
        //}

        //Debug.Log(listFrontierPoints.Count);
        frontierZones = new List<List<Vector3>>();

        //obatining position of the wall close to the player
        closestWall = CalculatingClosestWall();

        //defining frontier zones
        DefiningFrontierZones(listFrontierPoints);

        FixingOverlappingZones();

        //removing frontier zones considered too little to be taken in consideration
        RemovingUselessFrontierZones();
        //Debug.Log(frontierZones.Count);

        //Debug.Log(filteredFrontierZones.Count);

        if (filteredFrontierZones.Count > 0)
        {
            frontierZones = filteredFrontierZones;
        }

        //only for testing purpose
        for (int i = 0; i < frontierZones.Count; i++)
        {
            Debug.Log(frontierZones[i].Count);
        }

        //choosing the frontier zone closer to the selected wall
        closestFrontier = CalculatingClosestFrontier();

        //choosing the frontier point (of the chosen zone) closer to the chosen wall
        //for each point of the frontier, we inspect each point of the wall and check the distance. The frontier point which stand the least, is the chosen one
        possibleFrontierCell = new List<Vector3>()
        {
            closestFrontier[0]
        };
        //Debug.Log(frontierCell);
        distance = Mathf.Sqrt((closestFrontier[0].x - closestWall[0].x) * (closestFrontier[0].x - closestWall[0].x) + (closestFrontier[0].z - closestWall[0].z) * (closestFrontier[0].z - closestWall[0].z));
        for (int i = 1; i < closestFrontier.Count; i++)
        {
            //find the closest one
            for (int j = 0; j < closestWall.Count; j++)
            {
                tempDistance = Mathf.Sqrt( (closestFrontier[i].x - closestWall[j].x)*(closestFrontier[i].x - closestWall[j].x) + (closestFrontier[i].z - closestWall[j].z)*(closestFrontier[i].z - closestWall[j].z) );
                if (distance > tempDistance)
                {
                    distance = tempDistance;
                    possibleFrontierCell = new List<Vector3>()
                    {
                        closestFrontier[i]
                    };
                }
                else if (tempDistance == distance)
                {
                    possibleFrontierCell.Add(closestFrontier[i]);
                }
            }
        }

        if (possibleFrontierCell.Count == 1)//returning the point
        {
            //Debug.Log("E' uno");
            return possibleFrontierCell[0];
        }
        else //return the least distant frontier point from the agent
        {
            //Debug.Log("Sono di più");
            frontierCell = possibleFrontierCell[0];
            distance = Mathf.Sqrt( (possibleFrontierCell[0].x - transform.position.x)*(possibleFrontierCell[0].x - transform.position.x) + (possibleFrontierCell[0].z - transform.position.z)*(possibleFrontierCell[0].z - transform.position.z) );
            newPossibleFrontierCell.Clear();
            newPossibleFrontierCell.Add(frontierCell);
            for (int i = 1; i < possibleFrontierCell.Count; i++)
            {
                tempDistance = Mathf.Sqrt( (possibleFrontierCell[i].x - transform.position.x)*(possibleFrontierCell[i].x - transform.position.x) + (possibleFrontierCell[i].z - transform.position.z)*(possibleFrontierCell[i].z - transform.position.z) );
                if (tempDistance < distance)
                {
                    distance = tempDistance;
                    frontierCell = possibleFrontierCell[i];
                    newPossibleFrontierCell.Clear();
                    newPossibleFrontierCell.Add(possibleFrontierCell[i]);
                }
                else if (tempDistance == distance)
                {
                    newPossibleFrontierCell.Add(possibleFrontierCell[i]);
                }
                //Debug.Log(newPossibleFrontierCell.Count);
            }

            index = Random.Range(0, newPossibleFrontierCell.Count);
            //Debug.Log(index + ", " + newPossibleFrontierCell.Count);
            return newPossibleFrontierCell[index];
        }
    }

    /// <summary>
    /// This method returns the closest wall to the robot agent
    /// </summary>
    /// <returns></returns>
    private List<Vector3> CalculatingClosestWall()
    {
        wallFound = new List<Vector3>();
        wallList = new List<Vector3>();

        for (int i = 0; i < numeric_map.GetLength(0) ; i++)
        {
            for (int j = 0; j < numeric_map.GetLength(1); j++)
            {
                if (numeric_map[i,j] == 1f) 
                {
                    wallList.Add(new Vector3(squareSize * i, transform.position.y, squareSize * j));
                }
            }
        }

        wallCell = wallList[0];
        distance = Mathf.Sqrt( (transform.position.x - wallCell.x)*(transform.position.x - wallCell.x) + (transform.position.z - wallCell.z)*(transform.position.z - wallCell.z) );
        possibleCloseWall.Clear();
        possibleCloseWall.Add(wallCell);

        for (int i = 0; i < wallList.Count; i++)
        {
            tempDistance = Mathf.Sqrt((transform.position.x - wallList[i].x) * (transform.position.x - wallList[i].x) + (transform.position.z - wallList[i].z) * (transform.position.z - wallList[i].z));
            if (tempDistance < distance)
            {
                wallCell = wallList[i];
                distance = tempDistance;
                possibleCloseWall.Clear();
                possibleCloseWall.Add(wallList[i]);
            }
            else if (tempDistance == distance)
            {
                possibleCloseWall.Add(wallList[i]);
            }
        }

        if (possibleCloseWall.Count == 1)
        {
            wallCell = possibleCloseWall[0];
        }
        else
        {
            //Debug.Log(possibleCloseWall.Count);
            index = Random.Range(0, possibleCloseWall.Count);
            wallCell = possibleCloseWall[index];
        }

        wallFound.Add(wallCell);

        AddNeighbourWalls(wallCell, wallFound);

        return wallFound;
    }

    /// <summary>
    /// This method groups up frontier points into separated zones, called frontier zones
    /// </summary>
    /// <param name="frontierPoints"></param>
    private void DefiningFrontierZones(List<Vector3> frontierPoints)
    {
        currentZone = new List<Vector3>();
        for (int i = 0; i < frontierPoints.Count; i++)
        {
            //if (frontierZones.Count == 0 || !currentZone.Contains(frontierPoints[i])) //la seconda condizione non va bene
            if(frontierZones.Count == 0 || !PosAlreadyExplored(frontierPoints[i]))
            {
                currentZone = new List<Vector3>()
                {
                    frontierPoints[i]
                };
                //currentZone.Add(frontierPoints[i]);
                frontierZones.Add(currentZone);

                AddNeighbourFrontier(frontierPoints[i], currentZone, frontierPoints);
            }
        }
    }

    private void FixingOverlappingZones()
    {
        bool pointFound = false;
        List<List<Vector3>> fixedZones = new List<List<Vector3>>();

        foreach (List<Vector3> zone in frontierZones)
        {
            if(fixedZones.Count == 0)
            {
                fixedZones.Add(zone);
            }
            else
            {
                for (int a = 0; a < zone.Count; a++)
                {
                    if (!pointFound)
                    {
                        foreach (List<Vector3> newZone in fixedZones)
                        {
                            if (newZone.Contains(zone[a]))
                            {
                                pointFound = true;
                                MergingZones(newZone, zone);
                            }
                        }
                    }
                }

                if (!pointFound)
                {
                    fixedZones.Add(zone);
                }
                else pointFound = false;
            }
        }

        frontierZones = fixedZones;
    }

    private void MergingZones(List<Vector3> newZone, List<Vector3> oldZone)
    {
        foreach (Vector3 point in oldZone)
        {
            if (!newZone.Contains(point))
            {
                newZone.Add(point);
            }
        }
    }

    /// <summary>
    /// This method removes frontier zones considered too little to be taken in consideration
    /// </summary>
    private void RemovingUselessFrontierZones()
    {
        filteredFrontierZones.Clear();

        for (int i = 0; i < frontierZones.Count; i++)
        {
            if (frontierZones[i].Count > 1 || (frontierZones[i].Count == 1 && CheckingSuspiciousFrontier(frontierZones[i][0])))
            {
                filteredFrontierZones.Add(frontierZones[i]);
            }
        }

        //frontierZones = filteredFrontierZones;
    }

    private bool CheckingSuspiciousFrontier(Vector3 frontierPoint)
    {
        if ((numeric_map[(int)FixingRound(frontierPoint.x/squareSize), (int)FixingRound(frontierPoint.z/squareSize) - 1] == 0 || numeric_map[(int)FixingRound(frontierPoint.x / squareSize) - 1, (int)FixingRound(frontierPoint.z / squareSize)] == 0)
            || (numeric_map[(int)FixingRound(frontierPoint.x / squareSize), (int)FixingRound(frontierPoint.z / squareSize) - 1] == 0 || numeric_map[(int)FixingRound(frontierPoint.x / squareSize) + 1, (int)FixingRound(frontierPoint.z / squareSize)] == 0)
            || (numeric_map[(int)FixingRound(frontierPoint.x / squareSize), (int)FixingRound(frontierPoint.z / squareSize) + 1] == 0 || numeric_map[(int)FixingRound(frontierPoint.x / squareSize) - 1, (int)FixingRound(frontierPoint.z / squareSize)] == 0)
            || (numeric_map[(int)FixingRound(frontierPoint.x / squareSize), (int)FixingRound(frontierPoint.z / squareSize) + 1] == 0 || numeric_map[(int)FixingRound(frontierPoint.x / squareSize) + 1, (int)FixingRound(frontierPoint.z / squareSize)] == 0))
        {
            return true;
        } else return false;
    }

    /// <summary>
    /// This method returns, between all the frontier zones, the one that is closer to the robot agent
    /// </summary>
    /// <returns></returns>
    private List<Vector3> CalculatingClosestFrontier()
    {
        possibleFrontier.Clear();
        possibleFrontier.Add(frontierZones[0]);
        distance = Mathf.Sqrt( (GetXCoordinateFrontier(frontierZones[0]) - transform.position.x) * (GetXCoordinateFrontier(frontierZones[0]) - transform.position.x) + (GetZCoordinateFrontier(frontierZones[0]) - transform.position.z) * (GetZCoordinateFrontier(frontierZones[0]) - transform.position.z) );
        for (int i = 0; i < frontierZones.Count; i++)
        {
            tempDistance = Mathf.Sqrt((GetXCoordinateFrontier(frontierZones[i]) - transform.position.x) * (GetXCoordinateFrontier(frontierZones[i]) - transform.position.x) + (GetZCoordinateFrontier(frontierZones[i]) - transform.position.z) * (GetZCoordinateFrontier(frontierZones[i]) - transform.position.z));
            if (tempDistance < distance)
            {
                distance = tempDistance;
                possibleFrontier.Clear();
                possibleFrontier.Add(frontierZones[i]);
            }
            else if (tempDistance == distance)
            {
                possibleFrontier.Add(frontierZones[i]);
            }
        }

        Debug.Log("Num frontiers closest: " + possibleFrontier.Count);

        if (possibleFrontier.Count == 1)
        {
            return possibleFrontier[0];
        }
        else
        {
            index = Random.Range(0, possibleFrontier.Count);
            return possibleFrontier[index];
        }
    }

    /// <summary>
    /// This method checks if a frontier point is close to another one. If so, this is added in the list containing the former
    /// </summary>
    /// <param name="frontierPoint">The frontier point used to check if neighbours frontier exist</param>
    /// <param name="frontierZone">The frontier zone containing the frontier point</param>
    /// <param name="frontierPoints">The list of all frontier points</param>
    private void AddNeighbourFrontier(Vector3 frontierPoint, List<Vector3> frontierZone, List<Vector3> frontierPoints)
    {
        //List<Vector3> openList = frontierZone;
        frontierXCoord = (int)FixingRound(frontierPoint.x / squareSize);
        frontierZcoord = (int)FixingRound(frontierPoint.z / squareSize);

        //Debug.Log(frontierPoint.x / squareSize);
        //Debug.Log(frontierPoint.z / squareSize);
        //Debug.Log(x);

        
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (frontierZcoord + j >= 0 && frontierZcoord + j <= numeric_map.GetLength(1) - 1 && frontierXCoord + i >= 0 && frontierXCoord + i <= numeric_map.GetLength(0) - 1)
                {
                    //Debug.Log(((squareSize * i) + frontierPoint.x).ToString() + ", " + ((squareSize * j) + frontierPoint.z).ToString());
                    if (numeric_map[frontierXCoord + i, frontierZcoord + j] == 0 && !frontierZone.Contains(new Vector3( (squareSize*i) + frontierPoint.x, transform.position.y, (squareSize*j) + frontierPoint.z))  && frontierPoints.Contains(new Vector3( (squareSize*i) + frontierPoint.x, transform.position.y, (squareSize*j) + frontierPoint.z)))
                    {
                        //Debug.Log("Addedd");
                        Vector3 newFrontPoint = new Vector3((squareSize * i) + frontierPoint.x, transform.position.y, (squareSize * j) + frontierPoint.z);
                        frontierZone.Add(newFrontPoint);
                        //Debug.Log(newFrontPoint);
                        AddNeighbourFrontier(newFrontPoint, frontierZone, frontierPoints);
                    }
                }
            }
        }
        

        /*
        while (openList.Count > 0)
        {
            openList.Remove(frontierPoint);
            frontierZone.Add(frontierPoint);

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (frontierZcoord + j >= 0 && frontierZcoord + j <= numeric_map.GetLength(1) - 1 && frontierXCoord + i >= 0 && frontierXCoord + i <= numeric_map.GetLength(0) - 1)
                    {
                        //Debug.Log(numeric_map[frontierXCoord + 1, frontierZcoord + j]);

                        if (numeric_map[frontierXCoord + i, frontierZcoord + j] == 0 && !frontierZone.Contains(new Vector3((squareSize * i) + frontierPoint.x, transform.position.y, (squareSize * j) + frontierPoint.z)) && frontierPoints.Contains(new Vector3((squareSize*i) + frontierPoint.x, transform.position.y, (squareSize*j) + frontierPoint.z)))
                        {
                            Vector3 newFrontPoint = new Vector3((squareSize * i) + frontierPoint.x, transform.position.y, (squareSize * j) + frontierPoint.z);
                            openList.Add(newFrontPoint);
                        }
                    }
                }
            }

            if (openList.Count > 0)
            {
                frontierPoint = openList[0];
                frontierXCoord = (int)FixingRound(frontierPoint.x/squareSize);
                frontierZcoord = (int)FixingRound(frontierPoint.z/squareSize);
            }
        }
        */

    }

    /// <summary>
    /// This method checks if a wall point is close to another one. If so, this is added in the list containing the former
    /// </summary>
    /// <param name="cell">The wall cell analyzed</param>
    /// <param name="wall">The list of all wall points</param>
    private void AddNeighbourWalls(Vector3 cell, List<Vector3> wall)
    {
        int x, z;
        for (int i = -1; i < 2; i++)
        {
            x = (int)FixingRound(cell.x / squareSize);
            z = (int)FixingRound(cell.z / squareSize);
            if (x + i >= 0 && x + i <= numeric_map.GetLength(0) - 1)
            {
                if (numeric_map[x + i, z] == 1 && !wall.Contains(new Vector3(squareSize * (x + i), transform.position.y, squareSize * z)))
                {
                    Vector3 newWall = new Vector3(squareSize * (x + i), transform.position.y, squareSize * z);
                    wall.Add(newWall);
                    AddNeighbourWalls(newWall, wall);
                }
            }
        }

        for (int j = -1; j < 2; j++)
        {
            x = (int)FixingRound(cell.x / squareSize);
            z = (int)FixingRound(cell.z / squareSize);
            if (z + j >= 0 && z + j <= numeric_map.GetLength(1) -1)
            {
                if (numeric_map[x, z + j] == 1 && !wall.Contains(new Vector3(squareSize * x, transform.position.y, squareSize * (z + j))))
                {
                    Vector3 newWall = new Vector3(squareSize * x, transform.position.y, squareSize * (z + j));
                    wall.Add(newWall);
                    AddNeighbourWalls(newWall, wall);
                }
            }
        }
    }

    /// <summary>
    /// This method checks if a frontier point has already been added in a frontier zone
    /// </summary>
    /// <param name="frontierPoint">The frontier point analyzed</param>
    /// <returns></returns>
    private bool PosAlreadyExplored(Vector3 frontierPoint)
    {
        for (int i = 0; i < frontierZones.Count; i++)
        {
            for (int j = 0; j < frontierZones[i].Count; j++)
            {
                if( (int)FixingRound(frontierZones[i][j].x/squareSize) == (int)FixingRound(frontierPoint.x/squareSize) 
                    && (int)FixingRound(frontierZones[i][j].z/squareSize) == (int)FixingRound(frontierPoint.z/squareSize))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private float GetXCoordinateWall(List<Vector3> wall) 
    {
        float x = 0;
        for (int i = 0; i < wall.Count; i++)
        {
            x = x + wall[i].x;
        }

        x = x / wall.Count;

        return x;
    }

    private float GetXCoordinateFrontier(List<Vector3> frontier)
    {
        float x = 0;
        for (int i = 0; i < frontier.Count; i++)
        {
            x = x + frontier[i].x;
        }

        x = x / frontier.Count;

        return x;
    }

    private float GetZCoordinateWall(List<Vector3> wall)
    {
        float z = 0;
        for (int i = 0; i < wall.Count; i++)
        {
            z = z + wall[i].z;
        }

        z = z / wall.Count;

        return z;
    }

    private float GetZCoordinateFrontier(List<Vector3> frontier)
    {
        float z = 0;
        for (int i = 0; i < frontier.Count; i++)
        {
            z = z + frontier[i].z;
        }

        z = z / frontier.Count;

        return z;
    }

    /// <summary>
    /// This method corrects the round of coordinates of a tile. In this way is avoided to assign values to undesired tiles (for example, a wall tile as a free one)
    /// </summary>
    /// <param name="coordinate">The coordinate of a tile</param>
    /// <returns></returns>
    private float FixingRound(float coordinate)
    {
        if (Mathf.Abs(coordinate - Mathf.Round(coordinate)) >= 0.5f)
        {
            return (Mathf.Round(coordinate) - 1);
        }
        else return Mathf.Round(coordinate);
    }

}
