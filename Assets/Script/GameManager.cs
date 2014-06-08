//--------------------------------------------
// Jimmy Liu
// OWNSELF
// 2013.6.28
//--------------------------------------------
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

//Class for general manager of this Tetris
public class GameManager : MonoBehaviour {

    //static variables for game play
    static public int Xmax = 10;   //Screen size of game
    static public int Ymax = 22;
    //These are the different rewards when player finish 1 line or 2 or 3 or 4
    public int reward1 = 10;
    public int reward2 = 30;
    public int reward3 = 100;
    public int reward4 = 500;
    //Increase the speed when how many blocks dropped
    public int levelThreshold = 30;
    public float speed = 0.5f; //init speed
    public float speedIncrement = 0.05f; //Speed every time increased

    static int score = 0;   //Score
    static int RowsFinished = 0;   //Rows finished of game
    static int level = 0;   //Progress of game
    static int typeofBlocks = 7;    //Types of blocks  ######
    Block[] database;   //Store the basic info of blocks

    //Some help static variable
    static bool dropping = false;
    static bool movingLeft = false;
    static bool movingRight = false;

    bool gameFinished = false;

    int width = Screen.width;
    int height = Screen.height;

    //Screen array for store the info of cubes
    bool[,] blocks = new bool[Xmax,Ymax];
    GameObject[,] blockObjects = new GameObject[Xmax, Ymax]; 
	Block tetris;  //The moving one block
    GameObject cube;    //Prefab of cube
    Vector3 startPoint = new Vector3(Xmax/2,Ymax-1,0);  //start position

    //-------------------------------------------------------------
	// Use this for initialization
	void Start () {	
        cube = (GameObject)Resources.Load("Cube");
		
		//Initialize the type of Blocks
        database = new Block[typeofBlocks];

        database[0] = new Block();
        database[0].size = 2;
		database[0].blocks = new bool[2,2]{{true,true},{true,true}};
		database[0].length = 4;

        database[1] = new Block();
        database[1].size = 3;
		database[1].blocks = new bool[3,3]{{false,true,false},{true,true,true},{false,false,false}};
		database[1].length = 4;

        database[2] = new Block();
        database[2].size = 3;
        database[2].blocks = new bool[3, 3] { { false, false, false }, { true, true, false }, { false, true, true } };
        database[2].length = 4;

        database[3] = new Block();
        database[3].size = 3;
        database[3].blocks = new bool[3, 3] { { false, false, false }, { false, true, true }, { true, true, false } };
        database[3].length = 4;

        database[4] = new Block();
        database[4].size = 3;
        database[4].blocks = new bool[3, 3] { { false, false, false }, { true, true, true }, { false, false, true } };
        database[4].length = 4;

        database[5] = new Block();
        database[5].size = 3;
        database[5].blocks = new bool[3, 3] { { false, false, false }, { true, true, true }, { true, false, false } };
        database[5].length = 4;

        database[6] = new Block();
        database[6].size = 4;
        database[6].blocks = new bool[4, 4] { { false, false, false, false}, { false, false, false,false }, { true, true, true,true }, { false, false, false, false}};
        database[6].length = 4;

        init();
		SpawnBlock();
        StartCoroutine(UpdateGame());
        StartCoroutine(ImproveInput());
	}

    //-------------------------------------------------------------
    //Show score here
    void OnGUI() {
        GUI.enabled = true;

        string text;
        if(gameFinished)
        {
            GUILayout.BeginArea(new Rect(width/2 - 75, height/2-25, 150, 50));
            text = "Game Finished";
            GUILayout.Box(text);
            text = "Your Score" + score;
            GUILayout.Box(text);
            if(GUILayout.Button("Start a New Game"))
            {
                gameFinished = false;
                init();
                score = 0;
                level = 0;
                RowsFinished = 0;
                speed = 0.5f;
                SpawnBlock();
            }
            GUILayout.EndArea();
            return;
        }
        
        GUILayout.BeginArea(new Rect(10, 10, 100, 200));
        text = "Score : " + score;
        GUILayout.TextArea(text);
        text = "Rows :" + RowsFinished;
        GUILayout.TextArea(text);
        text = "Level : " + level;
        GUILayout.TextArea(text);
        text = "Speed : " + speed;
        GUILayout.TextArea(text);
        GUILayout.EndArea();
    }
    
