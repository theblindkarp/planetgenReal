using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fabricator : Interactable
{

    //Items to Craft
    public Item[] craftables;
    public static Item[] craftablesImbeded;
    public bool[] isDiscovered; //Can be seen (Seen in Inspector as well)
    public static bool[] isDiscoveredImbeded; //Can be Seen (Static)

    //Crafting Space (Where crafted items will be placed in the real/game world)
    public GameObject craftingPlace;

    //Available Items
    public List<InventoryItems> inventoryItems = new List<InventoryItems>();
    public InventoryItems exampleInventoryItem;

    //Fabricator Info
    public static bool isFabricatorOpen = false;

    //For finding slots and stuff
    public Transform itemsParent;
    public GameObject fabricatorUI;

    //Reference to the Player INV
    Inventory inventory;
    InventoryUI inventoryUI;

    //Slots
    public GameObject slotGameObject;
    FabricatorSlot[] slots;

    // Start is called before the first frame update
    void Start()
    {
        //For Updates
        inventory = Inventory.instance;
        inventoryUI = FindObjectOfType<InventoryUI>();
        inventory.onItemChangedCallBack += UpdateFabricator;

        for (int i = 0; i < craftablesImbeded.Length; i++)
        {
            Instantiate(slotGameObject, itemsParent);
        }

        slots = itemsParent.GetComponentsInChildren<FabricatorSlot>();

        for (int i = 0; i < craftablesImbeded.Length; i++)
        {
            if(slots[i] != null)
            {
                slots[i].fabricator = this;
                slots[i].craftableItem = craftablesImbeded[i];
            }
        }

        for (int i = 0; i < slots.Length; i++)
        {
            inventory.onItemChangedCallBack += slots[i].UpdateFabricatorSlots;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnValidate()
    {
        craftablesImbeded = craftables;
        isDiscoveredImbeded = isDiscovered;
    }

    //For Seeing what the player can get
    //Also Updated if things are changed
    void UpdateFabricator()
    {
        //To reset the list to prevent duplication
        inventoryItems = new List<InventoryItems>();
        exampleInventoryItem = new InventoryItems();
        inventoryItems.Add(exampleInventoryItem);

        //Goes through the Inverntory list and makes a new list
        for (int i = 0; i < inventory.items.Length; i++)
        {
            //Is something REALLY there?
            if(inventory.items[i] != null)
            {
                FindInFabInv(i);
            }
        }
    }

    void FindInFabInv(int invItem)
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
            else if(inventoryItems[u].invCount == 0 || inventoryItems[u].invItem == null)
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

    //For Interacting with the Player
    public override void Interact()
    {
        base.Interact();

        OpenFabricator();
        UpdateFabricator();

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].gameObject.SetActive(isDiscoveredImbeded[i]);
            slots[i].UpdateFabricatorSlots();
        }
    }

    void OpenFabricator()
    {
        isFabricatorOpen = true;

        fabricatorUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Inventory.isInInventory = true;
        inventoryUI.inventoryUI.SetActive(true);
        inventory.CloseHandsINV();
    }

    public void CloseFabricator()
    {
        fabricatorUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Inventory.isInInventory = false;
        inventoryUI.inventoryUI.SetActive(false);

        isFabricatorOpen = false;
    }

    public void Craft(int slotIndex, Item craftedItem, int count = 1)
    {
        //Removes the Items from the Inventory First
        for (int i = 0; i < craftablesImbeded[slotIndex].requiredItemsToCraft.Length; i++)
        {
            for (int p = 0; p < craftablesImbeded[slotIndex].requiredItemsToCraft[i].itemAmount; p++)
            {
                DeleteItemForCrafting(slotIndex, i);
            }
        }

        if (inventory.onItemChangedCallBack != null)
            inventory.onItemChangedCallBack.Invoke();

        //Crafts the Amount (Default is 1)
        for (int i = 0; i < count; i++)
        {
            //Crafts the Item
            GameObject spawnedObject = Instantiate(craftedItem.itemObject, craftingPlace.transform.position, Quaternion.identity, craftingPlace.transform);
        }
    }

    void DeleteItemForCrafting(int slotIndex, int i)
    {
        for (int o = 0; o < inventory.items.Length; o++)
        {
            if (craftablesImbeded[slotIndex].requiredItemsToCraft[i].reqItem == inventory.items[o])
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
