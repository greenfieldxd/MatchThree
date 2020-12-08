using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    public int xIndex { get; private set; }
    public int yIndex { get; private set; }
    
    public MatchValue matchValue;
    
    private Board _board;
    private bool _isMoving = false;


    public enum MatchValue
    {
        Yellow,
        Blue,
        Magenta,
        Indigo,
        Green,
        Teal,
        Red,
        Cyan,
        Wild
    }
    
    public void Init(Board board)
    {
        _board = board;
    }
    
    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }
    
    public void Move(int destX, int destY, float timeToMove)
    {
        if (!_isMoving)
        {
            _isMoving = true;
            
            transform.DOMove(new Vector3(destX, destY, 0), timeToMove).SetEase(Ease.InOutQuint).OnComplete((() =>
            {
                _board.PlaceGamePiece(this, destX, destY);
                _isMoving = false;
            }));
        }
    }
}