    //-------------------------------------------------------------
    void init() {
        for (int i = 0; i < Xmax; i++)
            for (int j = 0; j < Ymax; j++) {
                blocks[i, j] = false;
                if(blockObjects[i,j]!=null)
                    Object.Destroy(blockObjects[i,j]);
                blockObjects[i, j] = null;
            }
    }

    //-------------------------------------------------------------
    //Update the block
    IEnumerator UpdateGame()
    {
        while (true){
            if(!gameFinished)
            {
                float dropspeed;
                if (dropping)
                    dropspeed = 0.0333f;
                else
                    dropspeed = speed;
                //dropspeed for controling the game speed
                if (CheckCollide()) {
                    MarkCollide();
                    CheckRow();
                    SpawnBlock();
                }
                else
                    tetris.UpdateBlock();   //Drop it
                yield return new WaitForSeconds(dropspeed);
            }
            else
                yield return new WaitForSeconds(0.5f);
        }
    }

    //-------------------------------------------------------------
    //Here are all the controls
    void Update () {
        if(gameFinished)
            return;
        //Rotation
        if(Input.GetKeyDown("space") || Input.GetKeyDown("up") || Input.GetKeyDown("w"))
            Rotate();
        //Moving left
        if (Input.GetKeyDown("left") || Input.GetKeyDown("a"))
            MoveLeft();
        else
            if (Input.GetAxis("Horizontal")==-1)
                movingLeft = true;
            else
                movingLeft = false;
        //Moving right
        if (Input.GetKeyDown("right") || Input.GetKeyDown("d"))
            MoveRight();
        else
            if (Input.GetAxis("Horizontal")==1)
                movingRight = true;
            else
                movingRight = false;
        //Drop the block
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            dropping = true;
        else
            dropping = false;
    }

    //-------------------------------------------------------------
    //This is for improving the control of moving left or right
    IEnumerator ImproveInput() {
        while (true) {
            if(!gameFinished)
            {
                if (movingLeft || movingRight)
                    yield return new WaitForSeconds(0.1f);
                else
                    yield return new WaitForSeconds(0.5f);
                if (movingLeft)
                    MoveLeft();
                if (movingRight)
                    MoveRight();
            }
            else
                yield return new WaitForSeconds(0.5f);
        }
    }

    //-------------------------------------------------------------
    //Collision
    bool CheckCollide() {
		for(int i=0;i<tetris.length;i++) {
			int x = (int)tetris.blockObjects[i].transform.position.x;
			int y = (int)tetris.blockObjects[i].transform.position.y;
			if (y == 0)
				return true;
			else
				if(blocks[x, y - 1])
					return true;
		}
		return false;
    }

    //-------------------------------------------------------------
    //Apply the collision here
    void MarkCollide() {
        int minY = 99;
		for(int i=0;i<tetris.length;i++) {
			int x = (int)tetris.blockObjects[i].transform.position.x;
			int y = (int)tetris.blockObjects[i].transform.position.y;
			blocks[x, y] = true;
        	blockObjects[x, y] = tetris.blockObjects[i];

            if (y < minY)
            {
                minY = y;
            }
		}
        //Score calculate here
        int factor = dropping ? 2 : 1;
        score += ((minY + 1) * (level + 1) * factor);
    }

    //-------------------------------------------------------------
    //Chech if some row finished, from top to bottom
    void CheckRow()
    {
        int checkedRow = 0;
        for (int i = Ymax-1; i >=0; i--) {
            bool RowFinished = true;
            for (int j = 0; j < Xmax;j++ )
                if (blocks[j, i] == false)
                    RowFinished = false;
            if (RowFinished) {
                CollapsRow(i);
                checkedRow++;
            }//if
        }//for

        //Update game level
        if (checkedRow > 0)
        {
            RowsFinished += checkedRow;
        }
        if (RowsFinished > levelThreshold * (level + 1))
        {
            level++;
            speed -= speedIncrement;
        }
    }

