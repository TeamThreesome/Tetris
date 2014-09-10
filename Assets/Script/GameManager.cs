//------------------------------------------------------------------------------
// Jimmy Liu
// OWNSELF
// 2013.6.28
//------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

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
    void Update() {
        mTetrisGame.Update();
        //Draw Score board
        ShowHighScoreBoard();
    }

    //--------------------------------------------------------------------------
    void ShowHighScoreBoard() {

        string[] lines;
        lines = mHighScoreBoard.mResult.Split('\n');

        Text highScoreOrderLabel = GameObject.Find("HUD_HighScore_Order").GetComponent<Text>();
        Text highScoreNameLabel = GameObject.Find("HUD_HighScore_Name").GetComponent<Text>();
        Text highScoreScoreLabel = GameObject.Find("HUD_HighScore_Score").GetComponent<Text>();

        highScoreOrderLabel.text = "";
        highScoreNameLabel.text = "";
        highScoreScoreLabel.text = "";
        for (int i = 0; i < lines.Length - 1; ++i) {
            if (i != 0) { //line feed
                highScoreOrderLabel.text += "\n";
                highScoreNameLabel.text+= "\n";
                highScoreScoreLabel.text += "\n";
            }
            string[] scores = lines[i].Split(',');
            highScoreOrderLabel.text += (i+1);
            highScoreNameLabel.text += scores[0];
            highScoreScoreLabel.text += scores[1];
        }
    }

    //--------------------------------------------------------------------------
    // Post your score
    public void StartPostScore() {
        mHighScoreBoard.StartPostScore(mTetrisGame.mName, mTetrisGame.mLevel, mTetrisGame.mScore);
    }

    //--------------------------------------------------------------------------
    // Get all high scores
    public void StartGetScore() {
        mHighScoreBoard.StartGetScore();
    }

    //--------------------------------------------------------------------------
    public void RestartGame() {
        mTetrisGame.RestartGame();
        StartGetScore();
    }
}