using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int xIndex { get; private set; }
    public int yIndex { get; private set; }

    private Board _board;


    public void Init(int x, int y, Board board)
    {
        xIndex = x;
        yIndex = y;
        _board = board;
    }

    private void OnMouseDown()
    {
        if (_board != null)
        {
            _board.ClickTile(this);
        }
    }

    private void OnMouseEnter()
    {
        if (_board != null)
        {
            _board.DragToTile(this);
        }
    }

    private void OnMouseUp()
    {
        if (_board != null)
        {
            _board.ReleaseTile();
        }
    }
}
