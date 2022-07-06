using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "Building")]
public class Building : ScriptableObject
{

    //Item Info
    new public string name = "New Item";
    public Sprite icon = null;
    public GameObject itemObject; //For the ACTUAL item object

    //Crafting Requirements
    public RequiredItemsToCraft[] requiredItemsToCraft;

    //public class RequiredItem
    //{
    //    public Item reqItem;
    //    public int itemAmount;
    //}
}
