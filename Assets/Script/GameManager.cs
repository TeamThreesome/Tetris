//------------------------------------------------------------------------------
// Jimmy Liu
// OWNSELF
// 2013.6.28
//------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

//------------------------------------------------------------------------------
//Class for general manager of this Tetris game
public class GameManager : MonoBehaviour {

    //--------------------------------------------------------------------------

    //Const
    const int MaxBlocksWidth  = 10; //max number of blocks horizontally
    const int MaxBlocksHeight = 22; //max number of blocks vertically
    const int BlockTypes      = 7;
    const int RowsToNextLevel = 30;
    const float mSpeedIncrement = 0.05f; //speed every time increased

    //Player status
    int mScore          = 0;
    int mFinishedRows   = 0;
    int mLevel          = 0;

    //Game status
    float mSpeed = 0.5f; //time interval of game update(seconds)    //TODO : Change to mTickingTime
    bool mIsGameFinished = false;
    bool mIsFastDropping = false;
    bool mAllowFastDropping = false;    //Shouldn't continue fast dropping from next block

    //Game content variables
    Block[]         mBlocksData;    //Store the basic info of blocks
    GameObject      mBlockPrefab;   //Prefab of cube
    bool[,]         mTetrisState    = new bool[MaxBlocksWidth,MaxBlocksHeight];
    GameObject[,]   mBlockObjects   = new GameObject[MaxBlocksWidth, MaxBlocksHeight]; 
    Vector3         mStartPosition  = new Vector3(MaxBlocksWidth/2,MaxBlocksHeight-1,0);
    Block           mActiveBlock;   //The moving one block

    float leftMoveGap;
    float leftMoveInterval;
    float rightMoveGap;
    float rightMoveInterval;
    float droppingGap;
    float droppingInterval;

    //--------------------------------------------------------------------------
	// Use this for initialization
	void Start () {	
        mBlockPrefab = (GameObject)Resources.Load("Cube");
		
		//Initialize the type of Blocks
        mBlocksData = new Block[BlockTypes];

        mBlocksData[0] = new Block();
        mBlocksData[0].mSize = 2;
		mBlocksData[0].mBlocks = new bool[2,2]{{true,true},{true,true}};
		mBlocksData[0].mLength = 4;

        mBlocksData[1] = new Block();
        mBlocksData[1].mSize = 3;
		mBlocksData[1].mBlocks = new bool[3,3]{{false,true,false},{true,true,true},{false,false,false}};
		mBlocksData[1].mLength = 4;

        mBlocksData[2] = new Block();
        mBlocksData[2].mSize = 3;
        mBlocksData[2].mBlocks = new bool[3, 3] { { false, false, false }, { true, true, false }, { false, true, true } };
        mBlocksData[2].mLength = 4;

        mBlocksData[3] = new Block();
        mBlocksData[3].mSize = 3;
        mBlocksData[3].mBlocks = new bool[3, 3] { { false, false, false }, { false, true, true }, { true, true, false } };
        mBlocksData[3].mLength = 4;

        mBlocksData[4] = new Block();
        mBlocksData[4].mSize = 3;
        mBlocksData[4].mBlocks = new bool[3, 3] { { false, false, false }, { true, true, true }, { false, false, true } };
        mBlocksData[4].mLength = 4;

        mBlocksData[5] = new Block();
        mBlocksData[5].mSize = 3;
        mBlocksData[5].mBlocks = new bool[3, 3] { { false, false, false }, { true, true, true }, { true, false, false } };
        mBlocksData[5].mLength = 4;

        mBlocksData[6] = new Block();
        mBlocksData[6].mSize = 4;
        mBlocksData[6].mBlocks = new bool[4, 4] { { false, false, false, false}, { false, false, false,false }, { true, true, true,true }, { false, false, false, false}};
        mBlocksData[6].mLength = 4;

        init();
		SpawnBlock();
	}

