using UnityEngine;

public class HandInventoryUI : MonoBehaviour
{

    public Transform itemsParent;
    public GameObject inventoryUI;

    Inventory inventory;

    InventorySlot[] slots;

    public int inventoryIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        inventory = Inventory.instance;
        inventory.onItemChangedCallBack += UpdateUI;

        slots = itemsParent.GetComponentsInChildren<InventorySlot>();

        //For setting the Inventory Index
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].inventoryIndex = inventoryIndex;
        }
    }

    public void UsingItems(int handSlot)
    {
        slots[handSlot].UseItem();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void UpdateUI()
    {
        //For the other Inventories
        for (int i = 0; i < inventory.handItems.Length; i++)
        {
            if (inventory.handItems[i] != null)
            {
                //This converts a 2D Array into a 1D List by adding the multiplied sum of the length before the list has a new line and adds it to the x
                //The Slot = Inventory X Item Spot + (The Inventory Y Item Spot * xSize (So how long until it repeats again))
                slots[i].AddItem(inventory.handItems[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }






        ////For the Hands Inventory
        //for (int i = 0; i < slots.Length; i++)
        //{
        //    if(i < inventory.handItems.Count)
        //    {
        //        slots[i].AddItem(inventory.handItems[i]);
        //    }
        //    else
        //    {
        //        slots[i].ClearSlot();
        //    }
        //}
    }
}
