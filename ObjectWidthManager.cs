using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ObjectWidthManager : MonoBehaviour
{
    //Variables

    [Header("Tiles Colors")]
    [SerializeField] private Color _tileHoverColor = Color.green;
    [SerializeField] private Color _tileNotEmptyColor = Color.yellow;
    [SerializeField] private Color _tileEmptyColor = Color.white;
    [SerializeField] private Color _tileOcuppiedColor = Color.red;

    [Header("Image color")]
    [SerializeField] private Color _imageDragColor = Color.white;

    [Header("Options")]
    [SerializeField] private RectTransform _cursorTransform;
    [SerializeField] private bool onlyMoveOnGrid = false;
    [SerializeField] private Vector3 offset;
    [SerializeField] private bool SmoothMovement = true;
    [SerializeField, Range(0,20)] private float _smoothMovementAmount = 0;
    private int cellSize;
    public GameObject imagePrefab;

    [Header("Tiles")]
    public List<RectTransform> tiles = new List<RectTransform>();
    private List<RectTransform> hoveredTiles = new List<RectTransform>();
    [SerializeField] private RectTransform _inventory;

    [Header("Others")]
    private Vector2 objectSize = new Vector2(16,16);
    private Vector2 detectionSize = new Vector2(16, 16);
    private GridManager gridManager;
    [HideInInspector] public Item activeItem;
    private bool isDragging = false;
    private Vector2[] lastItemPositionInGrid = new Vector2[0];

    [Header("Testing")]
    public Item item1, item2;

    private Vector3 lastPos;
    private bool IsMoving;
    private Canvas canvas;
    //Functions

    private void Start() 
    {
        gridManager = GetComponent<GridManager>();
        _cursorTransform.sizeDelta = objectSize;
        lastPos = _cursorTransform.position;
        canvas = GetComponentInChildren<Canvas>();
        foreach (var tile in tiles)
        {
            tile.GetComponent<RawImage>().color = _tileEmptyColor;
        }
    }

    private void Update() 
    {
        Instantiate();
        if(Input.GetMouseButtonUp(0) && isDragging)
        {
            if(activeItem)
            InsertItem();
        }

        if(!activeItem) return;
        if(SmoothMovement)
        {
        float canvasScaleFactor = canvas.scaleFactor == 1 ? canvas.scaleFactor - 0.5f : canvas.scaleFactor / 2;

        _cursorTransform.position = Vector3.MoveTowards(_cursorTransform.position, GetMagnetizedPosition(Input.mousePosition), _smoothMovementAmount * canvasScaleFactor);
        }
        else
        {
           _cursorTransform.position = GetMagnetizedPosition(Input.mousePosition);
        }
        CheckTilesUnderCursor();
        UpdateTileHover();


    }

    void LateUpdate()
    {
        cellSize = (int)GetComponentInChildren<GridLayoutGroup>().cellSize.x;
    }

    void Instantiate()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            activeItem = item2;
            InsertIntoInventory(item2._size);
            activeItem = null;
        }
        if(Input.GetKeyDown(KeyCode.B))
        {
            activeItem = item1;
            InsertIntoInventory(item1._size);
            activeItem = null;

        }
    }

    #region Inventory Controllers

    /// <summary>
    /// Method in charge of inserting the item into the inventory
    /// </summary>
    /// <param name="size"></param>
    bool InsertIntoInventory(Vector2 size)
    {

        //Others
        Dictionary<RectTransform, bool> tilesToCheck = new();
        RectTransform firstTileChecked = null;
        foreach (var tile in tiles)
        {
            tilesToCheck.Add(tile, false);
        }
        GridManager grid = GetComponent<GridManager>();

        //Ints
        int _lastRow = 0;
        int _lastColumn = 0;
        int i = 0;
        int totalTilesToCheck = (int)(size.x * size.y);
        int _gridWidth = grid._width;
        int _gridHeight = grid._height;
        //Bools
        bool _canBePlaced = false;
        //Vector2
        Vector2 _firstTile = new(-1,-1);
        Vector2[] _corners = new Vector2[4];

        for (int j = 0; j < tilesToCheck.Count; j++)
        {
            _firstTile = new(-1,-1);

            foreach (var tile in tilesToCheck)
            {
                bool _isOcuppied = tile.Key.GetComponent<Tile>().IsOcuppied;
                bool _isChecked = tile.Value;

                int _row = Int32.Parse(tile.Key.name.Split(" ")[1]);
                int _column = Int32.Parse(tile.Key.name.Split(" ")[2]);

                if(!_isOcuppied && _firstTile.x == -1 && !_isChecked)
                {

                    //Ints
                    _lastRow = _row + (int)(size.y - 1);
                    _lastColumn = _column + (int)(size.x - 1);

                    //Vectors2
                    firstTileChecked = tile.Key;
                    _firstTile = new Vector2(_column, _row);
                    _corners[0] = _firstTile;
                    _corners[1] = new Vector2(_lastColumn,_firstTile.y);
                    _corners[2] = new Vector2(_firstTile.x, _lastRow);
                    _corners[3] = new Vector2(_lastColumn, _lastRow);

                    if(size.y * size.x == 1)
                    {
                        _corners[0] = _firstTile;
                        _corners[1] = _firstTile;
                        _corners[2] = _firstTile;
                        _corners[3] = _firstTile;
                    }
                }

                _firstTile = _lastColumn > _gridWidth - 1 ?  new Vector2(-1,-1) : _firstTile;

                if(_lastRow > _gridHeight -1) { break; }

                if(_firstTile.x != -1 && i < totalTilesToCheck)
                {
                    if(_row >= _firstTile.y && _row <= _lastRow)
                    {
                        if(_column >= _firstTile.x && _column <= _lastColumn)
                        {
                            if(tile.Key.GetComponent<Tile>().IsOcuppied)
                            {
                                tilesToCheck[firstTileChecked] = true;
                                i = 0;
                                break;
                            }
                            i++;
                        }
                    }
                }
                
                _canBePlaced = i == totalTilesToCheck;

                if(_canBePlaced) { break; }
            }
            if(_canBePlaced) { break; }
        
        }
        if(_canBePlaced)
        {
            var item = Instantiate(imagePrefab ,GetCenterPositionOfTiles(_corners[0], _corners[1], _corners[2], _corners[3]), Quaternion.identity);
            InstantiateItemIcon(item, _corners[0], _corners[3]);
        }
        return _canBePlaced;
    }

    /// <summary>
    /// Method in charge of insert the item in the desired position of the grid when dragging
    /// </summary>
    void InsertItem()
    {
        if(CanBePlaced(activeItem._size))
        {
            Vector2[] positions = new Vector2[hoveredTiles.Count];
            int i = 0;
            foreach (var tile in hoveredTiles)
            {
                tile.GetComponent<Tile>().IsOcuppied = true;
                positions[i] = new Vector2(Int32.Parse(tile.name.Split(" ")[1]), Int32.Parse(tile.name.Split(" ")[2]));
                UpdateTileWithItem(tile, true);
                i++;
            }
            gridManager.AddItem(activeItem, positions);
            var image = Instantiate(imagePrefab);
            RectTransform prefabTransform = image.GetComponent<RectTransform>();
            prefabTransform.SetParent(_inventory);
            prefabTransform.position = _cursorTransform.position;
            Vector2 itemSize = VisualItemSize(activeItem._size, true);
            prefabTransform.sizeDelta = new Vector2(itemSize.x * 2, itemSize.y * 2);;
            image.GetComponent<Image>().sprite = activeItem._itemIcon;
            _cursorTransform.GetComponent<Image>().color = Color.clear;
            image.GetComponent<ItemBehaviour>().item = activeItem;
            image.GetComponent<ItemBehaviour>().pos = positions;
            activeItem = null;
        }
        else
        {
            Vector2[] corners = GetCorners(lastItemPositionInGrid);
            if(lastItemPositionInGrid.Count() == 1)
            {
                var item = Instantiate(imagePrefab ,GetCenterPositionOfTiles(corners[0], corners[0], corners[0], corners[0]), Quaternion.identity);
                InstantiateItemIcon(item, corners[0], corners[3]);

            }
            else
            {
                var item = Instantiate(imagePrefab ,GetCenterPositionOfTiles(corners[0], corners[1], corners[2], corners[3]), Quaternion.identity);
                InstantiateItemIcon(item, corners[0], corners[3]);

            }
            foreach (var tile in hoveredTiles)
            {
                UpdateTileWithItem(tile, false);
            }
        }
    }

    #endregion

    #region Tile controllers

    /// <summary>
    /// Method called outside this script used to set the Active item and the parameters
    /// </summary>
    /// <param name="item"></param>
    /// <param name="positions"></param>
    public void MoveItem(Item item, Vector2[] positions, Vector3 position)
    {
        lastItemPositionInGrid = new Vector2[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            lastItemPositionInGrid[i].y = positions[i].x;
            lastItemPositionInGrid[i].x = positions[i].y;
        }
        _cursorTransform.position = position;
        activeItem = item;
        UpdateItemIcon(item);
        isDragging = true;
    }

    /// <summary>
    /// Method that updates the item icon dragging
    /// </summary>
    /// <param name="item"></param>
    void UpdateItemIcon(Item item)
    {
        Image image = _cursorTransform.GetComponent<Image>();
        image.sprite = item._itemIcon;
        image.color = Color.white;
    }

    /// <summary>
    /// Method that removes the item from the desired tiles
    /// </summary>
    /// <param name="_tiles"></param>
    public void RemoveItemFromTiles(Vector2[] _tiles)
    {
        
        for (int i = 0; i < _tiles.Length; i++)
        {
            foreach (var tile in tiles)
            {
                if(tile.name.Equals("Tile " + _tiles[i].x + " " + "" + _tiles[i].y))
                {
                    tile.GetComponent<Tile>().IsOcuppied = false;
                    UpdateTileWithItem(tile, false);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Check if the tile is under the cursor
    /// </summary>
    void CheckTilesUnderCursor()
    {
        List<RectTransform> currentlyHoveredTiles = new List<RectTransform>();
        foreach (RectTransform tile in tiles)
        {
            if (IsCursorOverTile(tile))
            {


                if(tile.GetComponent<Tile>().IsOcuppied)
                {
                    OnTileHoverEnter(tile, false);
                }
                else
                {
                    currentlyHoveredTiles.Add(tile);
                    OnTileHoverEnter(tile, true);

                }
                

            }
            else if (!tile.GetComponent<Tile>().IsOcuppied)
            {
                if (hoveredTiles.Contains(tile))
                {
                    OnTileHoverExit(tile);
                }
            }
        }
         hoveredTiles = currentlyHoveredTiles;

        // Obtener el índice del tile bajo el cursor y mostrarlo en la consola
        if (hoveredTiles.Count > 0)
        {
            RectTransform hoveredTile = hoveredTiles[0];
            int tileIndex = tiles.IndexOf(hoveredTile);
        }

    }

    /// <summary>
    /// Instantiates the item in the desired position
    /// </summary>
    /// <param name="item"></param>
    /// <param name="firstTile"></param>
    /// <param name="lastTile"></param>
    void InstantiateItemIcon(GameObject item, Vector2 firstTile, Vector2 lastTile)
    {
        // Posiciona el item y le añade la imagen
        item.transform.SetParent(_inventory);
        item.GetComponent<Image>().sprite = activeItem._itemIcon;
        Vector2[] positions = InsertPositions(firstTile, lastTile);
        gridManager.AddItem(activeItem, positions);
        item.GetComponent<ItemBehaviour>().item = activeItem;
        item.GetComponent<ItemBehaviour>().pos = positions;
        Vector2 itemSize = VisualItemSize(activeItem._size, true);
        item.GetComponent<RectTransform>().sizeDelta = new Vector2(itemSize.x * 2, itemSize.y * 2);
        _cursorTransform.GetComponent<Image>().color = Color.clear;
        activeItem = null;
    }

    #endregion 

    #region Bool Methods

    /// <summary>
    /// Checks if the width of the object is pair
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    bool IsWidthPair(Vector2 size)
    {
        if(size.x % 2 == 0)
        {
            return true;
        }
        else 
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the height of the object is pair
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    bool IsHeightPair(Vector2 size)
    {
        if(size.y % 2 == 0)
        {
            return true;
        }
        else 
        {
            return false;
        }
    }

    /// <summary>
    /// Method that checks if the cursor is over the tile
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    bool IsCursorOverTile(RectTransform tile)
    {
        detectionSize = ItemSize(activeItem._size);
        Vector3[] corners = new Vector3[4];
        tile.GetWorldCorners(corners);

        // Depending of canvasScaleFactor it changes everything
        float canvasScaleFactor = canvas.scaleFactor == 1 ? canvas.scaleFactor - 0.5f : canvas.scaleFactor / 2;

        Rect cursorRect = new Rect(_cursorTransform.position.x - detectionSize.x * canvasScaleFactor, 
                                   _cursorTransform.position.y - detectionSize.y * canvasScaleFactor, 
                                   detectionSize.x * 2 * canvasScaleFactor, 
                                   detectionSize.y * 2 * canvasScaleFactor);



        Rect tileRect = new Rect(corners[0].x, corners[0].y, corners[2].x - corners[0].x, corners[2].y - corners[0].y);

        // Verify if the rentangle of the cursor its above the rentangle of the tile
        return cursorRect.Overlaps(tileRect);
    }

    /// <summary>
    /// Checks if the item can be placed in the desired location
    /// </summary>
    /// <param name="itemSize"></param>
    /// <returns></returns>
    bool CanBePlaced(Vector2 itemSize)
    {
        if(itemSize.x * itemSize.y == hoveredTiles.Count)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the cursor in inside our grid
    /// </summary>
    /// <param name="cursorPosition"></param>
    /// <returns></returns>
    bool IsCursorInsideGrid(Vector2 cursorPosition)
    {
        GridLayoutGroup grid = GetComponentInChildren<GridLayoutGroup>();

        RectTransform gridRectTransform = grid.GetComponent<RectTransform>();

        // Convertir la posición del cursor a un punto local dentro del grid
        RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRectTransform, cursorPosition, null, out Vector2 localPoint);

        // Verificar si el punto local está dentro del rectángulo del grid
        return RectTransformUtility.RectangleContainsScreenPoint(gridRectTransform, cursorPosition, null);
    }

    #endregion

    #region Vector2 Methods

    /// <summary>
    /// Method that gets the next position based on the cursor and the grid
    /// </summary>
    /// <param name="cursorPosition"></param>
    /// <returns></returns>
    Vector2 GetMagnetizedPosition(Vector2 cursorPosition)
    {
        GridLayoutGroup grid = GetComponentInChildren<GridLayoutGroup>();

        // Converts the position of the cursor into the local position inside the grid
        RectTransformUtility.ScreenPointToLocalPointInRectangle(grid.GetComponent<RectTransform>(), cursorPosition, null, out Vector2 localPoint);

        // Changes the color of the image while moving
        _cursorTransform.GetComponent<Image>().color = _imageDragColor;
        _cursorTransform.sizeDelta = VisualItemSize(activeItem._size, false);

        // Gets the grid dimensions
        float cellWidth = grid.cellSize.x + grid.spacing.x;
        float cellHeight = grid.cellSize.y + grid.spacing.y;

        // Gets the local position of the center of the tile in the grid
        Vector2 magnetizedLocalPosition;

        // Gets the closest position in the grid
        int column;
        int row;

        // Depending if the width or the Height is pair gets diferrents positions
        if (IsWidthPair(activeItem._size) && IsHeightPair(activeItem._size))
        {
            column = Mathf.RoundToInt(localPoint.x / cellWidth);
            row = Mathf.RoundToInt(localPoint.y / cellHeight);
            magnetizedLocalPosition = new Vector2(column * cellWidth, row * cellHeight);
        }
        else if (IsWidthPair(activeItem._size) && !IsHeightPair(activeItem._size))
        {
            column = Mathf.FloorToInt(localPoint.x / cellWidth);
            row = Mathf.RoundToInt(localPoint.y / cellHeight);
            magnetizedLocalPosition = new Vector2(column * cellWidth, row * cellHeight + cellHeight / 2);

        }
        else if (!IsWidthPair(activeItem._size) && IsHeightPair(activeItem._size))
        {
            row = Mathf.RoundToInt(localPoint.y / cellHeight);
            column = Mathf.FloorToInt(localPoint.x / cellWidth);
            magnetizedLocalPosition = new Vector2(column * cellWidth + cellWidth / 2, row * cellHeight);
        }
        else
        {
            column = Mathf.FloorToInt(localPoint.x / cellWidth);
            row = Mathf.FloorToInt(localPoint.y / cellHeight);
            magnetizedLocalPosition = new Vector2(column * cellWidth + cellWidth / 2, row * cellHeight + cellHeight / 2);
        }

        // Converts the local position into screen position
        Vector2 magnetizedScreenPosition = LocalToWorldPoint(grid.GetComponent<RectTransform>(), magnetizedLocalPosition);
        

        // If you want the object only moving inside the grid
        if(onlyMoveOnGrid && IsCursorInsideGrid(cursorPosition))
        {
            return magnetizedScreenPosition;
        }
        else if (onlyMoveOnGrid && !IsCursorInsideGrid(cursorPosition))
        {
            return _cursorTransform.position;
        }
        else
        {
            // Else works normal
            return magnetizedScreenPosition;
        }

    }


    /// <summary>
    /// Gets the Icon size and transform it depending of the cellSize, only visual
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    Vector2 VisualItemSize(Vector2 size, bool useCanvasScaleFactor)
    {
        int width = (int)size.x == 1 ? 1 * cellSize : (int)size.x * cellSize;
        int height = (int)size.y == 1 ? 1 * cellSize : (int)size.y * cellSize;
        int widthPercentage = (2 * width) / 100;
        int heightPercentage = (2 * height) / 100;
        float canvasScaleFactor =0;
        if(useCanvasScaleFactor)
        {
            canvasScaleFactor = canvas.scaleFactor == 1 ? canvas.scaleFactor - 0.5f : canvas.scaleFactor / 2;
        }
        else
        {
            canvasScaleFactor = 1;

        }
        return new Vector2((width - widthPercentage) * canvasScaleFactor, (height - heightPercentage) * canvasScaleFactor);
    }

    /// <summary>
    /// Simply transforms the item size depending of the cellSize
    /// </summary>
    /// <param name="itemSize"></param>
    /// <returns></returns>
    Vector2 ItemSize(Vector2 itemSize)
    {
        int width = (int)itemSize.x == 1 ? 1 * cellSize : (int)itemSize.x * cellSize;
        int height = (int)itemSize.y == 1 ? 1 * cellSize : (int)itemSize.y * cellSize;
        float canvasScaleFactor = canvas.scaleFactor == 1 ? canvas.scaleFactor - 0.5f : canvas.scaleFactor / 2;
        return new Vector2(width * canvasScaleFactor, height * canvasScaleFactor);
    }

    /// <summary>
    /// Gets the localPoint into the worldPoint
    /// </summary>
    /// <param name="rectTransform"></param>
    /// <param name="localPoint"></param>
    /// <returns></returns>
    Vector2 LocalToWorldPoint(RectTransform rectTransform, Vector2 localPoint)
    {
        Vector3 worldPoint3D = rectTransform.TransformPoint(localPoint);
        return worldPoint3D;
    }

    /// <summary>
    /// Get all square based of the topLeftTile and the bottomRightTile
    /// </summary>
    /// <param name="_topLeftTile"></param>
    /// <param name="_bottomRightTile"></param>
    /// <returns></returns>
    Vector2[] InsertPositions(Vector2 _topLeftTile, Vector2 _bottomRightTile)
    {
        List<Vector2> tilesPos = new List<Vector2>();
        for (int y = (int)_topLeftTile.y; y <= _bottomRightTile.y; y++)
        {
            for (int x = (int)_topLeftTile.x; x <= _bottomRightTile.x; x++)
            {
                RectTransform tile = tiles.Find(item => item.name == "Tile " + y + " " + "" + x);
                tile.GetComponent<Tile>().IsOcuppied = true;
                UpdateTileWithItem(tile, true);
                tilesPos.Add(new Vector2(Int32.Parse(tile.name.Split(" ")[1]), Int32.Parse(tile.name.Split(" ")[2])));
            }
        }

        return tilesPos.ToArray();
    }

    /// <summary>
    /// Method that gets the center of an square based of all 4 main corners
    /// </summary>
    /// <param name="_topLeftTile"></param>
    /// <param name="_topRightTile"></param>
    /// <param name="_bottomLeftTile"></param>
    /// <param name="_bottomRightTile"></param>
    /// <returns></returns>
    public Vector2 GetCenterPositionOfTiles(Vector2 _topLeftTile, Vector2 _topRightTile, Vector2 _bottomLeftTile, Vector2 _bottomRightTile)
    {
        RectTransform topLeftTile = null;
        RectTransform topRightTile = null;
        RectTransform bottomLeftTile = null; 
        RectTransform bottomRightTile = null;

        // Gets the tiles based on the position on the grid
        foreach (var tile in tiles)
        {
            if(tile.name.Equals("Tile " + _topLeftTile.y + " " + "" + _topLeftTile.x))
            {
                topLeftTile = tile;
            }
            if(tile.name.Equals("Tile " + _topRightTile.y + " " + "" + _topRightTile.x))
            {
                topRightTile = tile;
            }
            if(tile.name.Equals("Tile " + _bottomLeftTile.y + " " + "" + _bottomLeftTile.x))
            {
                bottomLeftTile = tile;
            }
            if(tile.name.Equals("Tile " + _bottomRightTile.y + " " + "" + _bottomRightTile.x))
            {
                bottomRightTile = tile;
            }
        }

        GridLayoutGroup grid = GetComponentInChildren<GridLayoutGroup>();
        
        // Gets corners of tiles in world space
        Vector3[] topLeftCorners = new Vector3[4];
        topLeftTile.GetWorldCorners(topLeftCorners);

        Vector3[] topRightCorners = new Vector3[4];
        topRightTile.GetWorldCorners(topRightCorners);

        Vector3[] bottomLeftCorners = new Vector3[4];
        bottomLeftTile.GetWorldCorners(bottomLeftCorners);

        Vector3[] bottomRightCorners = new Vector3[4];
        bottomRightTile.GetWorldCorners(bottomRightCorners);

        // Gets the center of an area defined by 4 tiles
        float centerX = (topLeftCorners[0].x + topRightCorners[3].x + bottomLeftCorners[0].x + bottomRightCorners[3].x) / 4;
        float centerY = (topLeftCorners[0].y + topRightCorners[3].y + bottomLeftCorners[0].y + bottomRightCorners[3].y) / 4;

        float canvasScaleFactor = canvas.scaleFactor == 1 ? canvas.scaleFactor - 0.5f : canvas.scaleFactor / 2;


        centerY += cellSize * canvasScaleFactor;

        // Convert the position of the center into a local point inside the grid
        Vector2 localCenterPoint = grid.GetComponent<RectTransform>().InverseTransformPoint(new Vector3(centerX, centerY, 0));

        // Converts the local point into screen position
        Vector2 screenCenterPoint = LocalToWorldPoint(grid.GetComponent<RectTransform>(), localCenterPoint);

        return screenCenterPoint;
    }

    /// <summary>
    /// Method that get all 4 corners of our object
    /// </summary>
    /// <param name="positions"></param>
    /// <returns>An array of Vector2 with all 4 corners</returns>
    Vector2[] GetCorners(Vector2[] positions)
    {
        Vector2[] corners = new Vector2[4];
        corners[0] = positions[0];
        corners[3] = positions[positions.Length - 1];
        for (int i = 0; i < positions.Length; i++)
        {
            if(i + 1 == positions.Length / 2)
            {
                corners[1] = positions[i];
            }
            
            if(i == positions.Length / 2)
            {
                corners[2] = positions[i];

            }
        }
        return corners;
    }

    #endregion

    #region Tile Color Methods
    /// <summary>
    /// Change Tile hover color
    /// </summary>
    void UpdateTileHover()
    {
        foreach (RectTransform tile in hoveredTiles)
        {
            RawImage tileImage = tile.GetComponent<RawImage>();
            if(CanBePlaced(activeItem._size))
            {
                if (tileImage != null)
                {
                    tileImage.color = _tileHoverColor;

                }
            }
            else
            {
                if(_cursorTransform.position == lastPos)
                {
                    StartCoroutine(SetToOccupied());
                }
                else
                {
                    IsMoving = true;
                    StopAllCoroutines();
                }
                lastPos = _cursorTransform.position;

                if(!IsMoving)
                {
                    tileImage.color = _tileOcuppiedColor;

                }
            }
        }
        
    }

    IEnumerator SetToOccupied()
    {
        yield return new WaitForSeconds(0.05f);
        IsMoving = false;
    }

    ///<summary>
    /// Update tile color depending if its empty or not
    ///</summary>
    ///<param name="tile"> RectTransform for the Tile </param>
    ///<param name="hasItem"> Bool if the tile hasItem </param>
    void UpdateTileWithItem(RectTransform tile, bool hasItem)
    {
        RawImage tileImage = tile.GetComponent<RawImage>();
        if (tileImage != null)
        {
            tileImage.color = hasItem ? _tileNotEmptyColor : _tileEmptyColor;
        }
    }

    /// <summary>
    /// Change the tile color on hover
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="_isOcuppied"></param>
    void OnTileHoverEnter(RectTransform tile, bool _isOcuppied)
    {
        Image tileImage = tile.GetComponent<Image>();
        if (tileImage != null)
        {
            tileImage.color = _isOcuppied ? _tileOcuppiedColor : _tileHoverColor;
        }
    }

    /// <summary>
    /// Change tile color when hover exit
    /// </summary>
    /// <param name="tile"></param>
    void OnTileHoverExit(RectTransform tile)
    {
        // Lógica cuando el cursor sale de un tile
        RawImage tileImage = tile.GetComponent<RawImage>();
        if (tileImage != null)
        {
            tileImage.color = _tileEmptyColor;
        }
    }

    #endregion
}
