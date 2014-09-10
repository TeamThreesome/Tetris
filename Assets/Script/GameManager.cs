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
    private HighScoreComponent mHighScoreBoard;
    private TetrisGame mTetrisGame;

    //--------------------------------------------------------------------------
    // Use this for initialization
    void Start() {
        mHighScoreBoard = GetComponentInParent<HighScoreComponent>();

        mTetrisGame = new TetrisGame();
        mTetrisGame.Start();

        RestartGame();
    }

    //--------------------------------------------------------------------------
    public void RestartGame() {
        mTetrisGame.RestartGame();
        StartGetScore();
    }

    //--------------------------------------------------------------------------
    void Update() {
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