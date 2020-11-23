using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    [SerializeField] private int width = 7;
    [SerializeField] private int height = 9;
    [SerializeField] private int borderSize = 2;
    [SerializeField] private float swapTime = 0.4f;
    [Space]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject[] gamePiecePrefabs;

    private Tile[,] _allTiles;
    private GamePiece[,] _allGamePieces;

    private Tile _clickedTile;
    private Tile _targetTile;

    private void Start()
    {
        _allTiles = new Tile[width, height];
        _allGamePieces = new GamePiece[width,height];
        
        SetupTiles();
        SetupCamera();
        FillRandom();
    }

    private void SetupTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject tile = Instantiate(tilePrefab,new Vector3(i,j,0), Quaternion.identity) as GameObject;
                tile.name = "Tile (" + i + "," + j + ")";

                _allTiles[i,j] = tile.GetComponent<Tile>();

                tile.transform.parent = transform;
                _allTiles[i,j].Init(i,j, this);
            }
        }
    }
    
    private void SetupCamera()
    {
        Camera.main.transform.position = new Vector3((width - 1)/2f, (height - 1)/2f, -10);

        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = height / 2f + borderSize;
        float horizontalSize = width / 2f + borderSize;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }

    private GameObject GetRandomGamePiece()
    {
        int randIndex = Random.Range(0, gamePiecePrefabs.Length);

        if (gamePiecePrefabs[randIndex] == null)
        {
            Debug.LogWarning("BOARD: " + randIndex + " does not contain a valid GamePiece prefab!");
        }

        return gamePiecePrefabs[randIndex];
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("BOARD: Invalid GamePiece prefab!");
            return;
        }
        
        gamePiece.transform.position = new Vector3(x, y,0);
        gamePiece.transform.rotation = Quaternion.identity;
        if (IsWithinBounds(x, y))
        {
            _allGamePieces[x, y] = gamePiece;
        }
        gamePiece.SetCoord(x,y);
    }

    public bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    private void FillRandom()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity);

                if (randomPiece != null)
                {
                    randomPiece.GetComponent<GamePiece>().Init(this);
                    PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), i, j);
                    randomPiece.transform.parent = transform;
                }
            }
        }
    }

    public void ClickTile(Tile tile)
    {
        if (_clickedTile == null)
        {
            _clickedTile = tile;
        }
    }

    public void DragToTile(Tile tile)
    {
        if (_clickedTile != null && IsNextTo(_clickedTile, tile))
        {
            _targetTile = tile;
        }
    }

    public void ReleaseTile()
    {
        if (_clickedTile != null && _targetTile != null)
        {
            SwitchTiles(_clickedTile, _targetTile);
        }
        
        _clickedTile = null;
        _targetTile = null;
    }

    private void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        GamePiece clickedPiece = _allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
        GamePiece targetPiece = _allGamePieces[targetTile.xIndex, targetTile.yIndex];
        
        clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
        targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
    }

    private bool IsNextTo(Tile start, Tile end)
    {
        if (Math.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }
        
        if (Math.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
        {
            return true;
        }

        return false;
    }
}


