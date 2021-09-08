using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public sealed class GameEnvironment
{
    private static GameEnvironment instance;
    private List<GameObject> checkPoints = new List<GameObject>();
    public List<GameObject> CheckPoints { get { return checkPoints; } }

    public static GameEnvironment Singleton
    {
        get
        {
            if(instance == null)
            {
                instance = new GameEnvironment();
                instance.CheckPoints.AddRange(GameObject.FindGameObjectsWithTag("navpoint"));

                instance.checkPoints = instance.checkPoints.OrderBy(waypoint => waypoint.name).ToList();
            }
            return instance;
        }
    }
}
