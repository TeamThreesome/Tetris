//------------------------------------------------------------------------------
// Jimmy Liu
// OWNSELF
// 2014.9.10
//------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

//------------------------------------------------------------------------------
//Class for implementation of this Tetris game
public class TetrisGame {

    //--------------------------------------------------------------------------
    //Const
    const int MaxBlocksWidth        = 10; //max number of blocks horizontally
    const int MaxBlocksHeight       = 22; //max number of blocks vertically
    const int BlockTypes            = 7;  //how many types of blocks
    const int UnitRowsToNextLevel   = 3;
    const float SpeedIncrement      = 0.05f; //speed every time increased

    //Player status
    int mScore          = 0;
    int mFinishedRows   = 0;
    int mLevel          = 0;
    string mName = "Input your name";
    public int GetScore()           {return mScore;}
    public int GetFinishedRows()    {return mFinishedRows;}
    public int GetLevel()           {return mLevel;}
    public string GetPlayerName()   {return mName;}

    //Game status
    float mSpeed = 0.5f; //time interval of game update - in seconds    //TODO : Change to mTickingTime
    bool mIsGameFinished = false;
    bool mIsGamePaused = false;
    bool mIsFastDropping = false;
    bool mAllowFastDropping = false; //Shouldn't continue fast dropping from next block
    int mNextBlockType = 0;
    int RowsToNextLevel = UnitRowsToNextLevel;
    Color mNextBlockColor;

    //Game content variables
    Block[]         mBlocksData;    //Store the basic info of blocks
    GameObject      mBlockPrefab;   //Prefab of cube
    bool[,]         mTetrisState = new bool[MaxBlocksWidth, MaxBlocksHeight];
    GameObject[,]   mGameBlockObjects = new GameObject[MaxBlocksWidth, MaxBlocksHeight]; 
    Vector3         mStartPosition = new Vector3(MaxBlocksWidth/2, MaxBlocksHeight - 1, 0);
    Vector3         mPreviewPosition = new Vector3(MaxBlocksWidth + 5, MaxBlocksHeight - 1, 0);
    Block           mActiveBlock;   //The moving one block
    Block           mPreviewBlock;  //The block for preview

    //HUD Component
    Text mScoreTextLable;
    Text mRowsTextLable;
    Text mLevelTextLable;
    //Pause Panel
    GameObject mPausePanel;
    //Game End Panel
    GameObject mGameEndPanel;

    AudioSource mAudioPlayer;

    float leftMoveGap;
    float leftMoveInterval;
    float rightMoveGap;
    float rightMoveInterval;
    float droppingGap;
    float droppingInterval;

    //--------------------------------------------------------------------------
    // Use this for initialization
    public void Start() {

        mAudioPlayer = GameObject.Find("Main Camera").GetComponent<AudioSource>();

        mBlockPrefab = (GameObject)Resources.Load("Cube");

        mBlocksData = new Block[BlockTypes]; //Initialize the type of Blocks

        mBlocksData[0] = new Block();
        mBlocksData[0].mSize = 2;
        mBlocksData[0].mBlocks = new bool[2,2]{{true, true}, {true, true}};
        mBlocksData[0].mLength = 4;

        mBlocksData[1] = new Block();
        mBlocksData[1].mSize = 3;
        mBlocksData[1].mBlocks = new bool[3,3]{{false, true, false}, {true, true, true}, {false, false, false}};
        mBlocksData[1].mLength = 4;

        mBlocksData[2] = new Block();
        mBlocksData[2].mSize = 3;
        mBlocksData[2].mBlocks = new bool[3, 3] {{false, false, false}, {true, true, false}, {false, true, true}};
        mBlocksData[2].mLength = 4;

        mBlocksData[3] = new Block();
        mBlocksData[3].mSize = 3;
        mBlocksData[3].mBlocks = new bool[3, 3] {{false, false, false}, {false, true, true}, {true, true, false}};
        mBlocksData[3].mLength = 4;

        mBlocksData[4] = new Block();
        mBlocksData[4].mSize = 3;
        mBlocksData[4].mBlocks = new bool[3, 3] {{false, false, false}, {true, true, true}, {false, false, true}};
        mBlocksData[4].mLength = 4;

        mBlocksData[5] = new Block();
        mBlocksData[5].mSize = 3;
        mBlocksData[5].mBlocks = new bool[3, 3] {{false, false, false}, {true, true, true}, {true, false, false}};
        mBlocksData[5].mLength = 4;

        mBlocksData[6] = new Block();
        mBlocksData[6].mSize = 4;
        mBlocksData[6].mBlocks = new bool[4, 4] {{false, false, false, false}, {false, false, false,false}, {true, true, true,true}, {false, false, false, false}};
        mBlocksData[6].mLength = 4;

        mScoreTextLable = GameObject.Find("HUD_Score").GetComponent<Text>();
        mRowsTextLable = GameObject.Find("HUD_Rows").GetComponent<Text>();
        mLevelTextLable = GameObject.Find("HUD_Level").GetComponent<Text>();
        mPausePanel = GameObject.Find("PausePanel");
        mGameEndPanel = GameObject.Find("GameEndPanel");
    }

