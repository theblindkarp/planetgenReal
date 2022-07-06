using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{

    #region Singleton

    public static Inventory instance;

    public void Awake()
    {
        if(instance != null)
        {
            Debug.LogWarning("More than 1 instance of Inventory found!");
            return;
        }

        instance = this;
    }

    #endregion

    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallBack;

    public int handSlotSpace = 2;

    //public List<Item> handItems = new List<Item>();
    public Item[] handItems;

    public int xSize;
    public int ySize;
    int space;

    public Image cameraHoldingItem;
    public Item isCursorHoldingItem;
    Camera cam;

    public bool[,] isSlotAvailable;
    public Item[] items;
    //Hold the Items Spot (The Item Origin), itemSpots[0, Item Number] = the saved X, and itemSpots[1, Item Number] = the saved Y
    //public int[,] itemSpots;

    //For Identifing the Inventories
    public int totalInventoryIndex = 2;
    [HideInInspector] public Chest[] chests;
    public Transform itemsParent;

    public static bool isInInventory = false;

    HandInventoryUI handInventoryUI;

    void Start()
    {
        cam = Camera.main;

        handItems = new Item[handSlotSpace];

        //Because if you read 2 quotes down you might want this
        space = (xSize * ySize);

        //Makes the Arrays the Right Size
        isSlotAvailable = new bool[xSize, ySize];
        items = new Item[space];
        //itemSpots = new int[2, space];

        chests = itemsParent.GetComponentsInChildren<Chest>();

        //(Also Editors Note, I did change all of this to what all of you probably wanted me to do, just saying)
        //Making Sure that the Slot Sizes and Total Slot Space add up
        //This could be auto matic if in the start function space = xSize * ySize but I want there to be more customization idk
        //if((xSize * ySize) != space)
        //{
        //    Debug.LogWarning("The slotcount and given slot size do not match, are you sure you want to do this?"
        //        + "\n" + "xSize = " + xSize + ", and " + "ySize = " + ySize + " " + "However the total space allowed = " + space
        //        + "\n" + "That is saying that " + (xSize * ySize) + " = " + space
        //        + "\n" + "I'm sorry, but are you the STUPID? :<");
        //}

        handInventoryUI = FindObjectOfType<HandInventoryUI>();
    }

    public void OpenHandsINV()
    {
        if (handInventoryUI != null)
            handInventoryUI.gameObject.SetActive(true);
    }

    public void CloseHandsINV()
    {
        if (handInventoryUI != null)
            handInventoryUI.gameObject.SetActive(false);
    }

    void Update()
    {
        //Holding E
        if (Input.GetKey(KeyCode.E))
        {
            //If Left
            if (Input.GetMouseButtonDown(0))
            {
                handInventoryUI.UsingItems(0);
            }

            //If Right
            if (Input.GetMouseButtonDown(1))
            {
                handInventoryUI.UsingItems(1);
            }
        }
    }

    public int Convert2DListTo1D(int xPlace, int yPlace)
    {
        int returnValue = xPlace + (yPlace * xSize);

        return returnValue;
    }

    public int[] Convert1DListTo2D(int listPlace)
    {
        int[] fromSlot = new int[2];
        int tempYIndex = 0;

        for (int i = 0; i < ySize; i++)
        {
            int xthingy = (listPlace - (tempYIndex * xSize));
            if (xthingy >= xSize)
            {
                tempYIndex++;
            }
        }

        //Getting where it was from in a 2D way/grid
        fromSlot[0] = listPlace - (tempYIndex * xSize);
        fromSlot[1] = tempYIndex;

        return fromSlot;
    }

    public void SelectItemToMove(int xPlace, int yPlace, Item item, int fromInventoryIndex)
    {
        //Debug.Log("X: " + xPlace + " " + "Y: " + yPlace + " " + "Item: " + item + " " + "INV: " + fromInventoryIndex);

        if (isCursorHoldingItem == null)
        {
            isCursorHoldingItem = item;

            //For Showing It
            cameraHoldingItem.GetComponent<Image>().sprite = item.icon;
            cameraHoldingItem.enabled = true;
            StartCoroutine(ItemFollowCursor());



            //Slots Types in the Player Inventory
            if(fromInventoryIndex == 0) //Hand Slots
            {
                handItems[Convert2DListTo1D(xPlace, yPlace)] = null;
            }
            else if(fromInventoryIndex == 1) //Player Inventory Slots
            {
                //Removes the Original Item
                items[Convert2DListTo1D(xPlace, yPlace)] = null;

                //For Taking the Items from the Original Slot
                for (int x = 0; x < item.xSize; x++)
                {
                    for (int y = 0; y < item.ySize; y++)
                    {
                        if (x + xPlace < xSize && y + yPlace < ySize)
                            isSlotAvailable[x + xPlace, y + yPlace] = false;
                    }
                }

                //Removes the Spots
                //itemSpots[0, xPlace] = 0; //0 For the xPlace
                //itemSpots[1, yPlace] = 0; //1 For the yPlace

                //Remove the ItemSpots thingy and replace it with nothing but get th eindex of the item and use that or something like that idk my bairna f did  sfaileinyh g ................ eerrror offline...
            }
            else //Player Inventory Slots
            {
                //Removes the Original Item
                chests[fromInventoryIndex - 2].items[Convert2DListTo1D(xPlace, yPlace)] = null;

                //For Taking the Items from the Original Slot
                for (int x = 0; x < item.xSize; x++)
                {
                    for (int y = 0; y < item.ySize; y++)
                    {
                        if (x + xPlace < xSize && y + yPlace < ySize)
                            chests[fromInventoryIndex - 2].isSlotAvailable[x + xPlace, y + yPlace] = false;
                    }
                }
            }
        }

        //Set the Physical Spot for the Item
        if (onItemChangedCallBack != null)
            onItemChangedCallBack.Invoke();
    }

    //Validates if the item can be put in the Inventory
    public bool CanPlaceInSlot(int xPlace, int yPlace, Item item, int inventoryIndex)
    {
        //Debug.Log("X: " + xPlace + " " + "Y: " + yPlace + " " + "Item: " + item + " " + "INV: " + inventoryIndex);

        //Again Checks for the HARD CODED Inventory Types
        if (inventoryIndex == 0) //Hand INV
        {   
            int handIndex = Convert2DListTo1D(xPlace, yPlace);
            if(handItems[handIndex] == null)
            {
                handItems[handIndex] = item;
                return true;
            }
            else
            {
                Debug.Log("No Valid Space Here!");
                return false;
            }
        }
        else if(inventoryIndex == 1) //Player INV
        {
            int counter = 0;
            for (int x = 0; x < item.xSize; x++)
            {
                for (int y = 0; y < item.ySize; y++)
                {
                    if (x + xPlace < xSize && y + yPlace < ySize)
                    {
                        //Something is There already
                        if (isSlotAvailable[x + xPlace, y + yPlace] == true)
                        {
                            Debug.Log("No Valid Space Here!");
                            return false;
                        }
                        else
                        {
                            counter++;
                        }
                    }
                    //else //If it is outside of the Array
                    //{
                    //    Debug.Log("No Valid Space Here!");
                    //    return false;
                    //}
                }
            }

            //For seeing if ALL slots are available
            if (counter == (item.xSize * item.ySize))
            {
                //There is SPACE!!!
                return true;
            }
        }
        else //Chest INV
        {
            int counter = 0;
            for (int x = 0; x < item.xSize; x++)
            {
                for (int y = 0; y < item.ySize; y++)
                {
                    if (x + xPlace < xSize && y + yPlace < ySize)
                    {
                        //Something is There already
                        if (chests[inventoryIndex - 2].isSlotAvailable[x + xPlace, y + yPlace] == true)
                        {
                            Debug.Log("No Valid Space Here!");
                            return false;
                        }
                        else
                        {
                            counter++;
                        }
                    }
                    //else //If it is outside of the Array
                    //{
                    //    Debug.Log("No Valid Space Here!");
                    //    return false;
                    //}
                }
            }

            //For seeing if ALL slots are available
            if (counter == (item.xSize * item.ySize))
            {
                //There is SPACE!!!
                return true;
            }
        }

        return false;
    }

    public void PlaceInSlot(int xPlace, int yPlace, Item item, int inventoryIndex)
    {
        //Debug.Log("X: " + xPlace + " " + "Y: " + yPlace + " " + "Item: " + item + " " + "INV: " + inventoryIndex);

        //Again Again and Again it Checks the Inventory Types
        if (inventoryIndex == 0) //Hand INV
        {
            handItems[Convert2DListTo1D(xPlace, yPlace)] = item;
        }
        else if(inventoryIndex == 1) //And Again The Player INV
        {
            //To remember what things are there and most importantly WHERE THEY ARE
            items[Convert2DListTo1D(xPlace, yPlace)] = item;

            for (int x = 0; x < item.xSize; x++)
            {
                for (int y = 0; y < item.ySize; y++)
                {
                    if((xPlace + x) < xSize && (yPlace + y) < ySize)
                    {
                        isSlotAvailable[xPlace + x, yPlace + y] = true; //Sets the Slot and its Slot Places to be Available
                    }
                }
            }

            //Remebers the Spots
            //itemSpots[0, (xPlace + (yPlace * xSize))] = xPlace; //0 For the xPlace
            //itemSpots[1, (xPlace + (yPlace * xSize))] = yPlace; //1 For the yPlace
        }
        else //The Chest INV
        {
            //To remember what things are there and most importantly WHERE THEY ARE
            chests[inventoryIndex - 2].items[Convert2DListTo1D(xPlace, yPlace)] = item;

            for (int x = 0; x < item.xSize; x++)
            {
                for (int y = 0; y < item.ySize; y++)
                {
                    if ((xPlace + x) < xSize && (yPlace + y) < ySize)
                    {
                        chests[inventoryIndex - 2].isSlotAvailable[xPlace + x, yPlace + y] = true; //Sets the Slot and its Slot Places to be Available
                    }
                }
            }
        }

        //Debug.Log("placeable");

        //Set the Physical Spot for the Item
        if (onItemChangedCallBack != null)
            onItemChangedCallBack.Invoke();

        //To Take the Item Out of the Cursor
        cameraHoldingItem.GetComponent<Image>().sprite = null;
        cameraHoldingItem.enabled = false;
        isCursorHoldingItem = null;
        StopCoroutine(ItemFollowCursor());
    }

    bool AutoPlaceInINV(Item item)
    {
        //INV Size
        for (int yPlace = 0; yPlace < ySize; yPlace++)
        {
            for (int xPlace = 0; xPlace < xSize; xPlace++)
            {
                bool result = INVChecking(item, xPlace, yPlace);
                if(result == true)
                {
                    return true;
                }
            }
        }

        Debug.Log("NO SPACE!");

        return false;
    }

    bool INVChecking(Item item, int xPlace, int yPlace)
    {
        //Item Size
        int counter = 0;
        for (int x = 0; x < item.xSize; x++)
        {
            for (int y = 0; y < item.ySize; y++)
            {
                if (x + xPlace < xSize && y + yPlace < ySize)
                {
                    //Something is There already
                    if (isSlotAvailable[x + xPlace, y + yPlace] == true)
                    {
                        //Debug.Log("No Valid Space Here!");
                        return false;
                    }
                    else
                    {
                        counter++;
                    }
                }
            }
        }

        //For seeing if ALL slots are available
        if (counter == (item.xSize * item.ySize))
        {
            //There is SPACE!!!
            //Debug.Log("placed");

            //Places it
            items[Convert2DListTo1D(xPlace, yPlace)] = item;

            for (int x = 0; x < item.xSize; x++)
            {
                for (int y = 0; y < item.ySize; y++)
                {
                    if ((xPlace + x) < xSize && (yPlace + y) < ySize)
                    {
                        isSlotAvailable[xPlace + x, yPlace + y] = true; //Sets the Slot and its Slot Places to be Available
                    }
                }
            }

            //Set the Physical Spot for the Item
            if (onItemChangedCallBack != null)
                onItemChangedCallBack.Invoke();

            return true;
        }

        return false;
    }

    IEnumerator ItemFollowCursor()
    {
        //Vector3 mouseWorldPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
        Vector2 mouseWorldPosition = Input.mousePosition;
        //mouseWorldPosition.z = 0f;
        cameraHoldingItem.transform.position = mouseWorldPosition;

        yield return new WaitForEndOfFrame();

        StartCoroutine(ItemFollowCursor());
    }

    public bool Add(Item item)
    {
        for (int i = 0; i < handItems.Length; i++)
        {
            if(handItems[i] == null)
            {
                handItems[i] = item;

                if (onItemChangedCallBack != null)
                    onItemChangedCallBack.Invoke();

                return true;
            }
        }

        bool addedToINV = AutoPlaceInINV(item);

        //Debug.Log("NO SPACE");

        return addedToINV;

            //if (!item.isDefaultItem)
            //{
            //    if (handItems.Count >= handSlotSpace)
            //    {
            //        Debug.Log("NO SPACE");
            //        return false;
            //    }

            //    handItems.Add(item);
            //    //PlaceInSlot(0, 0, item);

            //    if (onItemChangedCallBack != null)
            //        onItemChangedCallBack.Invoke();
            //}

            //return true;
    }

    //public void Remove(Item item)
    //{
    //    handItems.Remove(item);

    //    if (onItemChangedCallBack != null)
    //        onItemChangedCallBack.Invoke();
    //}
}
