using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallScript : MonoBehaviour {

    private Vector2 wBegin;
    private Vector2 wFinish;


    void Start () {

        Vector3 wBegin = transform.GetChild(0).transform.position;
        Vector3 wFinish = transform.GetChild(1).transform.position;

        this.wBegin = new Vector2(wBegin.x, wBegin.z);
        this.wFinish = new Vector2(wFinish.x, wFinish.z);
    }
	
    public Utillites.WallPoints GetWallPoints() {

        Utillites.WallPoints points;
        points.wallStart = wBegin;
        points.wallEnd = wFinish;

        return points;
    }


}
