using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingManager : MonoBehaviour
{

    //public GameObject[] objects;
    private GameObject pendingObject;

    private Vector3 pos;

    private RaycastHit hit;
    [SerializeField] private LayerMask layerMask;

    public float rotationAmount;

    public float gridSize;
    bool gridOn;
    //[SerializeField] private Toggle gridToggle;

    Inventory inventory;
    InventoryUI inventoryUI;
    PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        inventory = FindObjectOfType<Inventory>();
        inventoryUI = FindObjectOfType<InventoryUI>();
        playerMovement = FindObjectOfType<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if(pendingObject != null)
        {
            if (gridOn)
            {
                pendingObject.transform.position = new Vector3(
                    RoundToNearestGrid(pos.x),
                    RoundToNearestGrid(pos.y),
                    RoundToNearestGrid(pos.z)
                    );
            }
            else
            {
                pendingObject.transform.position = pos;
            }

            if (Input.GetMouseButtonDown(0))
            {
                PlaceObject();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                RotateObject();
            }
        }

        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    SelectObject(0);
        //}

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (gridOn == false)
            {
                gridOn = true;
            }
            else if (gridOn == true)
            {
                gridOn = false;
            }

            //Debug.Log(gridOn);
        }
    }

    public void PlaceObject()
    {
        inventoryUI.UpdateUIItems();

        pendingObject = null;

        inventory.OpenHandsINV();
    }

    public void RotateObject()
    {
        pendingObject.transform.Rotate(Vector3.up, rotationAmount);
    }

    void FixedUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit, playerMovement.range, layerMask))
        {
            pos = hit.point + new Vector3(0, 0.5f, 0);
        }
        else
        {
            float xCord = Mathf.Sin(Camera.main.ScreenPointToRay(Input.mousePosition).direction.x);
            float zCord = Mathf.Sin(Camera.main.ScreenPointToRay(Input.mousePosition).direction.z);
            float yCord = Mathf.Sin(Camera.main.ScreenPointToRay(Input.mousePosition).direction.y);

            pos = Camera.main.ScreenPointToRay(Input.mousePosition).origin + new Vector3(xCord, yCord, zCord) * playerMovement.range;
        }
    }

    public void SelectObject(Building building)
    {
        pendingObject = Instantiate(building.itemObject, pos, transform.rotation);
    }

    public void ToggleGrid()
    {
        //if (gridToggle.isOn)
        //{
        //    gridOn = true;
        //}
        //else if(gridToggle.isOn == false)
        //{
        //    gridOn = false;
        //}
    }

    float RoundToNearestGrid(float pos)
    {
        float xDiff = pos % gridSize;
        pos -= xDiff;
        if(xDiff > (gridSize / 2))
        {
            pos += gridSize;
        }
        return pos;
    }

    public class Buildings
    {

    }
}
