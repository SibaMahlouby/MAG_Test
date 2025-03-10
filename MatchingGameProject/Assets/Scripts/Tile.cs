using UnityEngine;
using System;

public class Tile : MonoBehaviour
{
    public Color tileColor;  // The color of the tile
    public event Action<Tile> OnTileClicked;  // Event to handle tile click
    public Vector2Int gridPosition;  // Position of the tile on the grid
    private int gridX;
    private int gridY;
    // Method to set the tile color
    public void SetTileColor(Color color)
    {
        tileColor = color;
        // Set the color of the tile (e.g., the sprite renderer or UI element)
        GetComponent<SpriteRenderer>().color = color;
    }
    public Color GetTileColor()
    {
        return tileColor;
    }

    public void SetGridPosition(int x, int y)
    {
        gridX = x;
        gridY = y;
    }

    public int GetGridX()
    {
        return gridX;
    }

    public int GetGridY()
    {
        return gridY;
    }

    // Detect tile click
    private void OnMouseDown()
    {
        // If the tile is clicked, trigger the OnTileClicked event
        OnTileClicked?.Invoke(this);
    }
}
