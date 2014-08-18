//------------------------------------------------------------------------------
// Jimmy Liu
// OWNSELF
// 2013.6.28
//------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
using System.Text;
using System.Security;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

//------------------------------------------------------------------------------
//Class for general manager of this Tetris game
public class GameManager : MonoBehaviour {

    //--------------------------------------------------------------------------
    //High Score
    const string secretKey = "ownselftetris";
    const string HighScoreUrl = "http://www.ownself.org/threesome/Tetris/highscore.php";
    private string mResult = "";
    string mName = "Input your name";

    //Const
    const int MaxBlocksWidth  = 10; //max number of blocks horizontally
    const int MaxBlocksHeight = 22; //max number of blocks vertically
    const int BlockTypes      = 7;
    const int RowsToNextLevel = 10;
    const float mSpeedIncrement = 0.05f; //speed every time increased

    //Player status
    int mScore          = 0;
    int mFinishedRows   = 0;
    int mLevel          = 0;

    //Game status
    float mSpeed = 0.5f; //time interval of game update - in seconds    //TODO : Change to mTickingTime
    bool mIsGameFinished = false;
    bool mIsGamePaused = false;
    bool mIsFastDropping = false;
    bool mAllowFastDropping = false; //Shouldn't continue fast dropping from next block
    int mNextBlockType = 0;
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

    AudioSource mAudioPlayer;

    float leftMoveGap;
    float leftMoveInterval;
    float rightMoveGap;
    float rightMoveInterval;
    float droppingGap;
    float droppingInterval;

