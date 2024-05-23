using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemBehaviour : MonoBehaviour, IPointerDownHandler
{
    //Variables
    public Item item;
    public Vector2[] pos;
    //Functions

    public void OnPointerDown(PointerEventData eventData)
    {
        ObjectWidthManager objectWidthManager =  FindObjectOfType<ObjectWidthManager>();
        objectWidthManager.MoveItem(item, pos);
        objectWidthManager.RemoveItemFromTiles(pos);
        FindObjectOfType<GridManager>().RemoveItem(item,pos);
        Destroy(this.gameObject);
    }
}