    //--------------------------------------------------------------------------
    //Show score here
    void OnGUI() {
        GUI.enabled = true;

        string text;
        if(mIsGameFinished) {
            GUILayout.BeginArea(new Rect(Screen.width/2 - 75, Screen.height/2-25, 150, 50));
            text = "Game Finished";
            GUILayout.Box(text);
            text = "Your Score" + mScore;
            GUILayout.Box(text);
            if(GUILayout.Button("Start a New Game")) {
                mIsGameFinished = false;
                init();
                mScore = 0;
                mLevel = 0;
                mFinishedRows = 0;
                mSpeed = 0.5f;
                SpawnBlock();
            }
            GUILayout.EndArea();
            return;
        }
        
        GUILayout.BeginArea(new Rect(10, 10, 100, 200));
        text = "Score : " + mScore;
        GUILayout.TextArea(text);
        text = "Rows :" + mFinishedRows;
        GUILayout.TextArea(text);
        text = "Level : " + mLevel;
        GUILayout.TextArea(text);
        text = "Speed : " + mSpeed;
        GUILayout.TextArea(text);
        GUILayout.EndArea();
    }
    
    //--------------------------------------------------------------------------
    void init() {
        for (int i = 0; i < MaxBlocksWidth; i++)
            for (int j = 0; j < MaxBlocksHeight; j++) {
                mTetrisState[i, j] = false;
                if(mBlockObjects[i,j]!=null)
                    Object.Destroy(mBlockObjects[i,j]);
                mBlockObjects[i, j] = null;
            }
    }

    //--------------------------------------------------------------------------
    //Update the block
    void UpdateGame() {
        if(!mIsGameFinished) {
            if (CheckCollide()) {
                MarkCollide();
                CheckRow();
                SpawnBlock();
                //Clear dropping
                droppingGap = 0;
                mIsFastDropping = false;
                mAllowFastDropping = false;
            }
            else
                mActiveBlock.UpdateBlock();   //Drop it
        }
    }

    //--------------------------------------------------------------------------
    //Here are all the controls
    void Update() {
        if(mIsGameFinished)
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
            mAllowFastDropping = true;
        if (Input.GetKey("down") || Input.GetKey("s")) {
            if (mAllowFastDropping) {
                droppingGap += Time.deltaTime;
                if (droppingGap > 0.1)
                    mIsFastDropping = true;
            }
        }
        if (Input.GetKeyUp("down") || Input.GetKeyUp("s")) {
            droppingGap = 0;
            mIsFastDropping = false;
            mAllowFastDropping = false;
        }

        droppingInterval += Time.deltaTime;
        float dropspeed = mIsFastDropping ? 0.0333f : mSpeed;
        if (droppingInterval > dropspeed) {
            UpdateGame();
            droppingInterval = 0;
        }
    }

    //--------------------------------------------------------------------------
    //Collision
    bool CheckCollide() {
		for(int i=0;i<mActiveBlock.mLength;i++) {
			int x = (int)mActiveBlock.mBlockObjects[i].transform.position.x;
			int y = (int)mActiveBlock.mBlockObjects[i].transform.position.y;
			if (y == 0)
				return true;
			else
				if(mTetrisState[x, y - 1])
					return true;
		}
		return false;
    }

    //--------------------------------------------------------------------------
    //Apply the collision here
    void MarkCollide() {
        int minY = 99;
		for(int i=0;i<mActiveBlock.mLength;i++) {
			int x = (int)mActiveBlock.mBlockObjects[i].transform.position.x;
			int y = (int)mActiveBlock.mBlockObjects[i].transform.position.y;
			mTetrisState[x, y] = true;
        	mBlockObjects[x, y] = mActiveBlock.mBlockObjects[i];

            if (y < minY)
                minY = y;
		}
        //Score calculate here
        int factor = mIsFastDropping ? 2 : 1;
        mScore += ((minY + 1) * (mLevel + 1) * factor);
    }

    //--------------------------------------------------------------------------
    //Chech if some row finished, from top to bottom
    void CheckRow() {
        int checkedRow = 0;
        for (int i = MaxBlocksHeight-1; i >=0; i--) {
            bool RowFinished = true;
            for (int j = 0; j < MaxBlocksWidth;j++ )
                if (mTetrisState[j, i] == false)
                    RowFinished = false;
            if (RowFinished) {
                CollapsRow(i);
                checkedRow++;
            }//if
        }//for

        //Update game level
        if (checkedRow > 0)
            mFinishedRows += checkedRow;
        if (mFinishedRows > RowsToNextLevel * (mLevel + 1)) {
            mLevel++;
            mSpeed -= mSpeedIncrement;
        }
    }

