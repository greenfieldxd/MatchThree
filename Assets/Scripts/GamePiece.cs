using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    [SerializeField] private int xIndex;
    [SerializeField] private int yIndex;
    
    private Board _board;
    
    private bool _isMoving = false;

    
    public void Init(Board board)
    {
        _board = board;
    }
    
    public void SetCoord(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }

    private void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move((int)(transform.position.x - 2),(int)(transform.position.y), 0.5f);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move((int)(transform.position.x + 2),(int)(transform.position.y), 0.5f);
        }
        */
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
