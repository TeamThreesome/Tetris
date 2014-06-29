//--------------------------------------------
// Jimmy Liu
// OWNSELF
// 2013.6.28
//--------------------------------------------
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

//Class for general manager of this Tetris game
public class GameManager : MonoBehaviour {

    //static variables for game play
    static public int Xmax = 10;   //Screen size of game    should be const
    static public int Ymax = 22;
    //Increase the speed when how many blocks dropped
    public int levelThreshold = 30;
    public float speed = 0.5f; //init speed
    public float speedIncrement = 0.05f; //Speed every time increased

    static int score = 0;   //Score
    static int RowsFinished = 0;   //Rows finished of game
    static int level = 0;   //Progress of game
    static int typeofBlocks = 7;    //Types of blocks  ###### Should change this one to const
    Block[] database;   //Store the basic info of blocks

    //Some help static variable
    static bool toggleDropping = false;
    static bool dropping = false;
    // static bool movingLeft = false;
    // static bool movingRight = false;

    float leftMoveGap;
    float leftMoveInterval;
    float rightMoveGap;
    float rightMoveInterval;
    float droppingGap;
    float droppingInterval;

    bool gameFinished = false;

    int width = Screen.width;
    int height = Screen.height;

    //Screen array for store the info of cubes
    bool[,] blocks = new bool[Xmax,Ymax];
    GameObject[,] mBlockObjects = new GameObject[Xmax, Ymax]; 
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
        database[0].mSize = 2;
		database[0].mBlocks = new bool[2,2]{{true,true},{true,true}};
		database[0].mLength = 4;

        database[1] = new Block();
        database[1].mSize = 3;
		database[1].mBlocks = new bool[3,3]{{false,true,false},{true,true,true},{false,false,false}};
		database[1].mLength = 4;

        database[2] = new Block();
        database[2].mSize = 3;
        database[2].mBlocks = new bool[3, 3] { { false, false, false }, { true, true, false }, { false, true, true } };
        database[2].mLength = 4;

        database[3] = new Block();
        database[3].mSize = 3;
        database[3].mBlocks = new bool[3, 3] { { false, false, false }, { false, true, true }, { true, true, false } };
        database[3].mLength = 4;

        database[4] = new Block();
        database[4].mSize = 3;
        database[4].mBlocks = new bool[3, 3] { { false, false, false }, { true, true, true }, { false, false, true } };
        database[4].mLength = 4;

        database[5] = new Block();
        database[5].mSize = 3;
        database[5].mBlocks = new bool[3, 3] { { false, false, false }, { true, true, true }, { true, false, false } };
        database[5].mLength = 4;

        database[6] = new Block();
        database[6].mSize = 4;
        database[6].mBlocks = new bool[4, 4] { { false, false, false, false}, { false, false, false,false }, { true, true, true,true }, { false, false, false, false}};
        database[6].mLength = 4;

