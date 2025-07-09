using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BleControl : MonoBehaviour
{
    private bool isScaning;
    private BleApi.ScanStatus scan_status;
    private Button StartScaningBtn;

    //���ڲ��ң��������豸��dic
    Dictionary<string, Dictionary<string, string>> Scaning_devicesDic = new Dictionary<string, Dictionary<string, string>>();
    //��ҿ�ѡ�е��豸����List
    List<string> Select_deviceList = new List<string>();
    //��ȡjson��List
    List<string> AllInJson_deviceList = new List<string>();
    private void Start()
    {
        StartScaningBtn = transform.Find("StartScaningBtn").GetComponent<Button>();
        StartScaningBtn.onClick.AddListener(OnStartScaning);


        LoadJson();
        Select_deviceList = AllInJson_deviceList;
    }

    void OnStartScaning()
    {
        BleApi.StartDeviceScan();
        isScaning = true;
    }

    void Update()
    {
        if (isScaning)
        {
            BleApi.DeviceUpdate res = new BleApi.DeviceUpdate();
            do
            {
                scan_status = BleApi.PollDevice(ref res, false);
                if (scan_status == BleApi.ScanStatus.AVAILABLE)
                {
                    Debug.Log("AVAILABLE");

                    if (!Scaning_devicesDic.ContainsKey(res.id))
                    {
                        Scaning_devicesDic[res.id] = new Dictionary<string, string>() { { "name", "" }, { "isConnectable", "False" } };
                    }
                    if (res.isConnectableUpdated)
                        Scaning_devicesDic[res.id]["name"] = res.name;
                    if (res.isConnectableUpdated)
                        Scaning_devicesDic[res.id]["isConnectable"] = res.isConnectable.ToString();

                    if (Scaning_devicesDic[res.id]["name"] != "" && Scaning_devicesDic[res.id]["isConnectable"] == "True")
                    {
                        if (AllInJson_deviceList.Contains(Scaning_devicesDic[res.id]["name"]))
                        {
                            Debug.Log("Contains(devicesDic---��" + Scaning_devicesDic[res.id]["name"]);
                        }
                    }

                }
                else if (scan_status == BleApi.ScanStatus.FINISHED)
                {
                    Debug.Log("FINISHED");

                    //if(AllInJson_deviceList.Count==)
                    BleApi.StartDeviceScan();
                    //BleApi.StopDeviceScan();
                    //isScaning = false;

                    //OnStartScaning();

                    foreach (string key in Scaning_devicesDic.Keys)
                    {
                        Debug.Log("key----------" + key + "value----" + Scaning_devicesDic[key]);
                    }
                    Debug.Log(Scaning_devicesDic.Count + "count------------");
                }
            } while (scan_status == BleApi.ScanStatus.AVAILABLE);
        }
    }


    void LoadJson()
    {
        string jsonPath;

#if UNITY_EDITOR
        // �༭���У�ֱ�Ӵ� Assets ��ʼ
        jsonPath = Path.Combine(Application.dataPath, "Data/DeviceNames.json");
#else
        // �����exe ͬĿ¼�� Data �ļ���
        jsonPath = Path.Combine(Application.dataPath, "..", "Data", "DeviceNames.json");
#endif

        if (File.Exists(jsonPath))
        {
            string jsonContent = File.ReadAllText(jsonPath);
            DeviceNamesData deviceNameData = JsonUtility.FromJson<DeviceNamesData>(jsonContent);

            foreach (string deviceName in deviceNameData.DeviceNames)
            {
                AllInJson_deviceList.Add(deviceName);
                Debug.Log("Device Name: " + deviceName);
            }
        }
        else
        {
            Debug.LogError("�Ҳ��� JSON �ļ�: " + jsonPath);
        }
    }
}


[System.Serializable]
public class DeviceNamesData
{
    public List<string> DeviceNames;
}
