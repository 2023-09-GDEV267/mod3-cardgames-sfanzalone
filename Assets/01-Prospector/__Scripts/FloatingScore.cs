using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//An enum to track the possible states of a FloatingScore
public enum eFSState
{
    idle,
    pre,
    active,
    post
}

public class FloatingScore : MonoBehaviour
{
    [Header("Set Dynamically")]

    public eFSState state = eFSState.idle;

    [SerializeField]

    protected int _score = 0;
    public string scoreString;

    //The score property sets both _score and scoreString
    public int score
    {
        get
        {
            return (_score);
        }

        set
        {
            _score = value;
            scoreString = _score.ToString("N0"); //"N0" adds commas to the sum

            //Search "C# Standard Numeric Format Strings" for ToString formats
            GetComponent<Text>().text = scoreString;
        }
    }

    public List<Vector2> bezierPts; //Bezier points for movement
    public List<float> fontSizes; //Bezier points for font scaling
    public float timeStart = -1f;
    public float timeDuration = 1f;
    public string easingCurve = Easing.InOut; //Uses Easing in Util.cs

    //The GameObject that will receive the SendMessage when this is done moving
    public GameObject reportFinishTo = null;

    private RectTransform rectTrans;
    private Text txt;

    //Set up the FloatingScore and movement
    //Note the use of parameter defaults for eTimes & eTimeD
    public void Init(List<Vector2> ePts, float eTimes = 0, float etimesD = 1)
    {
        rectTrans = GetComponent<RectTransform>();
        rectTrans.anchoredPosition = Vector2.zero;

        txt = GetComponent<Text>();

        bezierPts = new List<Vector2>(ePts);

        if(ePts.Count == 1)
        {
            //If there's only one point
            //...then just go there.
            transform.position = ePts[0];

            return;
        }
    }
}