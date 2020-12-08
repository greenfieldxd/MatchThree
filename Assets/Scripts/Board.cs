using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        if (IsWithInBounds(x, y))
        {
            _allGamePieces[x, y] = gamePiece;
        }
        gamePiece.SetCoord(x,y);
    }

    public bool IsWithInBounds(int x, int y)
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
	    StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
    }

    private IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
	    GamePiece clickedPiece = _allGamePieces[clickedTile.xIndex, clickedTile.yIndex];
	    GamePiece targetPiece = _allGamePieces[targetTile.xIndex, targetTile.yIndex];

	    if (targetPiece != null && clickedPiece != null)
	    {
		    clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
		    targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);

		    yield return new WaitForSeconds(swapTime);

		    List<GamePiece> clickedPiecesMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
		    List<GamePiece> targetPiecesMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);

		    if (clickedPiecesMatches.Count == 0 && targetPiecesMatches.Count == 0)
		    {
			    clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
			    targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
		    }
		    else
		    {
			    yield return new WaitForSeconds(swapTime);

			    ClearPieceAt(clickedPiecesMatches);
			    ClearPieceAt(targetPiecesMatches);
			    
			    //HighLightMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
			    //HighLightMatchesAt(targetTile.xIndex, targetTile.yIndex);  
		    }
	    }
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

    private List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
	{
		List<GamePiece> matches = new List<GamePiece>();

		GamePiece startPiece = null;

		if (IsWithInBounds(startX, startY))
		{
			startPiece = _allGamePieces[startX, startY];
		}

		if (startPiece != null)
		{
			matches.Add(startPiece);
		}
		else
		{
			return null;
		}

		int nextX;
		int nextY;

		int maxValue = (width > height) ? width: height;

		for (int i = 1; i < maxValue - 1; i++)
		{
			nextX = startX + (int) Mathf.Clamp(searchDirection.x, -1, 1) * i;
			nextY = startY + (int) Mathf.Clamp(searchDirection.y, -1, 1) * i;

			if (!IsWithInBounds(nextX, nextY))
			{
				break;
			}

			GamePiece nextPiece = _allGamePieces[nextX, nextY];

			if (nextPiece == null)
			{
				break;
			}
			else
			{
				if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
				{
					matches.Add(nextPiece);
				}
				else
				{
					break;
				}
			}
		}

		if (matches.Count >= minLength)
		{
			return matches;
		}
			
		return null;

	}

	private List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
	{
		List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0,1), 2);
		List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0,-1), 2);

		if (upwardMatches == null)
		{
			upwardMatches = new List<GamePiece>();
		}

		if (downwardMatches == null)
		{
			downwardMatches = new List<GamePiece>();
		}

		var combinedMatches = upwardMatches.Union(downwardMatches).ToList();

		return (combinedMatches.Count >= minLength) ? combinedMatches : null;

	}

	private List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
	{
		List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1,0), 2);
		List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1,0), 2);

		if (rightMatches == null)
		{
			rightMatches = new List<GamePiece>();
		}

		if (leftMatches == null)
		{
			leftMatches = new List<GamePiece>();
		}

		var combinedMatches = rightMatches.Union(leftMatches).ToList();

		return (combinedMatches.Count >= minLength) ? combinedMatches : null;

	}
	
	private List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
	{
		List<GamePiece> horizMatches = FindHorizontalMatches(x, y, minLength);
		List<GamePiece> vertMatches = FindVerticalMatches(x, y, minLength);

		if (horizMatches == null)
		{
			horizMatches = new List<GamePiece>();
		}

		if (vertMatches == null)
		{
			vertMatches = new List<GamePiece>();
		}

		var combinedMatches = horizMatches.Union(vertMatches).ToList();
		return combinedMatches;
	}

	private void HighLightOff(int x, int y)
	{
		SpriteRenderer spriteRenderer = _allTiles[x,y].GetComponent<SpriteRenderer>();
		spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
	}

	private void HighLightOn(int x, int y, Color color)
	{
		SpriteRenderer spriteRenderer = _allTiles[x, y].GetComponent<SpriteRenderer>();
		spriteRenderer.color = color;
	}
	
	private void HighLightMatchesAt(int x, int y)
	{
		HighLightOff(x, y);

		var combinedMatches = FindMatchesAt(x, y);

		if (combinedMatches.Count > 0)
		{
			foreach (GamePiece piece in combinedMatches)
			{
				HighLightOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
			}
		}
	}

	private void HighlightMatches()
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				HighLightMatchesAt(i, j);
			}
		}
	}

	private void ClearPieceAt(int x, int y)
	{
		GamePiece pieceToClear = _allGamePieces[x, y];

		if (pieceToClear != null)
		{
			_allGamePieces[x, y] = null;
			Destroy(pieceToClear.gameObject);
		}

		HighLightOff(x, y);
	}

	private void ClearBoard()
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				ClearPieceAt(i,j);
			}
		}
	}

	private void ClearPieceAt(List<GamePiece> gamePieces)
	{
		foreach (var piece in gamePieces)
		{
			ClearPieceAt(piece.xIndex, piece.yIndex);
		}
	}


}


