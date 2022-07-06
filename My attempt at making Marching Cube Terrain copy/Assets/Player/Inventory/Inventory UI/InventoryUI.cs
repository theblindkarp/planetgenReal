using UnityEngine;

public class InventoryUI : MonoBehaviour
{

    public Transform itemsParent;
    public GameObject inventoryUI;

    public bool isCursorHoldingItem = false;

    Inventory inventory;

    InventorySlot[] slots;

    public int inventoryIndex = 1;

    Chest[] chests;
    Fabricator[] fabricators;
    Constructor constructor;

    // Start is called before the first frame update
    void Start()
    {
        inventory = Inventory.instance;
        UpdateUIItems();
        inventory.onItemChangedCallBack += UpdateUI;

        slots = itemsParent.GetComponentsInChildren<InventorySlot>();

        //For setting the Inventory Index
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].inventoryIndex = inventoryIndex;
        }
    }

    public void UpdateUIItems()
    {
        chests = FindObjectsOfType<Chest>();
        fabricators = FindObjectsOfType<Fabricator>();
        constructor = FindObjectOfType<Constructor>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Inventory"))
        {
            if (Inventory.isInInventory == false)
            {
                OpenINV();
            }
            else if (Inventory.isInInventory == true || Chest.isChestOpen == true
                || Fabricator.isFabricatorOpen == true || Constructor.isConstructorOpen == true)
            {
                CloseINV();
                inventory.OpenHandsINV();
                for (int i = 0; i < chests.Length; i++)
                {
                    chests[i].CloseChest();
                }
                for (int i = 0; i < fabricators.Length; i++)
                {
                    fabricators[i].CloseFabricator();
                }
                constructor.CloseConstructor();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseINV();
            inventory.OpenHandsINV();
            for (int i = 0; i < chests.Length; i++)
            {
                chests[i].CloseChest();
            }
            for (int i = 0; i < fabricators.Length; i++)
            {
                fabricators[i].CloseFabricator();
            }
            if (constructor != null)
                constructor.CloseConstructor();
        }
    }

    void OpenINV()
    {
        Cursor.lockState = CursorLockMode.None;
        Inventory.isInInventory = true;
        inventoryUI.SetActive(true);
    }

    void CloseINV()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Inventory.isInInventory = false;
        inventoryUI.SetActive(false);
    }

    void UpdateUI()
    {
        //For the other Inventories
        for (int i = 0; i < inventory.items.Length; i++)
        {
            if (inventory.items[i] != null)
            {
                //This converts a 2D Array into a 1D List by adding the multiplied sum of the length before the list has a new line and adds it to the x
                //The Slot = Inventory X Item Spot + (The Inventory Y Item Spot * xSize (So how long until it repeats again))
                //slots[inventory.itemSpots[0, i] + (inventory.itemSpots[1, i] * inventory.xSize)].AddItem(inventory.items[i]);
                slots[i].AddItem(inventory.items[i]);
            }
            else
            {
                //slots[inventory.itemSpots[0, i] + (inventory.itemSpots[1, i] * inventory.xSize)].ClearSlot();
                slots[i].ClearSlot();
            }
        }
    }
}