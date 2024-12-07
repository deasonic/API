using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;

class Program
{
    
    private static double Voltage = 230;      
    private static double Current = 15;       
    private static double Frequency = 50;     
    private static double Power = 5000;       
    private static bool GeneratorActive = false; 
    private static StringBuilder Logs = new StringBuilder(); 

    static void Main(string[] args)
    {
      
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();

        Console.WriteLine("Server is running at http://localhost:8080/");

        while (true)
        {
            
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

           
            if (request.Url.AbsolutePath == "/api/sensors" && request.HttpMethod == "GET")
            {
                HandleGetSensorData(response);
            }
            else if (request.Url.AbsolutePath == "/api/sensors" && request.HttpMethod == "POST")
            {
                HandleUpdateSensorData(request, response);
            }
            else if (request.Url.AbsolutePath == "/api/manual-control" && request.HttpMethod == "POST")
            {
                HandleManualControl(request, response);
            }
            else if (request.Url.AbsolutePath == "/api/logs" && request.HttpMethod == "GET")
            {
                HandleGetLogs(response);
            }
            else
            {
                response.StatusCode = 404;
                response.Close();
            }
        }
    }

    
    private static void HandleGetSensorData(HttpListenerResponse response)
    {
        var data = new
        {
            Voltage,
            Current,
            Frequency,
            Power,
            GeneratorActive
        };

        string jsonResponse = JsonConvert.SerializeObject(data);

        byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.Close();
    }

    
    private static void HandleUpdateSensorData(HttpListenerRequest request, HttpListenerResponse response)
    {
        using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
        {
            string body = reader.ReadToEnd();
            var sensorData = JsonConvert.DeserializeObject<SensorData>(body);

            Voltage = sensorData.Voltage;
            Current = sensorData.Current;
            Frequency = sensorData.Frequency;
            Power = sensorData.Power;

            Logs.AppendLine($"[Sensor Update] Voltage: {Voltage} V, Current: {Current} A, Frequency: {Frequency} Hz, Power: {Power} W");
            response.StatusCode = 200;
            response.Close();
        }
    }

    
    private static void HandleManualControl(HttpListenerRequest request, HttpListenerResponse response)
    {
        using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
        {
            string body = reader.ReadToEnd();
            var controlCommand = JsonConvert.DeserializeObject<ControlCommand>(body);

            if (controlCommand.Action == "StartGenerator")
            {
                GeneratorActive = true;
                Logs.AppendLine("[Manual Control] Generator started.");
            }
            else if (controlCommand.Action == "StopGenerator")
            {
                GeneratorActive = false;
                Logs.AppendLine("[Manual Control] Generator stopped.");
            }
            else
            {
                response.StatusCode = 400;
                response.Close();
                return;
            }

            response.StatusCode = 200;
            response.Close();
        }
    }

   
    private static void HandleGetLogs(HttpListenerResponse response)
    {
        string logs = Logs.ToString();

        byte[] buffer = Encoding.UTF8.GetBytes(logs);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.Close();
    }
}


public class SensorData
{
    public double Voltage { get; set; }   
    public double Current { get; set; }   
    public double Frequency { get; set; } 
    public double Power { get; set; }    
}

public class ControlCommand
{
    public string Action { get; set; } 
}