    //-------------------------------------------------------------
    //when row finished
    void CollapsRow(int row)
    {
        for (int i = 0; i < Xmax;i++ )
            Object.Destroy(blockObjects[i, row]);
        for (int j = row; j < Ymax-1;j++ )
            for (int i = 0; i < Xmax;i++ ){
                blocks[i, j] = blocks[i, j + 1];
                blockObjects[i, j] = blockObjects[i, j + 1];
                if (blockObjects[i, j] != null) {
                    Vector3 pos = blockObjects[i, j].transform.position;
                    blockObjects[i, j].transform.position = new Vector3(pos.x, pos.y - 1, pos.z);
                }
            }//for i
    }

    //-------------------------------------------------------------
    bool MoveLeft()
    {
		bool move = true;
		for(int i=0;i<tetris.length;i++) {
			Vector3 pos = tetris.blockObjects[i].transform.position;
			if (tetris.blockObjects[i].transform.position.x <= 0 || blocks[((int)pos.x-1),(int)pos.y]==true)
	        {
				move = false;
				break;
			}
		}
		if(move)
		{
			for(int i=0;i<tetris.length;i++)
			{
				Vector3 pos = tetris.blockObjects[i].transform.position;
				tetris.blockObjects[i].transform.position = new Vector3(pos.x - 1, pos.y, pos.z);
			}
			tetris.pos = new Vector3(tetris.pos.x-1,tetris.pos.y,0);
		}
        return move;
    }
    
    //-------------------------------------------------------------
    bool MoveRight()
    {
		bool move = true;
		for(int i=0;i<tetris.length;i++)
		{
			Vector3 pos = tetris.blockObjects[i].transform.position;
			if (tetris.blockObjects[i].transform.position.x >= Xmax-1 || blocks[((int)pos.x+1),(int)pos.y]==true)
			{
				move = false;
				break;
			}
		}
		if(move)
		{
			for(int i=0;i<tetris.length;i++)
			{
				Vector3 pos = tetris.blockObjects[i].transform.position;
				tetris.blockObjects[i].transform.position = new Vector3(pos.x + 1, pos.y, pos.z);
			}
			tetris.pos = new Vector3(tetris.pos.x+1,tetris.pos.y,0);
		}
        return move;
    }
	
    //-------------------------------------------------------------
	void Rotate()
	{
        bool doRotate = true;
        int nLeft = 0;
        int nRight = 0;
        //We need to check if it can be rotated first
        for (int i=0;i<tetris.size;i++)
            for (int j = 0; j < tetris.size;j++ ) {
                while(tetris.pos.x<0)
                    if(!MoveRight()) {
                        doRotate = false;
                        break;
                    }
                    else
                        nRight++;
                while(tetris.pos.x+tetris.size>Xmax)
                    if(!MoveLeft()){
                        doRotate = false;
                        break;
                    }
                    else
                        nLeft++;
                if (!doRotate || tetris.pos.y-tetris.size<=0 ||blocks[(int)tetris.pos.x+i,(int)tetris.pos.y-j])
                    doRotate = false;
            }
        if (doRotate)
            tetris.Rotate();
        else {  //if it can't rotate, then move back
            while(nLeft>0) {
                MoveRight();
                nLeft--;
            }
            while(nRight>0) {
                MoveLeft();
                nRight--;
            }
        }
	}

    //-------------------------------------------------------------
    void SpawnBlock()
    {
        //Get a random block
        int index = (int)Random.Range(0,typeofBlocks);
        //Create next block
        tetris = new Block();
		tetris.Spawn(database[index], cube, startPoint);

        //Check the if game is finished
        if(CheckCollide())
        {
            gameFinished = true;
            foreach(GameObject obj in tetris.blockObjects)
            {
                Object.Destroy(obj);
            }
        }
    }
}