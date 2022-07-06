using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ConstructorTool", menuName = "Inventory/ConstructorTool")]
public class ConstructorTool : Item
{

    public override void Use()
    {
        base.Use();

        Constructor constructor = FindObjectOfType<Constructor>();
        constructor.OpenConstructor();
        Inventory inventory = Inventory.instance;
        inventory.CloseHandsINV();
    }

}