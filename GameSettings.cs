using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Scriptable Objects/GameSettings")]
public class GameSettings : ScriptableObject
{
    public int BoardSizeX = 5; 

    public int BoardSizeY = 6;

    public List<GameObject> PizzaType;
    public GameObject Grid;
    public float GridSpacing = 2.0f;

    [Header("Player Progress")]
    public int GoldPerEat = 10;
    public int XPPerPlace = 5;
}
