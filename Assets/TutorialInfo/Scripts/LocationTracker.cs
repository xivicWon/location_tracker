using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TMPro;

public class LocationTracker : MonoBehaviour
{
    LocationInfo currentGPSPosition;
    private static bool gpsStarted = false;
    double detailed_num =1.0;//기존 좌표는 float형으로 소수점 자리가 비교적 자세히 출력되는 double을 곱하여 자세한 값을 구합니다.

    private double prev_latitude;
    private double prev_longitude;
    private int gps_connect = 0;
    private int userDistance;
    private static WaitForSeconds second;

    private TMP_Text tmp_latitude;
    private TMP_Text tmp_longitude;
    private TMP_Text tmp_latitude_diff;
    private TMP_Text tmp_longitude_diff;
    private TMP_Text tmp_refresh;
    private TMP_Text tmp_distance;

    /*
        void Start(){

            Input.location.Start(0.5f);
            int wait = 1000; // 기본 값
            tmp_refresh.text = gps_connect.ToString();

            // Checks if the GPS is enabled by the user (-> Allow location ) 
            if ( false)
            {

                if (Input.location.isEnabledByUser)//사용자에 의하여 좌표값을 실행 할 수 있을 경우
                {
                    while (Input.location.status == LocationServiceStatus.Initializing && wait > 0)//초기화 진행중이면
                    {
                        wait--; // 기다리는 시간을 뺀다
                    }
                    //GPS를 잡는 대기시간
                    if (Input.location.status != LocationServiceStatus.Failed)//GPS가 실행중이라면
                    {
                        gpsInit = true;
                        // We start the timer to check each tick (every 3 sec) the current gps position

                        InvokeRepeating("RetrieveGPSData", 0.0001f, 1.0f);//0.0001초에 실행하고 1초마다 해당 함수를 실행합니다.
                    }
                }
                else//GPS가 없는 경우 (GPS가 없는 기기거나 안드로이드 GPS를 설정 하지 않았을 경우
                {
                    failGPSData();
                }
            } else
            {
                *//*StartCoroutine(myScanLocation, 1.0f);*//*
                StartCoroutine("ScanLocation", 1.0f);
            }
        }
    */


    private void Awake()
    {
        second = new WaitForSeconds(1f);
        this.tmp_latitude = GameObject.Find("tmp_latitude").GetComponent<TMP_Text>();
        this.tmp_longitude = GameObject.Find("tmp_longitude").GetComponent<TMP_Text>();
        this.tmp_latitude_diff = GameObject.Find("tmp_latitude_diff").GetComponent<TMP_Text>();
        this.tmp_longitude_diff = GameObject.Find("tmp_longitude_diff").GetComponent<TMP_Text>();
        this.tmp_refresh = GameObject.Find("tmp_refresh").GetComponent<TMP_Text>();
        this.tmp_distance = GameObject.Find("tmp_distance").GetComponent<TMP_Text>();
        prev_latitude = 0f;
        prev_longitude = 0f;

        userDistance = 0;
        this.tmp_latitude_diff.text = prev_latitude.ToString();
        this.tmp_longitude_diff.text = prev_longitude.ToString();
        this.tmp_distance.text = userDistance + "M";
    }



    IEnumerator Start()
    {
        // Starts the location service.
        Input.location.Start();

        // Check if the user has location service enabled.
        if (!Input.location.isEnabledByUser)
        {
            failGPSData();
            yield break;
        }

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return second;
            maxWait -= 1;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            print("Timed out");
            yield break;
        }

        //연결 실패
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("Unable to determine device location");
            failGPSData();
            yield break;

        }
        else
        {
            //접근 허가됨, 최초 위치 정보 받아오기
            gpsStarted = true;

            //현재 위치 갱신
            while (gpsStarted)
            {
                retrieveGPSData();
                yield return second;
            }
        }
    }

    public static void StopGPS()
    {
        if (Input.location.isEnabledByUser)
        {
            gpsStarted = false;
            Input.location.Stop();
        }
    }

    void failGPSData()
    {
        gps_connect++;
        tmp_latitude.text = "GPS not available";
        tmp_longitude.text = "GPS not available";
        tmp_refresh.text = gps_connect.ToString();
    }
    
    void retrieveGPSData()
    {
        gps_connect++;
        tmp_refresh.text = gps_connect.ToString();

        currentGPSPosition = Input.location.lastData;//gps를 데이터를 받습니다.

        double fixedLatitude = currentGPSPosition.latitude * detailed_num;
        double fixedLongitude = currentGPSPosition.longitude * detailed_num;
        tmp_latitude.text = fixedLatitude.ToString();
        tmp_longitude.text = fixedLongitude.ToString();


        if (prev_latitude == 0f)
        {
            prev_latitude = currentGPSPosition.latitude * detailed_num;
        }
        if (prev_longitude == 0f)
        {
            prev_longitude = currentGPSPosition.longitude * detailed_num;
        }

        double diffLatitude = ((fixedLatitude - prev_latitude) * 100000f);
        double diffLongitude = ((fixedLongitude - prev_longitude) * 100000f);


        if (diffLatitude != 0f)
        {
            tmp_latitude_diff.text = diffLatitude.ToString();
        }
        if (diffLongitude != 0f)
        {
            tmp_longitude_diff.text = diffLongitude.ToString();
        }

        double distanceMeter = distance(prev_latitude, prev_longitude, fixedLatitude, fixedLongitude, "meter");
        userDistance += (int)distanceMeter;
        tmp_distance.text = userDistance + "M";

        prev_latitude = currentGPSPosition.latitude * detailed_num;
        prev_longitude = currentGPSPosition.longitude * detailed_num;
    }
	private static double distance(double lat1, double lon1, double lat2, double lon2, String unit) {

        
		double theta = lon1 - lon2;
		double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));

		dist = Math.Acos(dist);
		dist = rad2deg(dist);
		dist = dist * 60 * 1.1515;

		if (unit == "kilometer") {
			dist = dist * 1.609344;
		} else if(unit == "meter"){
			dist = dist * 1609.344;
		}

		return (dist);
	}


	// This function converts decimal degrees to radians
	private static double deg2rad(double deg) {
		return (deg * Math.PI / 180.0);
	}

	// This function converts radians to decimal degrees
	private static double rad2deg(double rad) {
		return (rad * 180 / Math.PI);
	}

}