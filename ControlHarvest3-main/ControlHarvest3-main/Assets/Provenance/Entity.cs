using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Entity
{
    public string entity_id;
    public string name;
    public string role;
    public double position_x;
    public double position_y;
    public double? position_z;
	public Dictionary<string, object> properties;

	public Entity(string id, string name)
	{
		this.entity_id = id;
		this.name = name;
		this.properties = new Dictionary<string, object>();
	}

	public static string GenerateEntityID()
	{
		System.DateTime currentTime = System.DateTime.Now;
		return "entity-"+currentTime.ToString("ddHHmmss");		
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
