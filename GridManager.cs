using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    //Variables
    [Header("Grid Options")]
    public int _width, _height;
    public Vector2 spacing;
    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private Transform _spawnParent;
    [SerializeField, Range(0,5)] private float rowsColumnProportion = 1.5f;
    private ObjectWidthManager objectWidthManager;

    [Serializable]
    public class ItemPosition
    {
        [SerializeField]
        public Vector2[] position;
        [SerializeField]
        public Item item;
    }

    [SerializeField] private ItemPosition[] items = new ItemPosition[100];
    //Functions

    #region Unity Methods

    private void Start()
    {
        objectWidthManager = GetComponent<ObjectWidthManager>();
        GenerateGrid();
    }

    private void LateUpdate() 
    {
        SetTileSize();
    }

    #endregion 

    #region Item Methods
    /// <summary>
    /// Method that adds an item in the desired position
    /// </summary>
    /// <param name="item"></param>
    /// <param name="pos"></param>
    public void AddItem(Item item, Vector2[] pos )
    {
        for (int i = 0; i < items.Length; i++)
        {
            if(items[i].item == null)
            {
                items[i].position = pos;
                items[i].item = item;
                break;
            }
        }
    }

    /// <summary>
    /// Method that removes and item in the desired position
    /// </summary>
    /// <param name="item"></param>
    /// <param name="pos"></param>
    public void RemoveItem(Item item, Vector2[] pos)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if(items[i].item == item && items[i].position.Equals(pos))
            {
                items[i].position = null;
                items[i].item = null;
                break;
            }
        }
    }

    #endregion

    #region Grid Methods
    /// <summary>
    /// Method that generates our grid
    /// </summary>
    void GenerateGrid()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector2(x,y), Quaternion.identity, _spawnParent);
                spawnedTile.name = $"Tile {x} {y}";
                objectWidthManager.tiles.Add(spawnedTile.GetComponent<RectTransform>());
            }
        }
    }

    /// <summary>
    /// Set the tiles size depending of the grid size
    /// </summary>
    void SetTileSize()
    {
        try
        {
            GridLayoutGroup grid = _spawnParent.GetComponent<GridLayoutGroup>();
            RectTransform rectTransform = grid.GetComponent<RectTransform>();
            float containerWidth = rectTransform.rect.width;
            float containerHeight = rectTransform.rect.height;
            int numberOfTiles = _width * _height;
            int columns = 0;
            int rows = 0;
            if(_height > _width)
            {
                // If we have more rows than columns
                rows = Mathf.CeilToInt(Mathf.Sqrt(numberOfTiles) * rowsColumnProportion);
                columns = Mathf.CeilToInt((float)numberOfTiles / rows);
            }
            else
            {
                //If we want the same tiles row/column based
                columns = Mathf.CeilToInt(Mathf.Sqrt(numberOfTiles));
                rows = Mathf.CeilToInt((float) numberOfTiles/ columns);
            }

            //Sets the grid spacing
            grid.spacing = spacing;

            //Size of tile depending of padding and spacing
            float cellWidth = (containerWidth - (grid.padding.left + grid.padding.right)- (grid.spacing.x * (columns - 1))) / columns;
            float cellHeight = (containerHeight - (grid.padding.top + grid.padding.bottom) - (grid.spacing.y * (rows - 1))) / rows;

            grid.cellSize = new Vector2(cellWidth, cellHeight);
        }
        catch
        {
            Debug.LogError("Error trying to get the grid component.");
        }
    }
    #endregion
}
