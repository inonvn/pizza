using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThanhTuu", menuName = "Scriptable Objects/ThanhTuu")]
public class ThanhTuu : ScriptableObject
{
    public List<Thanhtuu> TuuList;
    public List<DayReward> DayRewardList;
}

[System.Serializable]
public class Thanhtuu
{
    public string ID; // Unique identifier for the achievement
    public TypeThanhtuu type;
    public int soLuong;
    public Sprite Icon;
    public string Text;
}
[System.Serializable]
public class DayReward
{
    public Sprite Icon;
    public int SoLuong;
    public TypeReward type;
    
}
public enum TypeReward
{
    coin, itemXoa,ItemDoiLoai
}
public enum TypeThanhtuu
{
    PlaceCount,
    PizzaEat,
    CoinCount,
    LVCount
}