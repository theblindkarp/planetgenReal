using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constructor : MonoBehaviour
{

    //Items to Craft
    public Building[] buildables;
    public bool[] isDiscovered;

    //Available Items
    public List<InventoryItems> inventoryItems = new List<InventoryItems>();
    public InventoryItems exampleInventoryItem;

    //Constructor Info
    public static bool isConstructorOpen = false;

    //For finding slots and stuff
    public Transform itemsParent;
    public GameObject ConstructorUI;

    //Reference to the Player INV
    Inventory inventory;

    //Reference to the Building Manager
    BuildingManager buildingManager;

    //Slots
    public GameObject slotGameObject;
    ConstructorSlot[] slots;

    // Start is called before the first frame update
    void Start()
    {
        buildingManager = GetComponent<BuildingManager>();

        //For Updates
        inventory = Inventory.instance;
        inventory.onItemChangedCallBack += UpdateConstructor;

        //Makes the Slots
        for (int i = 0; i < buildables.Length; i++)
        {
            Instantiate(slotGameObject, itemsParent);
        }

        slots = itemsParent.GetComponentsInChildren<ConstructorSlot>();

        for (int i = 0; i < buildables.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].constructor = this;
                slots[i].buildableItem = buildables[i];
            }
        }

        for (int i = 0; i < slots.Length; i++)
        {
            inventory.onItemChangedCallBack += slots[i].UpdateConstructorSlots;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    //For Seeing what the player can get
    //Also Updated if things are changed
    void UpdateConstructor()
    {
        //To reset the list to prevent duplication
        inventoryItems = new List<InventoryItems>();
        exampleInventoryItem = new InventoryItems();
        inventoryItems.Add(exampleInventoryItem);

        //Goes through the Inventory list and makes a new list
        for (int i = 0; i < inventory.items.Length; i++)
        {
            //Is something REALLY there?
            if (inventory.items[i] != null)
            {
                FindInConInv(i);
            }
        }
    }

    void FindInConInv(int invItem)
    {
        //Gets the prober index number
        for (int u = 0; u < inventoryItems.Count; u++)
        {
            if (inventoryItems[u].invItem == inventory.items[invItem])
            {
                //Debug.Log("Found a Match");
                inventoryItems[u].invCount++;

                return;
            }
            else if (inventoryItems[u].invCount == 0 || inventoryItems[u].invItem == null)
            //If a item has no count or has not item then it will remove it
            {
                inventoryItems.RemoveAt(u);
            }
        }

        //Adds a new Item
        //Debug.Log("Needs to Add");
        exampleInventoryItem = new InventoryItems();
        exampleInventoryItem.invItem = inventory.items[invItem];
        exampleInventoryItem.invCount = 1;
        inventoryItems.Add(exampleInventoryItem);

        return;
    }

    public void OpenConstructor()
    {
        isConstructorOpen = true;

        ConstructorUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Inventory.isInInventory = true;
        inventory.CloseHandsINV();
    }

    public void CloseConstructor()
    {
        ConstructorUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Inventory.isInInventory = false;

        isConstructorOpen = false;
    }

    public void Build(int slotIndex, Building craftedItem, int count = 1)
    {
        //Removes the Items from the Inventory First
        for (int i = 0; i < buildables[slotIndex].requiredItemsToCraft.Length; i++)
        {
            for (int p = 0; p < buildables[slotIndex].requiredItemsToCraft[i].itemAmount; p++)
            {
                DeleteItemForBuilding(slotIndex, i);
            }
        }

        if (inventory.onItemChangedCallBack != null)
            inventory.onItemChangedCallBack.Invoke();

        //Builds the Item
        //Gives to the Building Manager
        buildingManager.SelectObject(craftedItem);

        //Quit out of the Constructor Menu
        CloseConstructor();
    }

    void DeleteItemForBuilding(int slotIndex, int i)
    {
        for (int o = 0; o < inventory.items.Length; o++)
        {
            if (buildables[slotIndex].requiredItemsToCraft[i].reqItem == inventory.items[o])
            {
                int xSize = inventory.items[o].xSize;
                int ySize = inventory.items[o].ySize;

                int[] xyPlace = inventory.Convert1DListTo2D(o);
                int xPlace = xyPlace[0];
                int yPlace = xyPlace[1];

                //For Taking the Items from the Original Slot
                for (int x = 0; x < xSize; x++)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        //if (x + xPlace <= xSize && y + yPlace <= ySize)
                        //{
                        inventory.isSlotAvailable[x + xPlace, y + yPlace] = false;
                        //Debug.Log(x + xPlace + " " + y + yPlace);
                        //}
                    }
                }

                inventory.items[o] = null;

                return;
            }
        }
    }

    [System.Serializable]
    public class InventoryItems
    {
        public Item invItem;
        public int invCount;
    }
}
