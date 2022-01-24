using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.Serialization; 

[System.Serializable]
public class Agent
{
    public string agent_id;
    public string name;
    public string role;
    public string type;
    public double position_x;
    public double position_y;
    public double? position_z;
	public Dictionary<string, object> properties;

	public Agent(string id, string name,string type)
	{	
		this.agent_id = id;
		this.name = name;
		this.type = type;
		this.properties = new Dictionary<string, object>();
	}
	
	public static string GenerateAgentID()
	{
		System.DateTime currentTime = System.DateTime.Now;
		return "agent-"+currentTime.ToString("ddHHmmss");
	}

	public void SetPosition(double x, double y, double? z = null)
	{
		this.position_x = x;
		this.position_y = y;
		this.position_z = z;
	}

	public void AddProperty(string key,object value)
	{
		// conversão de float para double porque o LitJSON não
		// consegue reconhecer floats
		if(value is float){
			this.properties.Add(key, (double) new decimal((float)value));
		}
		else{
			this.properties.Add(key,value);
		}
	}

	public void SetRole(string role)
	{
		this.role = role;
	}
}