    //--------------------------------------------------------------------------
    void PauseGame(bool pause) {

        mIsGamePaused = pause;
        if (mPausePanel)
            mPausePanel.SetActive(pause);
    }

    //--------------------------------------------------------------------------
    public void RestartGame() {
        //Reset all the state
        mPausePanel.SetActive(false);
        mGameEndPanel.SetActive(false);
        PauseGame(false);
        //Reinit the blocks and spawn the first one
        Reset();
        GenerateNextBlockType(); // Get first block type
        SpawnBlock();
        RestartBackgroundMusic();
    }

    //--------------------------------------------------------------------------
    public void Reset() {

        mIsGameFinished = false;
        mScore = 0;
        mLevel = 0;
        mFinishedRows = 0;
        mSpeed = 0.5f;
        RowsToNextLevel = UnitRowsToNextLevel;

        for (int i = 0; i < MaxBlocksWidth; i++)
            for (int j = 0; j < MaxBlocksHeight; j++) {
                mTetrisState[i, j] = false;
                if (mGameBlockObjects[i,j] != null)
                    Object.Destroy(mGameBlockObjects[i,j]);
                mGameBlockObjects[i, j] = null;
            }

        if (mActiveBlock != null)
            mActiveBlock.Destroy();

        if (mPreviewBlock != null)
            mPreviewBlock.Destroy();
    }

    //--------------------------------------------------------------------------
    void FinishGame() {

        PauseOrResumeBackgroundMusic();
        mIsGameFinished = true;
        mGameEndPanel.SetActive(true);
        foreach (GameObject obj in mActiveBlock.mBlockObjects)
            Object.Destroy(obj);
    }

    //--------------------------------------------------------------------------
    void GenerateNextBlockType() {
        //Get a random block
        mNextBlockType = (int)Random.Range(0, BlockTypes);

        // Random color for block
        mNextBlockColor = new Color();
        mNextBlockColor.r = Random.Range(0.3f, 1f);
        mNextBlockColor.g = Random.Range(0.3f, 1f);
        mNextBlockColor.b = Random.Range(0.3f, 1f);
        mNextBlockColor.a = 1.0f; // Opacity
    }

    //--------------------------------------------------------------------------
    void SpawnBlock() {
        //Create next block
        mActiveBlock = new Block();
        int halfSize = (int)(mBlocksData[mNextBlockType].mSize / 2);
        Vector3 startPosition = new Vector3(mStartPosition.x - halfSize, mStartPosition.y, 0);
        mActiveBlock.Init(mBlocksData[mNextBlockType], mBlockPrefab, startPosition, mNextBlockColor);

        //Check the if game is finished
        if (CheckCollide()) {
            FinishGame();
        }
        else {
            GenerateNextBlockType();    // Get next block type
            if (mPreviewBlock != null)  // Destroy the previous preview
                mPreviewBlock.Destroy();
            mPreviewBlock = new Block();
            mPreviewBlock.Init(mBlocksData[mNextBlockType], mBlockPrefab, mPreviewPosition, mNextBlockColor);
        }
    }

    //--------------------------------------------------------------------------
    // Finish active block
    void CollideActiveBlock() {

        MarkCollide();              // Collide current one
        CheckRow();                 // Check if rows finished
        SpawnBlock();               // Spawn next one
        droppingGap = 0;            // Clear dropping
        
        mIsFastDropping = false;
        mAllowFastDropping = false;
    }
    
    //--------------------------------------------------------------------------
    // Drop the active block
    void UpdateActiveBlock() {

        if (!mIsGameFinished && !mIsGamePaused)
            if (CheckCollide())
                CollideActiveBlock();       //Collide
            else
                mActiveBlock.UpdateBlock(); //Drop it
    }

    //--------------------------------------------------------------------------
    void PauseOrResumeBackgroundMusic() {

        if (mAudioPlayer == null || !mAudioPlayer.enabled)
            return;
        if (mAudioPlayer.isPlaying) {
            mAudioPlayer.Pause();
        }
        else {
            mAudioPlayer.Play();
        }
    }

