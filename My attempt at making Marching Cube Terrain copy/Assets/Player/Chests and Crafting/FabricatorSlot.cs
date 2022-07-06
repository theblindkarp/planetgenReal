using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FabricatorSlot : MonoBehaviour
{

    //Fabricator Script
    [HideInInspector]
    public Fabricator fabricator;

    //Icon
    public Image icon;
    public Image grayOver;

    //Craftable Item
    public Item craftableItem;

    //Info
    public Text itemName;
    public Text itemInfoText;
    public Text itemCostText;

    //If any items are needed to craft if failure to craft
    public List<MoreItemsNeeded> moreItemsNeeded = new List<MoreItemsNeeded>();
    MoreItemsNeeded exampleItemsNeeded;

    //Crafting Conditions
    public bool canCraftItem; //Can be Crafted

    public void UpdateFabricatorSlots()
    {
        grayOver.enabled = false;

        //Setting it up
        icon.sprite = craftableItem.icon;

        canCraftItem = checkCraftable();
        if(canCraftItem == false)
        {
            grayOver.enabled = true;
        }
    }

    bool checkCraftable()
    {
        moreItemsNeeded = new List<MoreItemsNeeded>();
        exampleItemsNeeded = new MoreItemsNeeded();

        for (int i = 0; i < craftableItem.requiredItemsToCraft.Length; i++)
        {
            moreItemsNeeded.Add(exampleItemsNeeded);
        }

        for (int i = 0; i < craftableItem.requiredItemsToCraft.Length; i++)
        {
            //Resets it
            exampleItemsNeeded = new MoreItemsNeeded();
            //Sets the item that is needed
            exampleItemsNeeded.whatItemisNeeded = craftableItem.requiredItemsToCraft[i].reqItem;
            //Sets the Amount needed
            exampleItemsNeeded.moreAmountNeeded = craftableItem.requiredItemsToCraft[i].itemAmount;
            moreItemsNeeded[i] = exampleItemsNeeded;
        }

        if (craftableItem.requiredItemsToCraft != null)
        {
            for (int o = 0; o < craftableItem.requiredItemsToCraft.Length; o++)
            {
                for (int i = 0; i < fabricator.inventoryItems.Count; i++)
                {
                    //Debug.Log(craftableItem.requiredItemsToCraft[o].reqItem + " and " + fabricator.inventoryItems[i].invItem);

                    //Item required if found
                    if (craftableItem.requiredItemsToCraft[o].reqItem == fabricator.inventoryItems[i].invItem)
                    {
                        //Subtracts the amount that the player needs by the amount the player has
                        moreItemsNeeded[o].moreAmountNeeded = craftableItem.requiredItemsToCraft[o].itemAmount - fabricator.inventoryItems[i].invCount;
                        //Debug.Log(moreItemsNeeded[o].whatItemisNeeded);
                        //Debug.Log(craftableItem.requiredItemsToCraft[o].reqItem);
                    }
                }
            }
        }

        //Makes the List into an Array for Deletion
        MoreItemsNeeded[] tempMoreItemsNeeded = new MoreItemsNeeded[moreItemsNeeded.Count];
        for (int i = 0; i < tempMoreItemsNeeded.Length; i++)
        {
            //Makes it so that I dont get the NULL ERROR AGAIN
            exampleItemsNeeded = new MoreItemsNeeded();
            tempMoreItemsNeeded[i] = exampleItemsNeeded;

            //Converts it
            tempMoreItemsNeeded[i].whatItemisNeeded = moreItemsNeeded[i].whatItemisNeeded;
            tempMoreItemsNeeded[i].moreAmountNeeded = moreItemsNeeded[i].moreAmountNeeded;
        }

        //The actual Deletion Process
        moreItemsNeeded = new List<MoreItemsNeeded>(); //So that it doesnt add to its self again
        for (int i = 0; i < tempMoreItemsNeeded.Length; i++)
        {
            if (tempMoreItemsNeeded[i].moreAmountNeeded > 0)
            {
                //"Removes the things that arent there"
                moreItemsNeeded.Add(tempMoreItemsNeeded[i]);
            }
        }

        for (int i = 0; i < moreItemsNeeded.Count; i++)
        {
            //Debug.Log(moreItemsNeeded[i].whatItemisNeeded + " * " + moreItemsNeeded[i].moreAmountNeeded);
        }

        if (moreItemsNeeded.Count > 0)
        {
            return false;
        }

        return true;
    }

    public void CraftItem()
    {
        if (canCraftItem)
        {
            //Tells the Fabricator what to craft
            fabricator.Craft(this.transform.GetSiblingIndex(), craftableItem);
        }

        UpdateFabricatorSlots();
    }

    [SerializeField]
    public class MoreItemsNeeded
    {
        public Item whatItemisNeeded;
        public int moreAmountNeeded;
    }
}
