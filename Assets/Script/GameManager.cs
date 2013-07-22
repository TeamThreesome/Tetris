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
    static public int Xmax = 12;   //Screen size of game
    static public int Ymax = 24;
    public float speed = 3; //init speed
    //These are the different rewards when player finish 1 line or 2 or 3 or 4
    public int reward1 = 10;
    public int reward2 = 30;
    public int reward3 = 100;
    public int reward4 = 500;
    //Increase the speed when how many blocks dropped
    public int levelThreshold = 15;
    public float speedIncrement = 2.0f; //Speed every time increased

    static int score = 0;   //Score
    static int level = 0;   //Progress of game
    static int typeofBlocks = 7;    //Types of blocks  ######
    Block[] database;   //Store the basic info of blocks

    //Some help static variable
    static bool dropping = false;
    static bool movingLeft = false;
    static bool movingRight = false;
    //Screen array for store the info of cubes
    bool[,] blocks = new bool[Xmax,Ymax];
    GameObject[,] blockObjects = new GameObject[Xmax, Ymax]; 
	Block tetris;  //The moving one block
    GameObject cube;    //Prefab of cube
    Vector3 startPoint = new Vector3(Xmax/2,Ymax-1,0);  //start position

	// Use this for initialization
	void Start () {	
        cube = (GameObject)Resources.Load("Cube");
		
		//Initialize the type of Blocks
        database = new Block[typeofBlocks];

        database[0] = new Block();
        database[0].size = 2;
		database[0].blocks = new bool[2,2]{{true,true},{true,true}};
		database[0].length = 4;
		database[0].cube = cube;

        database[1] = new Block();
        database[1].size = 3;
		database[1].blocks = new bool[3,3]{{false,true,false},{true,true,true},{false,false,false}};
		database[1].length = 4;
		database[1].cube = cube;

        database[2] = new Block();
        database[2].size = 3;
        database[2].blocks = new bool[3, 3] { { false, false, false }, { true, true, false }, { false, true, true } };
        database[2].length = 4;
        database[2].cube = cube;

        database[3] = new Block();
        database[3].size = 3;
        database[3].blocks = new bool[3, 3] { { false, false, false }, { false, true, true }, { true, true, false } };
        database[3].length = 4;
        database[3].cube = cube;

        database[4] = new Block();
        database[4].size = 3;
        database[4].blocks = new bool[3, 3] { { false, false, false }, { true, true, true }, { false, false, true } };
        database[4].length = 4;
        database[4].cube = cube;

        database[5] = new Block();
        database[5].size = 3;
        database[5].blocks = new bool[3, 3] { { false, false, false }, { true, true, true }, { true, false, false } };
        database[5].length = 4;
        database[5].cube = cube;

        database[6] = new Block();
        database[6].size = 4;
        database[6].blocks = new bool[4, 4] { { false, false, false, false}, { false, false, false,false }, { true, true, true,true }, { false, false, false, false}};
        database[6].length = 4;
        database[6].cube = cube;

        init();
		SpawnBlock();
        StartCoroutine(UpdateGame());
        StartCoroutine(ImproveInput());
	}

    //Show score here
    void OnGUI() {
        GUI.enabled = true;
        GUILayout.BeginArea(new Rect(10, 10, 50,50));
        string text = "" + score;
        GUILayout.TextArea(text);
        GUILayout.EndArea();
    }
    
    void init() {
        for (int i = 0; i < Xmax; i++)
            for (int j = 0; j < Ymax; j++) {
                blocks[i, j] = false;
                blockObjects[i, j] = null;
            }
    }
    //Update the block
    IEnumerator UpdateGame()
    {
        while (true){
            float dropspeed;
            if (dropping)
                dropspeed = 0.0333f;
            else
                dropspeed = 1.0f / speed;
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
    }

    //Here are all the controls
    void Update () {
        //Rotation
        if(Input.GetKeyDown ("space") )
            Rotate();
        //Moving left
        if (Input.GetKeyDown("left"))
            MoveLeft();
        else
            if (Input.GetAxis("Horizontal")==-1)
                movingLeft = true;
            else
                movingLeft = false;
        //Moving right
        if (Input.GetKeyDown("right"))
            MoveRight();
        else
            if (Input.GetAxis("Horizontal")==1)
                movingRight = true;
            else
                movingRight = false;
        //Drop the block
        if (Input.GetKey(KeyCode.DownArrow))
            dropping = true;
        else
            dropping = false;
    }

    //This is for improving the control of moving left or right
    IEnumerator ImproveInput() {
        while (true) {
            if (movingLeft || movingRight)
                yield return new WaitForSeconds(0.1f);
            else
                yield return new WaitForSeconds(0.5f);
            if (movingLeft)
                MoveLeft();
            if (movingRight)
                MoveRight();
        }
    }

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
    //Apply the collision here
    void MarkCollide() {
		for(int i=0;i<tetris.length;i++) {
			int x = (int)tetris.blockObjects[i].transform.position.x;
			int y = (int)tetris.blockObjects[i].transform.position.y;
			blocks[x, y] = true;
        	blockObjects[x, y] = tetris.blockObjects[i];
		}
    }
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
        //Score calculate here
        switch (checkedRow) {
            case 1:
                score += reward1;
                break;
            case 2:
                score += reward2;
                break;
            case 3:
                score += reward3;
                break;
            case 4:
                score += reward4;
                break;
        }//switch
    }

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

    void SpawnBlock()
    {
        //Get a random block
        int index = (int)Random.Range(0,typeofBlocks);
        tetris = new Block();
        tetris.size = database[index].size;
		tetris.blocks = database[index].blocks;
		tetris.length = database[index].length;
		tetris.cube = cube;
		tetris.pos = startPoint;

		tetris.SpawnBlock();
        //TODO : Here is some game code
        level++;
        if (level > levelThreshold)
        {
            speed += speedIncrement;
            level = 0;
        }
    }
}