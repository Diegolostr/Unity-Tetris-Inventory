using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemConfigMenu : MonoBehaviour
{
    //Variables

    [SerializeField] private Vector3 offset;
    [SerializeField] private RectTransform itemMenuTransform;

    private bool stopped = false;
    //Functions

    private void Update() 
    {
        OnLeftClick();
  
    }

    private void OnLeftClick()
    {
        if(Input.GetMouseButton(0) && stopped)
        {
            // Convierte la posición del mouse a la posición del RectTransform en el canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                itemMenuTransform, 
                Input.mousePosition, 
                null, 
                out Vector2 localPoint
            );

            if (!itemMenuTransform.rect.Contains(localPoint))
            {
                SetItemConfig(false);
                stopped = false;
            }
        }
    }

    public void OnRightClick()
    {
        SetItemConfig(true);

        stopped = true;
        int offsetX = (int)itemMenuTransform.sizeDelta.x + (int)offset.x;
        int offsetY = (int)itemMenuTransform.sizeDelta.y + (int)offset.y;

        itemMenuTransform.position = new Vector3(Input.mousePosition.x + offsetX,Input.mousePosition.y - offsetY,Input.mousePosition.z);  


    }

    void SetItemConfig(bool isEnabled)
    {
        itemMenuTransform.gameObject.SetActive(isEnabled);
    }

    public void OnEquipButton()
    {
        print("Equipado");
    }
}
