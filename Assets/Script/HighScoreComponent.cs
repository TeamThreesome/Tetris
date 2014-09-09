//------------------------------------------------------------------------------
// Jimmy Liu
// OWNSELF
// 2014.9.9
//------------------------------------------------------------------------------
using UnityEngine;
using System.Text;
using System.Collections;

//------------------------------------------------------------------------------
// This is class for a Block
public class HighScoreComponent : MonoBehaviour {

    //--------------------------------------------------------------------------
    //High Score
    const string secretKey = "ownselftetris";
    const string HighScoreUrl = "http://www.ownself.org/threesome/Tetris/highscore.php";
    public string mResult;

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

    public void StartPostScore(string name, int level, int score) {
        StartCoroutine(PostScore(name, level, score));
    }

    IEnumerator PostScore(string name, int level, int score)
    {
        if (name == "" || name == "Input your name")
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
            GetComponentInParent<GameManager>().RestartGame();
            // RestartGame();
        }
        else 
        {
            Debug.Log(www.error);
        }
    }

    public void StartGetScore()
    {
        StartCoroutine(GetScore());
    }

    IEnumerator GetScore()
    {
        mResult = "";

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
}