    public string Md5Sum(string input)
    {
        System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        byte[] hash = md5.ComputeHash(inputBytes);
 
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++)
        {
            sb.Append(hash[i].ToString("X2"));
        }
        return sb.ToString();
    }

    IEnumerator PostScore(string name, int level, int score)
    {
        if (name == "")
            yield break;

        string _name = name;
        int _level = level;
        int _score = score;
        
        string hash = Md5Sum(_name + _level + _score + secretKey).ToLower();
        
        WWWForm form = new WWWForm();
        form.AddField("do","posthighscore");
        form.AddField("playername",_name);
        form.AddField("level",_level);
        form.AddField("score",_score);
        form.AddField("hash",hash);
        
        WWW www = new WWW(HighScoreUrl, form);
        yield return www;
        
        if(www.text == "done") 
        {
            // StartCoroutine("GetScore");
            RestartGame();
        }
        else 
        {
            Debug.Log(www.error);
        }
    }

    IEnumerator GetScore()
    {
        mResult = "";
            
        // WindowTitel = "Loading";
        
        WWWForm form = new WWWForm();
        form.AddField("do","gethighscore");
        form.AddField("limit", 10);
        
        WWW www = new WWW(HighScoreUrl, form);
        yield return www;
        
        if(www.text == "") 
        {
            print("There was an error getting the high score: " + www.error);
            // WindowTitel = "There was an error getting the high score";
            Debug.Log("There was an error getting the high score");
        }
        else 
        {
            mResult = www.text;
        }
    }
    //--------------------------------------------------------------------------
    // Use this for initialization
    void Start() {  

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

        RestartGame();
    }

    //--------------------------------------------------------------------------
    void RestartGame() {
        //Reset all the state
        mIsGameFinished = false;
        mIsGamePaused = false;
        mScore = 0;
        mLevel = 0;
        mFinishedRows = 0;
        mSpeed = 0.5f;
        //Reinit the blocks and spawn the first one
        Init();
        GenerateNextBlockType(); // Get first block type
        SpawnBlock();
        RestartBackgroundMusic();
        StartCoroutine("GetScore");
    }

    //--------------------------------------------------------------------------
    void FinishGame() {
        PauseOrResumeBackgroundMusic();
        mIsGameFinished = true;
        foreach (GameObject obj in mActiveBlock.mBlockObjects)
            Object.Destroy(obj);
    }

    //--------------------------------------------------------------------------
    void Init() {

        for (int i = 0; i < MaxBlocksWidth; i++)
            for (int j = 0; j < MaxBlocksHeight; j++) {
                mTetrisState[i, j] = false;
                if (mGameBlockObjects[i,j] != null)
                    Object.Destroy(mGameBlockObjects[i,j]);
                mGameBlockObjects[i, j] = null;
            }
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
        mActiveBlock.Init(mBlocksData[mNextBlockType], mBlockPrefab, mStartPosition, mNextBlockColor);

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
    //Show score here
    void OnGUI() {

        GUI.enabled = true;

        string text;
        // Game finished
        if (mIsGameFinished) {
            GUILayout.BeginArea(new Rect(Screen.width/2 - 75, Screen.height/2 - 25, 150, 150));
            text = "Game Finished";
            GUILayout.Box(text);
            GUILayout.Space(20);

            text = "Your Score : " + mScore;
            GUILayout.Box(text);

            GUIStyle textFieldStyle = new GUIStyle("textfield");
            textFieldStyle.alignment  = TextAnchor.MiddleCenter;
            mName = GUILayout.TextField(mName, 15, textFieldStyle);

            if (GUILayout.Button("Post Score"))
            {
                StartCoroutine(PostScore(mName, mLevel, mScore));
            }
            if(GUILayout.Button("Start a New Game")) {
                RestartGame();
            }

            GUILayout.EndArea();
        }
        // Game is running
        else {
            if (mIsGamePaused) {
                GUILayout.BeginArea(new Rect(Screen.width/2 - 75, Screen.height/2 - 25, 150, 50));
                text = "Game Paused";
                GUILayout.Box(text);
                GUILayout.EndArea();
            }
            GUILayout.BeginArea(new Rect(10, 10, 100, 200));
            text = "Score : " + mScore;
            GUILayout.TextArea(text);
            text = "Rows :" + mFinishedRows;
            GUILayout.TextArea(text);
            text = "Level : " + mLevel;
            GUILayout.TextArea(text);
            // text = "Speed : " + mSpeed;
            // GUILayout.TextArea(text);
            // text = "Next Type : " + mNextBlockType;
            // GUILayout.TextArea(text);
            GUILayout.EndArea();
        }
        //Draw Score board
        ShowHighScoreBoard();
    }

    void ShowHighScoreBoard() {

        string[] lines;
        lines = mResult.Split('\n');

        GUILayout.BeginArea(new Rect(Screen.width * 0.65f, Screen.height / 4, Screen.width - 10, Screen.height - 5));
        GUIStyle leftyBoxStyle = new GUIStyle("box");
        leftyBoxStyle.alignment  = TextAnchor.MiddleLeft;
        GUIStyle rightyBoxStyle = new GUIStyle("box");
        rightyBoxStyle.alignment  = TextAnchor.MiddleRight;
        for (int i = 0; i < lines.Length - 1; ++i) {
            string[] scores = lines[i].Split(',');
            GUILayout.BeginHorizontal(GUILayout.Width(200));
            GUILayout.Box(""+(i+1), GUILayout.MaxWidth(20));
            // for (int j = 0; j < scores.Length; ++j) {
            GUILayout.Box(scores[0], leftyBoxStyle);
            GUILayout.Box(scores[1], rightyBoxStyle);
            // }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndArea();
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
    void Update() {
        if (Input.GetKeyDown("escape")) {
            mIsGamePaused = !mIsGamePaused;
            PauseOrResumeBackgroundMusic();
        }
        if (mIsGameFinished || mIsGamePaused)
            return;
        //Rotation
        if (Input.GetKeyDown("space") || Input.GetKeyDown("up") || Input.GetKeyDown("w"))
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
            }
        }

        // Update finished rows count
        if (finishedRows > 0)
            mFinishedRows += finishedRows;
        // Update level and speed
        if (mFinishedRows > RowsToNextLevel * (mLevel + 1)) {
            mLevel++;
            mSpeed -= mSpeedIncrement;
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
    void Rotate() {

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
            mActiveBlock.Rotate();
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
}