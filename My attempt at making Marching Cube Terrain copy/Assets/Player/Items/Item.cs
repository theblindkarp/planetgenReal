using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{

    //Item Info
    new public string name = "New Item";
    public Sprite icon = null;
    public bool isDefaultItem = false;
    public GameObject itemObject; //For the ACTUAL item object

    public int xSize;
    public int ySize;

    //Crafting Requirements
    public RequiredItemsToCraft[] requiredItemsToCraft;

    public virtual void Use()
    {
        //Debug.Log("Using " + name);
    }

    //public class RequiredItem
    //{
    //    public Item reqItem;
    //    public int itemAmount;
    //}
}
