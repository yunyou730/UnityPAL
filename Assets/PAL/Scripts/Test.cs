using System.IO;
using UnityEngine;

public class Test : MonoBehaviour
{
    MKFLoader _mkfLoader = null;
    
    void Start()
    {
        //int mapIndex = 12;
        //string fullPath = Path.Combine(Application.streamingAssetsPath, "MAP.MKF");
        //_mkfLoader = new MKFLoader(fullPath);
        //_mkfLoader.Load();


        var mapMKF = new MKFLoader(Path.Combine(Application.streamingAssetsPath, "MAP.MKF"));
        var gopMKF = new MKFLoader(Path.Combine(Application.streamingAssetsPath, "GOP.MKF"));
        mapMKF.Load();
        gopMKF.Load();
        var map = new ayy.pal.Map();
        map.LoadMap(12,mapMKF,gopMKF);



    }
    
    void Update()
    {
        
    }
}
