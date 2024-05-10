using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTranslator
{
    public enum PathDirection
    { 
        None = 0,
        Up = 1,
        Down = 2,
        Left = 3,
        Right = 4,
        TopLeft = 5,
        BottomLeft = 6,
        TopRight = 7,
        BottomRight = 8
    }

    public PathDirection TranslateDirection(OverlayTile previousTile, OverlayTile currentTile, OverlayTile futureTile)
    {
        bool isFinal = futureTile == null;

        Vector2Int pastDirection = previousTile != null ? (Vector2Int)(currentTile.gridLocation - previousTile.gridLocation) : new Vector2Int(0, 0);
        Vector2Int futureDirection = futureTile != null ? (Vector2Int)(futureTile.gridLocation - currentTile.gridLocation) : new Vector2Int(0, 0);
        Vector2Int direction = pastDirection != futureDirection ? pastDirection + futureDirection : futureDirection;

        if (direction == new Vector2(0, 1) && !isFinal)
        {
            return PathDirection.Up;
        }

        if (direction == new Vector2(0, -1) && !isFinal)
        {
            return PathDirection.Down;
        }

        if (direction == new Vector2(1, 0) && !isFinal)
        {
            return PathDirection.Right;
        }

        if (direction == new Vector2(-1, 0) && !isFinal)
        {
            return PathDirection.Left;
        }

        if (direction == new Vector2(1, 1))
        {
            if(pastDirection.y < futureDirection.y)
                return PathDirection.BottomLeft;
            else
                return PathDirection.TopRight;
        }

        if (direction == new Vector2(-1, 1))
        {
            if (pastDirection.y < futureDirection.y)
                return PathDirection.BottomRight;
            else
                return PathDirection.TopLeft;
        }

        if (direction == new Vector2(1, -1))
        {
            if (pastDirection.y > futureDirection.y)
                return PathDirection.TopLeft;
            else
                return PathDirection.BottomRight;
        }

        if (direction == new Vector2(-1, -1))
        {
            if (pastDirection.y > futureDirection.y)
                return PathDirection.TopRight;
            else
                return PathDirection.BottomLeft;
        }

        if (direction == new Vector2(0, 1) && isFinal)
        {
            return PathDirection.None;
        }

        if (direction == new Vector2(0, -1) && isFinal)
        {
            return PathDirection.None;
        }

        if (direction == new Vector2(-1, 0) && isFinal)
        {
            return PathDirection.None;
        }

        if (direction == new Vector2(1, 0) && isFinal)
        {
            return PathDirection.None;
        }

        return PathDirection.None;
        }
    
}
