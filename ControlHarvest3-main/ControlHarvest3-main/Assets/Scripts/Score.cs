using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score : EntityFactory
{
    // generate ID to Micelio
    private string id_entity = Entity.GenerateEntityID();
    public int value;



    public Entity GetEntity()
    {
        Entity a = new Entity(id_entity, "Score");

        a.AddProperty("Value", value);
        //a.AddProperty("pontos de vida", hp);
        //a.AddProperty("patente", patente);
        return a;
    }
}