    //--------------------------------------------------------------------------
    void RestartBackgroundMusic() {

        if (mAudioPlayer == null || !mAudioPlayer.enabled)
            return;
        mAudioPlayer.Stop();
        mAudioPlayer.Play();
    }

    //--------------------------------------------------------------------------
    // Here are all the controls
    public void Update() {

        // Update UI
        UpdateHUD();
        if (Input.GetKeyDown("escape") && !mIsGameFinished) {
            PauseGame(!mIsGamePaused);
            PauseOrResumeBackgroundMusic();
        }
        if (mIsGameFinished || mIsGamePaused)
            return;
        //Rotation
        if (Input.GetKeyDown("space") || Input.GetKeyDown("w")) {
            Rotate(false);
        } else if (Input.GetKeyDown("up")) {
            Rotate(true);
        }

        //Moving left
        if (Input.GetKeyDown("left") || Input.GetKeyDown("a"))
            MoveLeft();
        //Holding left
        if (Input.GetKey("left") || Input.GetKey("a")) {
            leftMoveGap += Time.deltaTime;
            if (leftMoveGap > 0.2) {
                leftMoveInterval += Time.deltaTime;
                if (leftMoveInterval > 0.1 - 0.01 * mLevel) {
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
                if (rightMoveInterval > 0.1 - 0.01 * mLevel) {
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
            UpdateActiveBlock();
            droppingInterval = 0;
        }
    }

    //--------------------------------------------------------------------------
    // Check if the active block hit ground or other blocks
    bool CheckCollide() {

        for (int i = 0; i < mActiveBlock.mLength; i++) {
            // Get coordinate
            int x = (int)mActiveBlock.mBlockObjects[i].transform.position.x;
            int y = (int)mActiveBlock.mBlockObjects[i].transform.position.y;

            if (y == 0) 
                return true; // Hit bottom
            else
                if (mTetrisState[x, y - 1]) // Hit others
                    return true;
        }
        return false;
    }

    //--------------------------------------------------------------------------
    // Apply the collision here
    void MarkCollide() {

        int minY = MaxBlocksHeight;
        // Make the active block on the ground
        for (int i=0; i < mActiveBlock.mLength; i++) {
            int x = (int)mActiveBlock.mBlockObjects[i].transform.position.x;
            int y = (int)mActiveBlock.mBlockObjects[i].transform.position.y;
            // Pass the active block to global blocks
            mTetrisState[x, y] = true;
            mGameBlockObjects[x, y] = mActiveBlock.mBlockObjects[i];
            // Score depends on minY
            if (y < minY)
                minY = y;
        }
        //Score calculate here, double the score if fast dropping
        int factor = mIsFastDropping ? 2 : 1;
        mScore += ((minY + 1) * (mLevel + 1) * (mLevel + 1) * factor);
    }

    //--------------------------------------------------------------------------
    // Chech if there is any row finished, from top to bottom
    void CheckRow() {

        int finishedRows = 0;
        int maxY = 0;
        // Check, collaps and count the finished rows
        for (int i = MaxBlocksHeight - 1; i >= 0; i--) {
            bool finished = true;
            // Check if there is an empty block in this line
            for (int j = 0; j < MaxBlocksWidth; j++)
                if (mTetrisState[j, i] == false)
                    finished = false;
            // Collaps this line right away if it's finished
            if (finished) {
                CollapsRow(i);
                finishedRows++;
                if (i > maxY)
                    maxY = i;
            }
        }

        // Update finished rows count
        if (finishedRows > 0) {
            mFinishedRows += finishedRows;
            mScore += finishedRows * finishedRows * (mLevel + 1) * (mLevel + 1) * (maxY + 1);
        }
        // Update level and speed
        if (mFinishedRows > RowsToNextLevel) {
            mLevel++;
            RowsToNextLevel += UnitRowsToNextLevel * (mLevel + 1);
            mSpeed -= SpeedIncrement;
        }
    }

    //--------------------------------------------------------------------------
    // when a row finished
    void CollapsRow(int row) {

        // Destory the blocks gameObject in the row one by one
        for (int i = 0; i < MaxBlocksWidth;i++ )
            Object.Destroy(mGameBlockObjects[i, row]);

        // Move the above blocks down once orderly
        for (int j = row; j < MaxBlocksHeight - 1; j++)
            for (int i = 0; i < MaxBlocksWidth; i++) {
                // Update state
                mTetrisState[i, j] = mTetrisState[i, j + 1];
                // Update pointer
                mGameBlockObjects[i, j] = mGameBlockObjects[i, j + 1];
                // Update position if there is gameObject
                if (mGameBlockObjects[i, j] != null) {
                    Vector3 pos = mGameBlockObjects[i, j].transform.position;
                    mGameBlockObjects[i, j].transform.position = new Vector3(pos.x, pos.y - 1, pos.z);
                }
            }
    }

    //--------------------------------------------------------------------------
    // The action to try to move the active block left once
    bool MoveLeft() {

        bool canMove = true;
        // Check if it can move left
        for (int i = 0;i < mActiveBlock.mLength; i++) {
            Vector3 blockPos = mActiveBlock.mBlockObjects[i].transform.position;
            if (mActiveBlock.mBlockObjects[i].transform.position.x <= 0
                    || mTetrisState[((int)blockPos.x - 1), (int)blockPos.y]) {
                canMove = false;
                break;
            }
        }
        // Start to move
        if (canMove) {
            // Move blocks
            for (int i = 0; i < mActiveBlock.mLength; i++) {
                Vector3 pos = mActiveBlock.mBlockObjects[i].transform.position;
                mActiveBlock.mBlockObjects[i].transform.position = new Vector3(pos.x - 1, pos.y, pos.z);
            }
            // Update logic position
            mActiveBlock.mPos = new Vector3(mActiveBlock.mPos.x - 1, mActiveBlock.mPos.y, 0);
        }

        return canMove;
    }
    
    //--------------------------------------------------------------------------
    // The action to try to move the active block right once
    bool MoveRight() {

        bool canMove = true;
        // Check if it can move right
        for (int i = 0; i < mActiveBlock.mLength; i++) {
            Vector3 pos = mActiveBlock.mBlockObjects[i].transform.position;
            if (mActiveBlock.mBlockObjects[i].transform.position.x >= MaxBlocksWidth - 1
                    || mTetrisState[((int)pos.x + 1),(int)pos.y]) {
                canMove = false;
                break;
            }
        }
        // Start to move
        if (canMove) {
            // Move blocks
            for (int i = 0; i < mActiveBlock.mLength; i++) {
                Vector3 pos = mActiveBlock.mBlockObjects[i].transform.position;
                mActiveBlock.mBlockObjects[i].transform.position = new Vector3(pos.x + 1, pos.y, pos.z);
            }
            // Update logic position
            mActiveBlock.mPos = new Vector3(mActiveBlock.mPos.x + 1, mActiveBlock.mPos.y, 0);
        }
        return canMove;
    }
    
    //--------------------------------------------------------------------------
    // Rotate the Block
    void Rotate(bool IsClockwise) {

        bool canRotate = true;
        int leftMoved = 0;
        int rightMoved = 0;

        // We need to check if it can rotate firstly
        // If it sticks on the left side
        while (mActiveBlock.mPos.x < 0)
            if (!MoveRight()) {
                canRotate = false;
                break;
            }
            else
                rightMoved++; // Need move back to rotate

        // If it sticks on the right side
        while (mActiveBlock.mPos.x + mActiveBlock.mSize > MaxBlocksWidth)
            if (!MoveLeft()){
                canRotate = false;
                break;
            }
            else
                leftMoved++; // Need move back to rotate

        // Check if it will potentially hit ground or other blocks
        if (canRotate) {
            for (int i = 0; i < mActiveBlock.mSize; i++)
                for (int j = 0; j < mActiveBlock.mSize; j++) {
                    //TODO : This may have problem in some specific situation
                    if (mActiveBlock.mPos.y - mActiveBlock.mSize <= 0 ||
                            mTetrisState[(int)mActiveBlock.mPos.x + i,(int)mActiveBlock.mPos.y - j])
                        canRotate = false;
                }
        }

        // Start to rotate
        if (canRotate)
            mActiveBlock.Rotate(IsClockwise);
        else {
        //if it still can't rotate, then recover the scene
            while (leftMoved > 0) {
                MoveRight();
                leftMoved--;
            }
            while (rightMoved > 0) {
                MoveLeft();
                rightMoved--;
            }
        }
    }

    //--------------------------------------------------------------------------
    //Show score here
    void UpdateHUD() {

        // Game finished
        if (mIsGameFinished) {
            mGameEndPanel.SetActive(true);
            Text finishedScoreLabel = GameObject.Find("HUD_FinishedScore").GetComponent<Text>();
            finishedScoreLabel.text = "Game Finished";
            finishedScoreLabel.text += "\nYour Score : "+mScore;

            Text nameInputerLabel = GameObject.Find("HUD_NameInputLabel").GetComponent<Text>();
            mName = nameInputerLabel.text;
        }
        // Game is running
        else {
            mGameEndPanel.SetActive(false);
            mScoreTextLable.text = "" + mScore;
            mRowsTextLable.text = "" + mFinishedRows;
            mLevelTextLable.text = "" + mLevel;
        }
    }
}
