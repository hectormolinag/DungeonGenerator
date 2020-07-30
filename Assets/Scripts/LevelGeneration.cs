using System.Collections.Generic;
using UnityEngine;

public class LevelGeneration : MonoBehaviour {

    [SerializeField] int numberOfRooms = 20;
    [SerializeField] GameObject mapSpritePrefab = null;
    [SerializeField] List<GameObject> roomsList = new List<GameObject>();
    
	private readonly Vector2 worldSize = new Vector2(4,4);
	private readonly List<Vector2> takenPositions = new List<Vector2>();
	
	private Room[,] rooms;
	private int gridSizeX;
	private int gridSizeY;
	
    

	void Start () 
    {
	    if (numberOfRooms >= (worldSize.x * 2) * (worldSize.y * 2)) 
			numberOfRooms = Mathf.RoundToInt((worldSize.x * 2) * (worldSize.y * 2));
		
		gridSizeX = Mathf.RoundToInt(worldSize.x); 
		gridSizeY = Mathf.RoundToInt(worldSize.y);
		
		CreateRooms(); 
		SetRoomDoors(); 
		DrawMap();
		PutBoss();
	}

	void CreateRooms()
    {
		rooms = new Room[gridSizeX * 2,gridSizeY * 2];
		rooms[gridSizeX,gridSizeY] = new Room(Vector2.zero, 1);
		takenPositions.Insert(0,Vector2.zero);

		float randomCompareStart = 0.2f;
		float randomCompareEnd = 0.01f;
		
		for (int i =0; i < numberOfRooms -1; i++)
		{
			float randomPercent = ((float) i) / (((float)numberOfRooms - 1));
			float randomCompare = Mathf.Lerp(randomCompareStart, randomCompareEnd, randomPercent);
			
			//grab new position
			Vector2 checkPos = NewPosition();
			//test new position
			if (NumberOfNeighbors(checkPos, takenPositions) > 1 && Random.value > randomCompare){
				int iterations = 0;
				do{
					checkPos = SelectiveNewPosition();
					iterations++;
				}while(NumberOfNeighbors(checkPos, takenPositions) > 1 && iterations < 100);
				if (iterations >= 50)
					print("error: could not create with fewer neighbors than : " + NumberOfNeighbors(checkPos, takenPositions));
			}
			
			//finalize position
			rooms[(int) checkPos.x + gridSizeX, (int) checkPos.y + gridSizeY] = new Room(checkPos, 0);
			takenPositions.Insert(0,checkPos);
		}	
	}

	Vector2 NewPosition()
	{
		int x = 0;
		int y = 0;
		Vector2 checkingPos;
		
		do
		{
			int index = Mathf.RoundToInt(Random.value * (takenPositions.Count - 1)); 
			x = (int) takenPositions[index].x;
			y = (int) takenPositions[index].y;
			//Pick whether to look on horizontal or vertical axis
			bool UpDown = (Random.value < 0.5f);
			
			//Pick whether to be positive or negative on that axis
			bool positive = (Random.value < 0.5f);
			
			//find the position based on the above booleans
			if (UpDown)
			{ 
				if (positive)
					y += 1;
				else
					y -= 1;
			}
			else
			{
				if (positive)
					x += 1;
				else
					x -= 1;
			}
			
			checkingPos = new Vector2(x,y); 
			//make sure the position is valid
		}while (takenPositions.Contains(checkingPos) || x >= gridSizeX || x < -gridSizeX || y >= gridSizeY || y < -gridSizeY); 
		
		return checkingPos;
	}


	Vector2 SelectiveNewPosition()
	{
		int index = 0;
		int increase = 0;
		int x = 0; 
		int y = 0;
		Vector2 checkingPos;
		
		do
		{
			increase = 0;
			do
			{ 
				//start with one that only has one neighbor. This will make it more likely that it returns a room that branches out
				index = Mathf.RoundToInt(Random.value * (takenPositions.Count - 1));
				increase ++;
			}while (NumberOfNeighbors(takenPositions[index], takenPositions) > 1 && increase < 100);
			
			x = (int) takenPositions[index].x;
			y = (int) takenPositions[index].y;
			bool UpDown = (Random.value < 0.5f);
			bool positive = (Random.value < 0.5f);
			
			if (UpDown)
			{
				if (positive)
					y += 1;
				else
					y -= 1;
				
			}
			else
			{
				if (positive)
					x += 1;
				else
					x -= 1;
				
			}
			
			checkingPos = new Vector2(x,y);
			
		}while (takenPositions.Contains(checkingPos) || x >= gridSizeX || x < -gridSizeX || y >= gridSizeY || y < -gridSizeY);
		
		// break loop if it takes too long
		if (increase >= 100) 
			print("Error: could not find position with only one neighbor");
		return checkingPos;
	}


	int NumberOfNeighbors(Vector2 checkingPos, List<Vector2> usedPositions)
    {
	    // start at zero, add 1 for each side there is already a room
		int r = 0; 
		
		if (usedPositions.Contains(checkingPos + Vector2.right)){ 
			r++;
		}
		if (usedPositions.Contains(checkingPos + Vector2.left)){
			r++;
		}
		if (usedPositions.Contains(checkingPos + Vector2.up)){
			r++;
		}
		if (usedPositions.Contains(checkingPos + Vector2.down)){
			r++;
		}
		return r;
	}


	void DrawMap()
    {
		foreach (Room room in rooms)
		{
			//skip where there is no room
			if (room == null)
				continue; 
			
			Vector2 drawPos = room.gridPos;
			drawPos.x *= 16;
			drawPos.y *= 8;
			
			MapSpriteSelector mapper = Instantiate(mapSpritePrefab, drawPos, Quaternion.identity).GetComponent<MapSpriteSelector>();
			mapper.transform.parent = transform;
			mapper.type = room.type;
			mapper.up = room.doorTop;
			mapper.down = room.doorBot;
			mapper.right = room.doorRight;
			mapper.left = room.doorLeft;

            roomsList.Add(mapper.gameObject);
		}
	}


	void SetRoomDoors()
    {
		for (int x = 0; x < ((gridSizeX * 2)); x++){
			for (int y = 0; y < ((gridSizeY * 2)); y++){
				
				if (rooms[x,y] == null)
					continue;
				
				Vector2 gridPosition = new Vector2(x,y);
				
				//UP
				if (y - 1 < 0) 
					rooms[x,y].doorBot = false;
				else
					rooms[x,y].doorBot = (rooms[x,y-1] != null);
				
				//DOWN
				if (y + 1 >= gridSizeY * 2) 
					rooms[x,y].doorTop = false;
				else
					rooms[x,y].doorTop = (rooms[x,y+1] != null);
				
				//LEFT
				if (x - 1 < 0)
					rooms[x,y].doorLeft = false;
				else
					rooms[x,y].doorLeft = (rooms[x - 1,y] != null);
				
				//RIGHT
				if (x + 1 >= gridSizeX * 2) 
					rooms[x,y].doorRight = false;
				else
					rooms[x,y].doorRight = (rooms[x+1,y] != null);
				
			}
		}
	}

    void PutBoss()
    {
        if(roomsList.Count == numberOfRooms)
        {
            if (Random.Range(0, 2) != 1)
                roomsList[roomsList.Count-1].GetComponent<MapSpriteSelector>().type = 2;
            else
                roomsList[0].GetComponent<MapSpriteSelector>().type = 2;
        }
    }

    public void ReloadScene()
    {
	    UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
