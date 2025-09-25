using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using TMPro;


public class GameController : MonoBehaviour
{

    public Slider by1slider;
    public Slider by2slider;
    public Slider by1fishslider;
    public Slider by2fishslider;
    public TextMeshProUGUI team1Log;
    public TextMeshProUGUI team2Log;
    public TextMeshProUGUI roundText;
    string code; //Fra codegen.cs
    private Coroutine postRoutine;
    private int[] lastProcessedRoundForPlayer = new int[] {
    -1, -1, -1, -1, -1, -1, -1, -1
    };
    public Animation anim;
    public int round;
    public int killVote;
    public string[] actions = new string[8];
    bool timber = false;
    public bool isFisherDead = false;
    public Animator FisherAnimator;
    public string deathAnim;
    public int fishingBlocks = 0;

    public GameObject GameOverCanvas;
    public GameObject WorstEndingCanvas;
    public GameObject BadEndingCanvas;
    public GameObject GoodEndingCanvas;

    [System.Serializable]
    public class Actions
    {
        public string _1, _2, _3, _4, _5, _6, _7, _8;
    }

    [System.Serializable]
    public class ActionResponse { public int votes; public int round; public Actions actions; }

    [System.Serializable]
    public class CodePayload { public string code; }

    void Start()
    {
        postRoutine = StartCoroutine(InitAndSend());
    }

    private int localRound = 0;

    void Update()
    {
        if (round > localRound && isFisherDead == false)
        {
            localRound = round;
            team2Log.text = "Jeres fisker har fangst (+8 Fisk)" + "\n" + team2Log.text;
            by2fishslider.value = by2fishslider.value + 8;
        }
        if (isFisherDead == true)
        {
            FisherAnimator.Play(deathAnim);
        }
        if (timber == false && round == 5)
        {
            anim.Play("Treefall");
            timber = true;
            team1Log.text = "<color=green>Jeres døde træ væltede (+50 Træ)</color>\n" + team1Log.text;
            by1slider.value = by1slider.value + 50;
        }
        if (isFisherDead == false && killVote >= 4)
        {
            team2Log.text = "<color=red>Jeres fisker blev dræbt!</color>" + "\n" + team2Log.text;
            isFisherDead = true;
        }


        roundText.text = "Runde " + (round+1) + "/10";

        if (round == 10)
        {
            GameOverCanvas.SetActive(true);
            if (by1slider.value > 99 && by1fishslider.value > 99 && by2slider.value > 99 && by2fishslider.value > 99) {
                GoodEndingCanvas.SetActive(true);
            } else if (by1slider.value > 99 && by1fishslider.value > 99 || by2slider.value > 99 && by2fishslider.value > 99) {
                BadEndingCanvas.SetActive(true);
            } else {
                WorstEndingCanvas.SetActive(true);
            }
        }
 
    }

    void ActionHandler(int player, string action, int votes)
    {
        if (player >= 1 && player <= 4)
        {
            //Team 1
            if (action == "steal")
            {
                team1Log.text = "Spiller " + player + " Stjal Træ (+5 træ)" + "\n" + team1Log.text;
                team2Log.text = "<color=orange>Spiller " + player + " Stjal Træ (-5 træ)</color>" + "\n" + team2Log.text;
                Debug.Log("Stjæleri: " + player);
                by1slider.value = by1slider.value + 5;
                by2slider.value = by2slider.value - 5;
            }
            else if (action == "saml")
            {
                team1Log.text = "Spiller " + player + " Fældede frugttræer (+2 træ, +1 mad)" + "\n" + team1Log.text;
                by1slider.value = by1slider.value + 2;
                by2fishslider.value = by2fishslider.value + 1;
            }
            else if (action == "fiskh1")
            {
                if (fishingBlocks == 0)
                {
                    team1Log.text = "Spiller " + player + " Fiskede (+8 fisk)" + "\n" + team1Log.text;
                    by1fishslider.value = by1fishslider.value + 8;
                } else if (fishingBlocks > 0)
                {
                    team1Log.text = "<color=orange>Spiller " + player + " Kunne ikke fiske da åen var blokeret</color>" + "\n" + team1Log.text;
                    fishingBlocks = fishingBlocks - 1;
                }

            }

        } else
        {
            //Team 2
            if (action == "steal")
            {
                team2Log.text = "Spiller " + player + " Stjal Træ (+5 træ)" + "\n" + team2Log.text;
                team1Log.text = "<color=orange>Spiller " + player + " Stjal Træ (-5 træ)</color>" + "\n" + team1Log.text;
                Debug.Log("Stjæleri: " + player);
                by1slider.value = by1slider.value - 5;
                by2slider.value = by2slider.value + 5;
            }
            else if (action == "saml")
            {
                team2Log.text = "Spiller " + player + " Fældede træer (+2 træ)" + "\n" + team2Log.text;
                Debug.Log("Samleri: " + player);
                by2slider.value = by2slider.value + 2;
            }
            else if (action == "fisk")
            {
                team2Log.text = "Spiller " + player + " Fiskede (+8 fisk)" + "\n" + team2Log.text;
                Debug.Log("Fiskeri: " + player);
                by2fishslider.value = by2fishslider.value + 8;
            }
            else if (action == "fiskvogt")
            {
                team2Log.text = "Spiller " + player + " Fiskede og vogtede åen (+8 fisk)" + "\n" + team2Log.text;
                Debug.Log("Fiskeri: " + player);
                by2fishslider.value = by2fishslider.value + 8;
                fishingBlocks = fishingBlocks + 1;
            }
        }
    }


    IEnumerator InitAndSend()
    {
        // Vent på kodegen, nok ikke nødvendigt
        while (string.IsNullOrEmpty(CodeGen.code))
            yield return null;

        code = CodeGen.code;
        yield return StartCoroutine(SendPostEverySecond());
    }

    IEnumerator SendPostEverySecond()
    {
        string url = "http://localhost:8080/actions";

        while (true)
        {

            string jsonData = JsonUtility.ToJson(new CodePayload { code = code });

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);


            UnityWebRequest request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");


            yield return request.SendWebRequest();

            string responseText = request.downloadHandler != null ? request.downloadHandler.text : null;


            if (!string.IsNullOrEmpty(responseText))
            {
                string normalized = Regex.Replace(responseText, @"""(\d+)""\s*:", @"""_$1"":");

                ActionResponse parsed = JsonUtility.FromJson<ActionResponse>(normalized);

                if (parsed != null)
                {
                    round = parsed.round;
                    killVote = parsed.votes;

                    if (parsed.actions != null)
                    {
                        string[] tmp = {
                            parsed.actions._1,
                            parsed.actions._2,
                            parsed.actions._3,
                            parsed.actions._4,
                            parsed.actions._5,
                            parsed.actions._6,
                            parsed.actions._7,
                            parsed.actions._8
                        };


                        //Handle actions
                        for (int i = 0; i < tmp.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(tmp[i]) && lastProcessedRoundForPlayer[i] < parsed.round)
                            {
                                ActionHandler(i + 1, tmp[i], parsed.votes);
                                lastProcessedRoundForPlayer[i] = parsed.round;
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(1f);
        }
    }

    void OnDestroy()
    {
        if (postRoutine != null) StopCoroutine(postRoutine);
    }
}
