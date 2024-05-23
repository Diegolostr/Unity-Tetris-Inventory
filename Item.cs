using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Item/New Item")]
public class Item : ScriptableObject
{
    //Variables
    public string _name;
    public string _description;
    public Vector2 _size;
    public Sprite _itemIcon;
}