        init();
		SpawnBlock();
	}

    //-------------------------------------------------------------
    //Show score here
    void OnGUI() {
        GUI.enabled = true;

        string text;
        if(gameFinished) {
            GUILayout.BeginArea(new Rect(width/2 - 75, height/2-25, 150, 50));
            text = "Game Finished";
            GUILayout.Box(text);
            text = "Your Score" + score;
            GUILayout.Box(text);
            if(GUILayout.Button("Start a New Game")) {
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
                if(mBlockObjects[i,j]!=null)
                    Object.Destroy(mBlockObjects[i,j]);
                mBlockObjects[i, j] = null;
            }
    }

    //-------------------------------------------------------------
    //Update the block
    void UpdateGame() {
        if(!gameFinished) {
            if (CheckCollide()) {
                MarkCollide();
                CheckRow();
                SpawnBlock();
                //Clear dropping
                droppingGap = 0;
                dropping = false;
                toggleDropping = false;
            }
            else
                tetris.UpdateBlock();   //Drop it
        }
    }

    //-------------------------------------------------------------
    //Here are all the controls
    void Update() {
        if(gameFinished)
            return;
        //Rotation
        if(Input.GetKeyDown("space") || Input.GetKeyDown("up") || Input.GetKeyDown("w"))
            Rotate();
        //Moving left
        if (Input.GetKeyDown("left") || Input.GetKeyDown("a"))
            MoveLeft();
        //Holding left
        if (Input.GetKey("left") || Input.GetKey("a")) {
            leftMoveGap += Time.deltaTime;
            if (leftMoveGap > 0.2) {
                leftMoveInterval += Time.deltaTime;
                if (leftMoveInterval > 0.1) {
                    leftMoveInterval = 0;
                    MoveLeft();
                }
            }
        }
        if (Input.GetKeyUp("left") || Input.GetKeyUp("a"))
            leftMoveGap = 0;
        //Moving right
        if (Input.GetKeyDown("right") || Input.GetKeyDown("d"))
            MoveRight();
        if (Input.GetKey("right") || Input.GetKey("d")) {
            rightMoveGap += Time.deltaTime;
            if (rightMoveGap > 0.2) {
                rightMoveInterval += Time.deltaTime;
                if (rightMoveInterval > 0.1) {
                    rightMoveInterval = 0;
                    MoveRight();
                }
            }
        }
        if (Input.GetKeyUp("right") || Input.GetKeyUp("d"))
            rightMoveGap = 0;
        //Drop the block
        if (Input.GetKeyDown("down") || Input.GetKeyDown("s"))
            toggleDropping = true;
        if (Input.GetKey("down") || Input.GetKey("s")) {
            if (toggleDropping) {
                droppingGap += Time.deltaTime;
                if (droppingGap > 0.1)
                    dropping = true;
            }
        }
        if (Input.GetKeyUp("down") || Input.GetKeyUp("s")) {
            droppingGap = 0;
            dropping = false;
            toggleDropping = false;
        }

        droppingInterval += Time.deltaTime;
        float dropspeed = dropping ? 0.0333f : speed;
        if (droppingInterval > dropspeed) {
            UpdateGame();
            droppingInterval = 0;
        }
    }

    //-------------------------------------------------------------
    //Collision
    bool CheckCollide() {
		for(int i=0;i<tetris.mLength;i++) {
			int x = (int)tetris.mBlockObjects[i].transform.position.x;
			int y = (int)tetris.mBlockObjects[i].transform.position.y;
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
		for(int i=0;i<tetris.mLength;i++) {
			int x = (int)tetris.mBlockObjects[i].transform.position.x;
			int y = (int)tetris.mBlockObjects[i].transform.position.y;
			blocks[x, y] = true;
        	mBlockObjects[x, y] = tetris.mBlockObjects[i];

            if (y < minY)
                minY = y;
		}
        //Score calculate here
        int factor = dropping ? 2 : 1;
        score += ((minY + 1) * (level + 1) * factor);
    }

    //-------------------------------------------------------------
    //Chech if some row finished, from top to bottom
    void CheckRow() {
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
            RowsFinished += checkedRow;
        if (RowsFinished > levelThreshold * (level + 1)) {
            level++;
            speed -= speedIncrement;
        }
    }

    //-------------------------------------------------------------
    //when row finished
    void CollapsRow(int row) {
        for (int i = 0; i < Xmax;i++ )
            Object.Destroy(mBlockObjects[i, row]);
        for (int j = row; j < Ymax-1;j++ )
            for (int i = 0; i < Xmax;i++ ){
                blocks[i, j] = blocks[i, j + 1];
                mBlockObjects[i, j] = mBlockObjects[i, j + 1];
                if (mBlockObjects[i, j] != null) {
                    Vector3 pos = mBlockObjects[i, j].transform.position;
                    mBlockObjects[i, j].transform.position = new Vector3(pos.x, pos.y - 1, pos.z);
                }
            }//for i
    }

    //-------------------------------------------------------------
    bool MoveLeft() {
		bool move = true;
		for(int i=0;i<tetris.mLength;i++) {
			Vector3 pos = tetris.mBlockObjects[i].transform.position;
			if (tetris.mBlockObjects[i].transform.position.x <= 0
                    || blocks[((int)pos.x-1),(int)pos.y]==true) {
				move = false;
				break;
			}
		}
		if(move) {
			for(int i=0;i<tetris.mLength;i++) {
				Vector3 pos = tetris.mBlockObjects[i].transform.position;
				tetris.mBlockObjects[i].transform.position = new Vector3(pos.x - 1, pos.y, pos.z);
			}
			tetris.mPos = new Vector3(tetris.mPos.x-1,tetris.mPos.y,0);
		}
        return move;
    }
    
    //-------------------------------------------------------------
    bool MoveRight() {
		bool move = true;
		for(int i=0;i<tetris.mLength;i++) {
			Vector3 pos = tetris.mBlockObjects[i].transform.position;
			if (tetris.mBlockObjects[i].transform.position.x >= Xmax-1
                    || blocks[((int)pos.x+1),(int)pos.y]==true) {
				move = false;
				break;
			}
		}
		if(move) {
			for(int i=0;i<tetris.mLength;i++) {
				Vector3 pos = tetris.mBlockObjects[i].transform.position;
				tetris.mBlockObjects[i].transform.position = new Vector3(pos.x + 1, pos.y, pos.z);
			}
			tetris.mPos = new Vector3(tetris.mPos.x+1,tetris.mPos.y,0);
		}
        return move;
    }
	
    //-------------------------------------------------------------
	void Rotate() {
        bool doRotate = true;
        int nLeft = 0;
        int nRight = 0;
        //We need to check if it can be rotated first
        for (int i=0;i<tetris.mSize;i++)
            for (int j = 0; j < tetris.mSize;j++ ) {
                while(tetris.mPos.x<0)
                    if(!MoveRight()) {
                        doRotate = false;
                        break;
                    }
                    else
                        nRight++;
                while(tetris.mPos.x+tetris.mSize>Xmax)
                    if(!MoveLeft()){
                        doRotate = false;
                        break;
                    }
                    else
                        nLeft++;
                if (!doRotate || tetris.mPos.y-tetris.mSize<=0 ||blocks[(int)tetris.mPos.x+i,(int)tetris.mPos.y-j])
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
    void SpawnBlock() {
        //Get a random block
        int index = (int)Random.Range(0,typeofBlocks);
        //Create next block
        tetris = new Block();
		tetris.Spawn(database[index], cube, startPoint);

        //Check the if game is finished
        if(CheckCollide()) {
            gameFinished = true;
            foreach(GameObject obj in tetris.mBlockObjects) {
                Object.Destroy(obj);
            }
        }
    }
}