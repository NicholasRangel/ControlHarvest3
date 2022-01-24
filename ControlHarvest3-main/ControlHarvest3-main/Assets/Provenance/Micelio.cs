using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Reflection;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;


public class Micelio
{
 
    private string _defaultURL = "https://achernar.eic.cefet-rj.br/micelio/api";
    private string _token;
    private string _device_id;
    private bool _is_dev;
    private Assembly _dll;
    private string _configFilePath = Application.persistentDataPath + "/micelioDeviceSettings.bin";
    
    //construtor classe Micelio
    public Micelio(string token, string is_dev = "")
    {

        this._token = token;
        this._device_id = GetDeviceInformation();
        string path = Directory.GetCurrentDirectory();
        this._dll = Assembly.LoadFrom(path+"/Assets/Provenance/LitJson.dll");
        if(is_dev == "dev"){
            this._is_dev = true;
        }
        else{
            this._is_dev = false;   
        }
    }

    public static string ToJSON(object o)
    {
        LitJson.JsonWriter writer = new LitJson.JsonWriter(Console.Out);
        writer.PrettyPrint = true;
        writer.IndentValue = 2;
        
        string json = LitJson.JsonMapper.ToJson(o);
        return json;
    }

    //retorna o device_id do aparelho utilizado
    private string GetDeviceInformation()
    {
        Device device;

        //carrega informações do device, se existir
        if (File.Exists(this._configFilePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fs = new FileStream(this._configFilePath, FileMode.Open);
            device = (Device)formatter.Deserialize(fs);
            fs.Close();
            Debug.Log("[MICELIO LOG] Device information was successfully found!");
        }
        else
        {
            Debug.Log("[MICELIO LOG] Device information not found! Loading data to generate the file.");
            device = new Device();

            //teste se todos os campos de Device estão completos
            if(device.VerifyDataIntegrity())
            {
                //envia cadastro do dispositivo e guardando os dados
                SendDevice(device);
            }
        }
        return device.device_id;
    }

    //envia as informações de device para o banco
    private void SendDevice(Device device)
    {
        string payload = ToJSON(device);
        

        //verifica sucesso do cadastro, e cria arquivo de configuração
        if(SendAPIRequest("/device", "POST", payload, "The device data was successfully sent.")){
        
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fs = new FileStream(this._configFilePath, FileMode.Create);
            formatter.Serialize(fs, device);
            fs.Close();
        
            Debug.Log("[MICELIO LOG] Device information was successfully registered!");
        }

    }

    // envia as informações de seção para o banco
    public void StartSession(Session session)
    {
        string payload = ToJSON(session);
        SendAPIRequest("/session", "POST", payload, "The session was successfully started.");
    }

    //envia as informações da atividade para o banco
    public string SendActivity(Activity activity)
    {
        string payload = ToJSON(activity);
        SendAPIRequest("/activity", "POST", payload, "The activity data was successfully sent.");
        return activity.activity_id;
    }

    // envia as informações para fechar uma seção para o banco
    public void CloseSession()
    {
        System.DateTime currentTime = System.DateTime.Now;
        string payload  = "{\"end_time\" : \""+currentTime.ToString("HH:mm:ss")+"\"}";
        SendAPIRequest("/session", "PUT", payload, "The session was successfully ended.");
    }

    //função para envio de dados a API
    private bool SendAPIRequest(string endPoint,
                                string method,
                                string payload,
                                string sucessLogMessage = "The information was successfully sent.",
                                string errorLogMessage = "Cannot send data to Micelio!")
    {
        bool sucess = true;
        try
        {
            var requisicaoWeb = (HttpWebRequest)WebRequest.CreateHttp(this._defaultURL + endPoint);
            

            //criação da requisição e configuração dos parametros
            if(this._is_dev)
            {
                requisicaoWeb = (HttpWebRequest)WebRequest.CreateHttp(this._defaultURL + endPoint + "/test");
            }
                            

            requisicaoWeb.ContentType = "application/json";
            requisicaoWeb.UserAgent = "MicelioUnityAgent";
            requisicaoWeb.Headers.Add("token", this._token);
            requisicaoWeb.Headers.Add("device_id", this._device_id);
            requisicaoWeb.ContentLength = payload.Length;
            requisicaoWeb.Method = method;

            //envio do payload para a API
            using (var stream = new StreamWriter(requisicaoWeb.GetRequestStream()))
            {
                stream.Write(payload);
                stream.Close();
            }

            //recebimento da resposta da API
            using (var resposta = requisicaoWeb.GetResponse())
            {
                //montando a stream de resposta
                var streamDados = resposta.GetResponseStream();
                StreamReader reader = new StreamReader(streamDados);

                //status code da requisição
                HttpStatusCode statusCode = ((HttpWebResponse)resposta).StatusCode;
                
                //resposta do servidor
                object objResponse = reader.ReadToEnd();

                streamDados.Close();
                resposta.Close();
            }
            Debug.Log("[MICELIO LOG] "+ sucessLogMessage + "\n" + payload +"\nRequest -> ["+method+"]: "+endPoint);
        }
        catch(Exception e){
            sucess = false;
            Debug.Log("[MICELIO LOG] "+ errorLogMessage +" \n" + e);
        }
        
        return sucess;
        
    }

}
