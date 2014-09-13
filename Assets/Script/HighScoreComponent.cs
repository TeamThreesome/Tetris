//------------------------------------------------------------------------------
// Jimmy Liu
// OWNSELF
// 2014.9.9
//------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections;

//------------------------------------------------------------------------------
// Script for high score board implementation
public class HighScoreComponent : MonoBehaviour {

    //--------------------------------------------------------------------------
    const string SecretKey = "ownselftetris";
    const string HighScoreUrl = "http://www.ownself.org/threesome/Tetris/highscore.php";

    //--------------------------------------------------------------------------
    string Md5Sum(string input) {
        System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
        byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
        byte[] hash = md5.ComputeHash(inputBytes);
 
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < hash.Length; i++) {
            sb.Append(hash[i].ToString("X2"));
        }
        return sb.ToString();
    }

    //--------------------------------------------------------------------------
    public void StartPostScore(string name, int level, int score) {
        StartCoroutine(PostScore(name, level, score));
    }

    public void StartGetScore() {
        StartCoroutine(GetScore());
    }

    //--------------------------------------------------------------------------
    IEnumerator PostScore(string name, int level, int score) {

        // In case of careless posting
        if (name == "" || name == "Input your name")
            yield break;

        string hash = Md5Sum(name + level + score + SecretKey).ToLower();
        
        WWWForm form = new WWWForm();
        form.AddField("do","posthighscore");
        form.AddField("playername", name);
        form.AddField("level", level);
        form.AddField("score", score);
        form.AddField("hash", hash);

        WWW www = new WWW(HighScoreUrl, form);
        yield return www;
        
        if(www.text == "done") {
            //TODO: Hard code
            GetComponentInParent<GameManager>().ReturnToMainMenu();
        }
        else {
            Debug.Log("There was an error posting the high score" + www.error);
        }
    }

    //--------------------------------------------------------------------------
    IEnumerator GetScore() {

        WWWForm form = new WWWForm();
        form.AddField("do","gethighscore");
        form.AddField("limit", 10);
        
        WWW www = new WWW(HighScoreUrl, form);
        yield return www;
        
        if(www.text == "") {
            // Network failed
            Debug.Log("There was an error getting the high score" + www.error);
        }
        else {
            // Draw Score board
            ShowHighScoreBoard(www.text);
        }
    }

    //--------------------------------------------------------------------------
    // Display with Unity UI
    void ShowHighScoreBoard(string result) {

        string[] lines;
        lines = result.Split('\n');

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
}