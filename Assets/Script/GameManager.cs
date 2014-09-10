//------------------------------------------------------------------------------
// Jimmy Liu
// OWNSELF
// 2013.6.28
//------------------------------------------------------------------------------
using UnityEngine;

//------------------------------------------------------------------------------
//Class for game flow manager
public class GameManager : MonoBehaviour {

    //--------------------------------------------------------------------------
    //High Score
    HighScoreComponent mHighScoreBoard;
    TetrisGame mTetrisGame;
    GameObject mMainMenu;
    GameObject mLeftBorder;
    GameObject mRightBorder;
    GameObject mBottomBorder;
    GameObject mGameStatusPanel;
    GameObject mPausePanel;
    GameObject mGameEndPanel;
    GameObject mHighScorePanel;
    bool mIsGameStarted = false;

    //--------------------------------------------------------------------------
    // Use this for initialization
    void Start() {
        mHighScoreBoard = GetComponentInParent<HighScoreComponent>();

        mMainMenu = GameObject.Find("MainMenu");
        mLeftBorder = GameObject.Find("LeftBorder");
        mRightBorder = GameObject.Find("RightBorder");
        mBottomBorder = GameObject.Find("BottomBorder");
        mGameStatusPanel = GameObject.Find("GameStatusPanel");
        mPausePanel = GameObject.Find("PausePanel");
        mGameEndPanel = GameObject.Find("GameEndPanel");
        mHighScorePanel = GameObject.Find("HighScorePanel");
        
        mTetrisGame = new TetrisGame();
        mTetrisGame.Start();
        ReturnToMainMenu();
    }

    public void StartTetrisGame() {

        mMainMenu.SetActive(false);

        mLeftBorder.SetActive(true);
        mRightBorder.SetActive(true);
        mBottomBorder.SetActive(true);
        mGameStatusPanel.SetActive(true);
        mPausePanel.SetActive(true);
        mGameEndPanel.SetActive(true);
        mHighScorePanel.SetActive(true);

        RestartGame();
        mIsGameStarted = true;
    }

    public void ReturnToMainMenu() {

        mTetrisGame.Reset();
        mMainMenu.SetActive(true);

        mLeftBorder.SetActive(false);
        mRightBorder.SetActive(false);
        mBottomBorder.SetActive(false);
        mGameStatusPanel.SetActive(false);
        mPausePanel.SetActive(false);
        mGameEndPanel.SetActive(false);
        mHighScorePanel.SetActive(false);

        mIsGameStarted = false;
    }

    //--------------------------------------------------------------------------
    public void RestartGame() {
        mTetrisGame.RestartGame();
        StartGetScore();
    }

    //--------------------------------------------------------------------------
    void Update() {
        if (mIsGameStarted)
            mTetrisGame.Update();
    }

    //--------------------------------------------------------------------------
    // Get all high scores
    public void StartGetScore() {
        mHighScoreBoard.StartGetScore();
    }

    //--------------------------------------------------------------------------
    // Post your score
    public void StartPostScore() {
        mHighScoreBoard.StartPostScore(mTetrisGame.GetPlayerName(), mTetrisGame.GetLevel(), mTetrisGame.GetScore());
    }
}