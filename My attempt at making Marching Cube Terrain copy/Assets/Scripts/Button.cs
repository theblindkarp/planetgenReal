using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Button : MonoBehaviour
{

    public UnityEvent unityEvent = new UnityEvent();
    public Material[] materials;
    Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Input.GetMouseButtonDown(0))
        {
            if(Physics.Raycast(ray, out hit) && hit.collider.gameObject == gameObject)
            {
                unityEvent.Invoke();
            }
        }
    }

    public void ChangeColor(int colorIndex)
    {
        rend.sharedMaterial = materials[colorIndex];
    }
}
