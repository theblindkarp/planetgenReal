using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : Interactable
{

    //For Item Info
    public bool[,] isSlotAvailable;
    public Item[] items;

    //Chest Info
    public int xSize;
    public int ySize;
    int space;
    public static bool isChestOpen = false;

    //Chest ID
    public int inventoryIndex;

    //For finding slots and stuff
    public Transform itemsParent;
    public GameObject chestUI;

    //Reference to the Player INV
    Inventory inventory;
    InventoryUI inventoryUI;

    //Slots
    InventorySlot[] slots;

    // Start is called before the first frame update
    void Start()
    {
        //For Updates
        inventory = Inventory.instance;
        inventoryUI = FindObjectOfType<InventoryUI>();
        inventory.onItemChangedCallBack += UpdateUI;

        //For Giving its self an index
        inventoryIndex = this.transform.GetSiblingIndex() + 2;
        //inventoryIndex = inventory.totalInventoryIndex;
        //inventory.totalInventoryIndex++;

        //Variables and Stuff
        space = (xSize * ySize);

        isSlotAvailable = new bool[xSize, ySize];
        items = new Item[space];

        slots = itemsParent.GetComponentsInChildren<InventorySlot>();

        //For setting the Inventory Index
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].inventoryIndex = inventoryIndex;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Only for Closing
        //if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Inventory")) && isChestOpen == true)
        //{
        //    CloseChest();
        //}
    }

    //For Interacting with the Player
    public override void Interact()
    {
        base.Interact();

        OpenChest();
    }

    void OpenChest()
    {
        isChestOpen = true;

        chestUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Inventory.isInInventory = true;
        inventoryUI.inventoryUI.SetActive(true);
        inventory.CloseHandsINV();
    }

    public void CloseChest()
    {
        chestUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Inventory.isInInventory = false;
        inventoryUI.inventoryUI.SetActive(false);

        isChestOpen = false;
    }

    void UpdateUI()
    {
        //For the other Inventories
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                slots[i].AddItem(items[i]);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }
}
