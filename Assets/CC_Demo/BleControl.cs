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

    //用于查找，是所有设备的dic
    Dictionary<string, Dictionary<string, string>> Scaning_devicesDic = new Dictionary<string, Dictionary<string, string>>();
    //玩家可选中的设备名称List
    List<string> Select_deviceList = new List<string>();
    //读取json的List
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
                            Debug.Log("Contains(devicesDic---：" + Scaning_devicesDic[res.id]["name"]);
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
        // 编辑器中：直接从 Assets 开始
        jsonPath = Path.Combine(Application.dataPath, "Data/DeviceNames.json");
#else
        // 打包后：exe 同目录的 Data 文件夹
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
            Debug.LogError("找不到 JSON 文件: " + jsonPath);
        }
    }
}


[System.Serializable]
public class DeviceNamesData
{
    public List<string> DeviceNames;
}
