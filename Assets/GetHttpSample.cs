using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using MiniJSON;
using System;
using Newtonsoft.Json;
using UnityEngine.UI;



public class GetHttpSample : MonoBehaviour {


    //ここから使う

    private string apikey = "xxxx"; 

    private string searchBaseURI = "https://www.googleapis.com/youtube/v3/search?key=xxxx&part=snippet&channelId=";

    private string searchBaseChannnel = "xxxx";//ここを書き換えるYoutubeチャンネル

    private string searchBaseStr = "&eventType=live&type=video";

    private string videoId;

    private string youtubeAPIbase = "https://www.googleapis.com/youtube/v3/";

    private string channnelSearch = "videos?part=liveStreamingDetails&id=";

    private string chatId;

    private string pagetoken = "&pageToken=";

    private string chatURIUp = "liveChat/messages?liveChatId=";

    private bool connectionflag = false;

    private string nextPageTokenstr = null;

    private string jsontext;


    [SerializeField]
    private GameObject canvas;


    //コメントと投稿時間だけ出るやつ
    private string chatURIbottom = "&part=snippet&hl=ja&maxResults=2000&fields=items/snippet/displayMessage,items/snippet/publishedAt,items/authorDetails/displayName&key=";//&part=snippet&hl=ja&maxResults=2000&fields=items/snippet/displayMessage,items/snippet/publishedAt&key=
    //全部出るやつ
    private string chatURIbottom2 = "&part=snippet,authorDetails&key=";

    // Use this for initialization
    void Start () {
        StartCoroutine(GetYoutubeAPI());
	}
	
	// Update is called once per frame
	void Update () {
        //StartCoroutine(GetYoutubeAPI());
    }


    private IEnumerator GetYoutubeAPI()
    {
        /*
        UnityWebRequest request = UnityWebRequest.Get(uri);

        yield return request.SendWebRequest();

        if(request.isHttpError || request.isNetworkError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
        }
        */
        var urisample = searchBaseURI + searchBaseChannnel + searchBaseStr;

        UnityWebRequest liverequest = UnityWebRequest.Get(urisample);//testURI + apikey);
        yield return liverequest.SendWebRequest();

        
        

        if (liverequest.isHttpError || liverequest.isNetworkError)
        {

            Debug.Log(liverequest.error);

        }
        else
        {
            jsontext = liverequest.downloadHandler.text;
            //MiniJSON　つかうー！！！！
            var mjson = (IDictionary)MiniJSON.Json.Deserialize(jsontext);
            Debug.Log("miniJson 1 : " + jsontext);

            var mitems = (IList)mjson["items"];
            var mid = (IDictionary)mitems[0];
            var sid = (IDictionary)mid["id"];
            string mvideoId = (string)sid["videoId"];
            Debug.Log(mvideoId);
            //videoIdを取得
            videoId = (string)sid["videoId"];


            /*vs2017ならできる？？
            var chatJsonObj = JsonConvert.DeserializeObject<dynamic>(jsontext);
            string videoId2 = chatJsonObj.items[0].id.videoId;
            Debug.Log("videoID2017 : " + videoId2);
            */

           
            StartCoroutine(GetChatId());

        }

    }

    private IEnumerator GetChatId()
    {
        StopCoroutine(GetYoutubeAPI());


        //ChatIdを取得しにいくよ！！
        var searchChannel = youtubeAPIbase + channnelSearch + videoId + "&key=" + apikey;

        UnityWebRequest channelrequest = UnityWebRequest.Get(searchChannel);
        yield return channelrequest.SendWebRequest();

        var mchanjson = (IDictionary)Json.Deserialize(channelrequest.downloadHandler.text);
        Debug.Log("miniJson 2 : " + channelrequest.downloadHandler.text);

        var citems = (IList)mchanjson["items"];
        var cslsd = (IDictionary)citems[0];
        var clad = (IDictionary)cslsd["liveStreamingDetails"];
        string mvchatId = (string)clad["activeLiveChatId"];
        //chatIdを取得
        chatId = (string)clad["activeLiveChatId"];

        StartCoroutine(GetComment());
    }

    private IEnumerator GetComment()
    {
        StopCoroutine(GetChatId());

        yield return new WaitForSeconds(5.0f);

        //チャットを取りに行く！！！
        var chatURI = youtubeAPIbase + chatURIUp + chatId + pagetoken + nextPageTokenstr + chatURIbottom2 + apikey;
        Debug.Log(chatURI);

        UnityWebRequest connectChatrequest = UnityWebRequest.Get(chatURI);
        yield return connectChatrequest.SendWebRequest();

        var commentlogjson = (IDictionary)Json.Deserialize(connectChatrequest.downloadHandler.text);
        Debug.Log(connectChatrequest.downloadHandler.text + " : commentjson");


        if(nextPageTokenstr == (string)commentlogjson["nextPageToken"])
        {
            Debug.Log("sameToken");
        }
        else
        {
            nextPageTokenstr = (string)commentlogjson["nextPageToken"];
            Debug.Log(nextPageTokenstr);

            

            var pageinfo = (IDictionary)commentlogjson["pageInfo"];
            //var totalResults = (IDictionary)pageinfo[0];
            int commentcount = int.Parse(pageinfo["totalResults"].ToString());
            Debug.Log(commentcount + " : countnum");
           
            //コメント分だけ描画
            for(var i = 0; i < (int)commentcount; i++)
            {
                GameObject cvn = Instantiate(canvas);

                var citems = (IList)commentlogjson["items"];
                var cslsd = (IDictionary)citems[i];
                var clad = (IDictionary)cslsd["snippet"];
                string message = (string)clad["displayMessage"];

                cvn.transform.Find("Description").gameObject.GetComponent<Text>().text = message;

                var author = (IDictionary)cslsd["authorDetails"];
                //var cslsd = (IDictionary)author[i];
                var dispName = (string)author["displayName"];

                cvn.transform.Find("Name").gameObject.GetComponent<Text>().text = dispName;

                float _x = UnityEngine.Random.Range(-400f, 400f);
                float _y = UnityEngine.Random.Range(-250f, 250f);
                cvn.transform.position = new Vector3(_x, _y, cvn.transform.position.z);
            }

            

        }
        

        StartCoroutine(stopWait());
    }

    IEnumerator stopWait()
    {
        yield return new WaitForSeconds(1f);

        StartCoroutine(GetComment());
        //StopCoroutine(retC);
       // Debug.Log("stop Coroutine");
    }

}

