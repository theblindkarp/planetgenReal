using UnityEngine;

public class ItemDiscover : Interactable
{

    public Item item;

    public override void Interact()
    {
        base.Interact();

        Discover();
    }

    void Discover()
    {
        for (int i = 0; i < Fabricator.craftablesImbeded.Length; i++)
        {
            if (Fabricator.craftablesImbeded[i] == item)
            {
                Fabricator.isDiscoveredImbeded[i] = true;
            }
        }
    }
}
