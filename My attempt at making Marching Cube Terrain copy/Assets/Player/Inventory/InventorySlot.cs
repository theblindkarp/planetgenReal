using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{

    public Image icon;

    Item item;

    public int inventoryIndex;

    public Text isFullText;

    public float normalSlotDisplaySize = 50f;
    public float gapSize = 6f;

    //int[] fromSlot = new int[2];

    //void Start()
    //{
    //    //To change the 1D List of the Slot Number into a 2D List of the slot on a Grid
    //    int tempYIndex = 0;

    //    for (int i = 0; i < Inventory.instance.ySize; i++)
    //    {
    //        int xthingy = (this.transform.GetSiblingIndex() - (tempYIndex * Inventory.instance.xSize));
    //        if (xthingy >= Inventory.instance.xSize)
    //        {
    //            tempYIndex++;
    //        }
    //    }

    //    fromSlot[0] = (this.transform.GetSiblingIndex() - (tempYIndex * Inventory.instance.xSize));
    //    fromSlot[1] = tempYIndex;
    //    //Debug.Log("x" + fromSlot[0] + "y" + fromSlot[1]);
    //    //Debug.Log(this.transform.GetSiblingIndex());
    //}

    //public void UpdateTextForIsFull()
    //{
    //    int[] index2D = Inventory.instance.Convert1DListTo2D(this.transform.GetSiblingIndex());

    //    if (inventoryIndex == 0)
    //    {
    //        if(Inventory.instance.handItems[this.transform.GetSiblingIndex()] == null)
    //        {
    //            isFullText.text = 0.ToString();
    //        }
    //        else
    //        {
    //            isFullText.text = 1.ToString();
    //        }
    //    }
    //    else if (inventoryIndex == 1)
    //    {
    //        if (Inventory.instance.isSlotAvailable[index2D[0], index2D[1]] == true)
    //        {
    //            isFullText.text = 1.ToString();
    //        }
    //        else
    //        {
    //            isFullText.text = 0.ToString();
    //        }
    //    }
    //    else if (inventoryIndex >= 2)
    //    {
    //        if (Inventory.instance.chests[inventoryIndex - 2].isSlotAvailable[index2D[0], index2D[1]] == true)
    //        {
    //            isFullText.text = 1.ToString();
    //        }
    //        else
    //        {
    //            isFullText.text = 0.ToString();
    //        }
    //    }
    //}

    public void AddItem(Item newItem)
    {
        item = newItem;

        icon.sprite = item.icon;
        icon.enabled = true;

        //For changing the slot image size to scale up the image to fit the item size

        //Looking to see if it is NOT in the Hand Slots
        if(inventoryIndex != 0)
        {
            icon.rectTransform.sizeDelta = new Vector2(((normalSlotDisplaySize * item.xSize) + ((item.xSize - 1) * gapSize)), ((normalSlotDisplaySize * item.ySize) + ((item.ySize - 1) * gapSize)));
            icon.rectTransform.localPosition = new Vector2(((normalSlotDisplaySize * (item.xSize - 1) + ((item.xSize - 1) * gapSize)) / 2), -((normalSlotDisplaySize * (item.ySize - 1) + ((item.ySize - 1) * gapSize)) / 2));
        }

        //UpdateTextForIsFull();

        //For changing Item Slot Size to fit the Item
        //Actually I might not need to put any code here anyway
    }

    public void ClearSlot()
    {
        item = null;

        icon.sprite = null;
        icon.enabled = false;

        //For changing the slot image size back to normal
        icon.rectTransform.sizeDelta = new Vector2(normalSlotDisplaySize, normalSlotDisplaySize);
        icon.rectTransform.localPosition = new Vector2(0, 0);

        //UpdateTextForIsFull();
    }

    public void OnRemoveButton()
    {
        //Inventory.instance.Remove(item);
    }

    public void UseItem()
    {
        if(item != null)
        {
            item.Use();
        }
    }

    //public void UseItem()
    //{
    //    if(item != null)
    //    {
    //        item.Use();
    //    }
    //}

    //When the item is selected It will be ready to be moved
    public void MoveItem()
    {
        if (Inventory.instance.isCursorHoldingItem == null && item != null)
        {
            //This tells the Inventory that the slot it is tring to move from is this slot,
            //and the Item that will be moved is the Item that this slot is holding
            int[] valueXAndY = Inventory.instance.Convert1DListTo2D(this.transform.GetSiblingIndex());
            Inventory.instance.SelectItemToMove(valueXAndY[0], valueXAndY[1], item, inventoryIndex);
        }
        else if(Inventory.instance.isCursorHoldingItem != null)
        {
            //Debug.Log("x" + fromSlot[0] + "y" + fromSlot[1]);
            //Debug.Log(this.transform.GetSiblingIndex());
            //Debug.Log(Inventory.instance.isCursorHoldingItem);

            int[] valueXAndY = Inventory.instance.Convert1DListTo2D(this.transform.GetSiblingIndex());
            bool canPlace = Inventory.instance.CanPlaceInSlot(valueXAndY[0], valueXAndY[1], Inventory.instance.isCursorHoldingItem, inventoryIndex);

            if (canPlace)
                Inventory.instance.PlaceInSlot(valueXAndY[0], valueXAndY[1], Inventory.instance.isCursorHoldingItem, inventoryIndex);
        }
    }
}
