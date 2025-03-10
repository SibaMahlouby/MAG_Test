using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using System;

public class BoardManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public int boardWidth = 5;
    public int boardHeight = 5;
    private Tile[,] board;
    private float horizontalSpacing = 1.0f;
    private float verticalSpacing = 1.0f;
    private Color[] tileColors = new Color[] { Color.blue, Color.red, Color.green, Color.yellow };

    void Start()
    {
        GenerateBoard();
    }
        public void GenerateBoard()
    {
        board = new Tile[boardWidth, boardHeight];
        GameObject boardContainer = new GameObject("BoardContainer");

        // Calculate offsets to center the board
        float offsetX = -(boardWidth - 1) * horizontalSpacing / 2f;
        float offsetY = -(boardHeight - 1) * verticalSpacing / 2f;

        for (int x = 0; x < boardWidth; x++)
        {
            for (int y = 0; y < boardHeight; y++)
            {
                // Calculate position for each tile
                Vector3 position = new Vector3(
                    x * horizontalSpacing + offsetX,
                    y * verticalSpacing + offsetY,
                    -y // Set Z-position based on row (lower rows are behind higher rows)
                );

                // Instantiate the tile
                GameObject tileObject = Instantiate(tilePrefab, position, Quaternion.identity);
                tileObject.transform.parent = boardContainer.transform;

                // Set up the tile
                Tile tile = tileObject.GetComponent<Tile>();
                tile.SetTileColor(tileColors[UnityEngine.Random.Range(0, tileColors.Length)]);
                tile.SetGridPosition(x, y); // Store grid position
                board[x, y] = tile;
                tile.OnTileClicked += HandleTileClick;
            }
        }

        // Center the board container
        boardContainer.transform.position = Vector3.zero;
    }


    public void HandleTileClick(Tile clickedTile)
    {
        int clickedX = clickedTile.GetGridX();
        int clickedY = clickedTile.GetGridY();
        Color clickedColor = clickedTile.GetTileColor();
        List<Tile> tilesToPop = new List<Tile>();
        HashSet<int> affectedColumns = new HashSet<int>(); // Track affected columns

        FloodFill(clickedX, clickedY, clickedColor, tilesToPop, affectedColumns);

        if (tilesToPop.Count > 1)
        {
            foreach (var tile in tilesToPop)
            {
                tile.gameObject.transform.DOScale(Vector3.zero, 0.3f).OnComplete(() =>
                {
                    Destroy(tile.gameObject);
                });
                board[tile.GetGridX(), tile.GetGridY()] = null; // Remove from board
            }
            StartCoroutine(RegenerateBoard(affectedColumns));
        }
    }

    private void FloodFill(int x, int y, Color color, List<Tile> tilesToPop, HashSet<int> affectedColumns)
    {
        if (x < 0 || x >= boardWidth || y < 0 || y >= boardHeight || board[x, y] == null || board[x, y].GetTileColor() != color || tilesToPop.Contains(board[x, y]))
            return;

        tilesToPop.Add(board[x, y]);
        affectedColumns.Add(x); // Track affected column

        FloodFill(x - 1, y, color, tilesToPop, affectedColumns);
        FloodFill(x + 1, y, color, tilesToPop, affectedColumns);
        FloodFill(x, y - 1, color, tilesToPop, affectedColumns);
        FloodFill(x, y + 1, color, tilesToPop, affectedColumns);
    }




    private IEnumerator RegenerateBoard(HashSet<int> affectedColumns)
    {
        // Step 1: Wait for popping animation to complete
        yield return new WaitForSeconds(0.3f); // Delay after popping

        // Step 2: Shift tiles downward in affected columns
        foreach (int x in affectedColumns)
        {
            // Iterate from the bottom to the top of the column
            for (int y = boardHeight - 1; y >= 0; y--)
            {
                if (board[x, y] != null)
                {
                    // Move the tile downward as far as possible
                    yield return StartCoroutine(MoveTileDown(x, y));
                }
            }
        }

        // Step 3: Generate new tiles at the top of affected columns
        foreach (int x in affectedColumns)
        {
            int emptySpaces = 0;

            // Count empty spaces in the column
            for (int y = 0; y < boardHeight; y++)
            {
                if (board[x, y] == null)
                {
                    emptySpaces++;
                }
            }

            // Generate new tiles at the top of the column
            for (int i = 0; i < emptySpaces; i++)
            {
                int newY = boardHeight - emptySpaces + i;

                // Calculate the spawn position above the board
                Vector2 spawnPosition = new Vector2(
                    x * horizontalSpacing - (boardWidth - 1) * horizontalSpacing / 2f,
                    (boardHeight + i) * verticalSpacing - (boardHeight - 1) * verticalSpacing / 2f
                );

                // Instantiate the new tile
                GameObject newTileObject = Instantiate(tilePrefab, spawnPosition, Quaternion.identity);
                Tile newTile = newTileObject.GetComponent<Tile>();
                newTile.SetTileColor(tileColors[UnityEngine.Random.Range(0, tileColors.Length)]);
                newTile.SetGridPosition(x, newY);
                board[x, newY] = newTile;
                newTile.OnTileClicked += HandleTileClick;

                // Set the target position for the new tile to drop to
                Vector2 targetPosition = new Vector2(
                    x * horizontalSpacing - (boardWidth - 1) * horizontalSpacing / 2f,
                    -(newY * verticalSpacing - (boardHeight - 1) * verticalSpacing / 2f)
                );

                // Move the new tile to the target position
                yield return StartCoroutine(MoveTileToPosition(newTile, targetPosition, 5f));
            }
        }

        // Step 4: Wait for new tiles to finish dropping
        yield return new WaitForSeconds(0.5f); // Delay after moving up
    }


    private IEnumerator MoveTileDown(int x, int y)
    {
        while (y > 0 && board[x, y - 1] == null) // Check if there's an empty space below
        {
            int newY = y - 1;
            board[x, newY] = board[x, y]; // Update the board array
            board[x, y] = null; // Clear the old position

            // Set the target position for the tile to fall to
            Vector2 targetPosition = new Vector2(
                x * horizontalSpacing - (boardWidth - 1) * horizontalSpacing / 2f,
                -(newY * verticalSpacing - (boardHeight - 1) * verticalSpacing / 2f)
            );

            // Move the tile to the target position
            yield return StartCoroutine(MoveTileToPosition(board[x, newY], targetPosition, 5f));

            // Update the tile's grid position
            board[x, newY].SetGridPosition(x, newY);

            // Update the current y position
            y = newY;
        }
    }


    private IEnumerator MoveTileToPosition(Tile tile, Vector2 targetPosition, float speed)
    {
        Rigidbody2D tileRigidbody = tile.GetComponent<Rigidbody2D>();
        tileRigidbody.velocity = new Vector2(0, -speed); // Move downward

        // Wait until the tile reaches the target position
        while (Mathf.Abs(tile.transform.position.y - targetPosition.y) > 0.1f) // Use an epsilon value for smooth stopping
        {
            yield return null;
        }

        // Stop the tile and disable physics
        tileRigidbody.velocity = Vector2.zero;
        tileRigidbody.isKinematic = true; // Disable physics
        tile.transform.position = targetPosition; // Snap to the exact position
    }

}