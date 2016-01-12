using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class BoardManager: MonoBehaviour
{

	[Serializable]
	public class Count
	{
		public int minimum;
		public int maximum;

		public Count (int min, int max)
		{

			minimum = min;
			maximum = max;

		}
	}

	// Size of the game board
	public int columns = 8;
	public int rows = 8;
	public int increaseSizeEvery = 5;					// Increase size board every 5 levels
	public int increaseAmount = 4;						// Amount of increase of the board size
	public Count wallCount = new Count (5, 9);			// Minimum of 5 walls per lvl, max 9
	public Count foodCount = new Count (1, 5);			// Same for food
	public Count weapondCount = new Count (-2, 1);		// Same for weapons

	public GameObject exit;
	public GameObject[] enemyTiles;
	public GameObject[] floorTiles;
	public GameObject[] foodTiles;
	public GameObject[] wallTiles;
	public GameObject[] weaponTiles;
	public GameObject[] outerWallTiles;
	private Transform _boardHolder;
	private List<Vector3> _gridPositions = new List<Vector3> ();		// Track all the possible positions on the board

	private int _curColumns;
	private int _curRows;
	
	void InitializeList ()
	{

		// Reset List
		_gridPositions.Clear ();
	
		// Fill the list, leaving 1 empty border to make sure the level is always solvable
	
		for (int x = 1; x < _curColumns - 1; x++)
			for (int y = 1; y < _curRows - 1; y++)
				_gridPositions.Add (new Vector3 (x, y, 0f));
			
		
	}

	void BoardSetup (int level)
	{
		_boardHolder = new GameObject ("Board").transform;

		// Calculate current level board size
		_curColumns = columns + (((int)(level / increaseSizeEvery)) * increaseAmount);
		_curRows = rows + (((int)(level / increaseSizeEvery)) * increaseAmount);

		Debug.Log ("Current level:  " + level + " - Board size is: " + _curColumns + "," + _curRows);

		// Creating the board edges around the playable area
		for (int x = -1; x < _curColumns + 1; x++)
			for (int y = -1; y < _curRows + 1; y++) {

				GameObject curTile;
				
				// Randomly select an outer wall or a floor tile
				if (x == -1 || x == _curColumns || y == -1 || y == _curRows)
					curTile = outerWallTiles [Random.Range (0, outerWallTiles.Length)];
				else
					curTile = floorTiles [Random.Range (0, floorTiles.Length)];

				// Instatiate it
				GameObject tileInstance = Instantiate (curTile, new Vector3 (x, y, 0f), Quaternion.identity) as GameObject;
				tileInstance.transform.SetParent (_boardHolder);
			}

	}

	// Location to spawn a new item (food, soda, enemy, wall)
	Vector3 RandomPosition ()
	{
		// Randomly pick a location from the available ones in the grid
		int randomIndex = Random.Range (0, _gridPositions.Count);
		Vector3 randomPositions = _gridPositions [randomIndex];

		// Remove the picked position from the grid (to avoid having 2 items on the same spot)
		_gridPositions.RemoveAt (randomIndex);

		return randomPositions;
	}

	void LayoutObjectAtRandom (GameObject[] tiles, int min, int max)
	{

		// Randomly pick the number of objects to be placed betwee min and max
		int objectCount = Random.Range (min, max + 1);

		if (objectCount > 0)
			// For each object, randomly position/instatiate it
			for (int i = 0; i < objectCount; i++)
				Instantiate (tiles [Random.Range (0, tiles.Length)], RandomPosition (), Quaternion.identity);

	}

	public void SetupScene (int level)
	{
		BoardSetup (level);
		InitializeList ();
		LayoutObjectAtRandom (wallTiles, wallCount.minimum, wallCount.maximum);
		LayoutObjectAtRandom (foodTiles, foodCount.minimum, foodCount.maximum);

		// Every 5 levels, always spawn a weapon
		if (level % 5 != 0)
			LayoutObjectAtRandom (weaponTiles, weapondCount.minimum, weapondCount.maximum);
		else
			LayoutObjectAtRandom (weaponTiles, 1, weapondCount.maximum);

		// Progressively increase the amount of enemies in a level [log(curLvl)]
		int enemyCount = (int)Mathf.Log (level, 2f);
		LayoutObjectAtRandom (enemyTiles, enemyCount, enemyCount);

		// Position exit at the upper-rightmost tile of the grid
		Instantiate (exit, new Vector3 (_curColumns - 1, _curRows - 1, 0f), Quaternion.identity);

	}
}
