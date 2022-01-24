using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Device
{
	public string device_id;
	public string model;
	public int screen_width;
	public int screen_height;
	public string system_name; 

	//construtor da classe Device
	public Device()
	{

		this.device_id = SystemInfo.deviceUniqueIdentifier;
        this.model = SystemInfo.deviceModel;
        this.screen_width = Screen.width;
        this.screen_height = Screen.height;
        this.system_name = SystemInfo.operatingSystem;
		
	}

	//verifica se algum dos dados não foi reconhecido
	public bool VerifyDataIntegrity()
	{
		if(this.screen_height <= 0 || this.screen_width <= 0){
			return false;
		}
		if(this.device_id == null || this.model == null || this.system_name == null){
			return false;
		}
		return true;
	}
}