    //--------------------------------------------------------------------------
    //when row finished
    void CollapsRow(int row) {
        for (int i = 0; i < MaxBlocksWidth;i++ )
            Object.Destroy(mBlockObjects[i, row]);
        for (int j = row; j < MaxBlocksHeight-1;j++ )
            for (int i = 0; i < MaxBlocksWidth;i++ ){
                mTetrisState[i, j] = mTetrisState[i, j + 1];
                mBlockObjects[i, j] = mBlockObjects[i, j + 1];
                if (mBlockObjects[i, j] != null) {
                    Vector3 pos = mBlockObjects[i, j].transform.position;
                    mBlockObjects[i, j].transform.position = new Vector3(pos.x, pos.y - 1, pos.z);
                }
            }//for i
    }

    //--------------------------------------------------------------------------
    bool MoveLeft() {
		bool move = true;
		for(int i=0;i<mActiveBlock.mLength;i++) {
			Vector3 pos = mActiveBlock.mBlockObjects[i].transform.position;
			if (mActiveBlock.mBlockObjects[i].transform.position.x <= 0
                    || mTetrisState[((int)pos.x-1),(int)pos.y]==true) {
				move = false;
				break;
			}
		}
		if(move) {
			for(int i=0;i<mActiveBlock.mLength;i++) {
				Vector3 pos = mActiveBlock.mBlockObjects[i].transform.position;
				mActiveBlock.mBlockObjects[i].transform.position = new Vector3(pos.x - 1, pos.y, pos.z);
			}
			mActiveBlock.mPos = new Vector3(mActiveBlock.mPos.x-1,mActiveBlock.mPos.y,0);
		}
        return move;
    }
    
    //--------------------------------------------------------------------------
    bool MoveRight() {
		bool move = true;
		for(int i=0;i<mActiveBlock.mLength;i++) {
			Vector3 pos = mActiveBlock.mBlockObjects[i].transform.position;
			if (mActiveBlock.mBlockObjects[i].transform.position.x >= MaxBlocksWidth-1
                    || mTetrisState[((int)pos.x+1),(int)pos.y]==true) {
				move = false;
				break;
			}
		}
		if(move) {
			for(int i=0;i<mActiveBlock.mLength;i++) {
				Vector3 pos = mActiveBlock.mBlockObjects[i].transform.position;
				mActiveBlock.mBlockObjects[i].transform.position = new Vector3(pos.x + 1, pos.y, pos.z);
			}
			mActiveBlock.mPos = new Vector3(mActiveBlock.mPos.x+1,mActiveBlock.mPos.y,0);
		}
        return move;
    }
	
    //--------------------------------------------------------------------------
	void Rotate() {
        bool doRotate = true;
        int nLeft = 0;
        int nRight = 0;
        //We need to check if it can be rotated first
        for (int i=0;i<mActiveBlock.mSize;i++)
            for (int j = 0; j < mActiveBlock.mSize;j++ ) {
                while(mActiveBlock.mPos.x<0)
                    if(!MoveRight()) {
                        doRotate = false;
                        break;
                    }
                    else
                        nRight++;
                while(mActiveBlock.mPos.x+mActiveBlock.mSize>MaxBlocksWidth)
                    if(!MoveLeft()){
                        doRotate = false;
                        break;
                    }
                    else
                        nLeft++;
                if (!doRotate || mActiveBlock.mPos.y-mActiveBlock.mSize<=0 ||mTetrisState[(int)mActiveBlock.mPos.x+i,(int)mActiveBlock.mPos.y-j])
                    doRotate = false;
            }
        if (doRotate)
            mActiveBlock.Rotate();
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

    //--------------------------------------------------------------------------
    void SpawnBlock() {
        //Get a random block
        int index = (int)Random.Range(0,BlockTypes);
        //Create next block
        mActiveBlock = new Block();
		mActiveBlock.Spawn(mBlocksData[index], mBlockPrefab, mStartPosition);

        //Check the if game is finished
        if(CheckCollide()) {
            mIsGameFinished = true;
            foreach(GameObject obj in mActiveBlock.mBlockObjects) {
                Object.Destroy(obj);
            }
        }
    }
    //--------------------------------------------------------------------------
}