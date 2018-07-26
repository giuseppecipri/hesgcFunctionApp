using System;
using System.Data.SqlClient;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace hesgcFunctionApp
{
    /*
    Exemple de <Application de fonction> (Function App) 
    Cette fonction travaille sur une file d'attente Service Bus et pas sur le service de stockage
    Cette fonction est une copie, pour backup, de la fonction en ligne sur le compte Azure.
    */
    public static class hesgcstockageFonction
    {
        [FunctionName("IoTHubServiceBusQueueTrigger")]
        public static void Run(string myQueueItem, TraceWriter log)
        {
            log.Info($"--------------- Processed message: <{myQueueItem}>");// For test

            // test if message contains key word <protocolVersion>
            if (myQueueItem.Contains("protocolVersion"))
            {
                try
                {
                    /*    
                            // With dynamic object
                            dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(myQueueItem);
                            if(obj != null){
                                log.Info($"{obj.ToString()}");
                            } 
                    */

                    // With TelemetryData object
                    TelemetryData obj = Newtonsoft.Json.JsonConvert.DeserializeObject<TelemetryData>(myQueueItem);
                    if (obj != null)
                    {
                        //log.Info($" TelemetryData ToString {obj.ToString()}");// For test

                        // Set connectionString for Azure SQL Server
                        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                        {
                            DataSource =  "your_server.database.windows.net",
                            UserID = "your_user",
                            Password = "your_password",
                            InitialCatalog = "your_database"
                        };
                        SqlConnection sqlConnection = new SqlConnection(builder.ConnectionString);
                        sqlConnection.Open();

                        // Set SQL command for insert new Message with sqlConnection
                        SqlCommand cmd = new SqlCommand();
                        cmd.Connection = sqlConnection;
                        cmd.CommandType = System.Data.CommandType.Text;

                        cmd.CommandText = "INSERT Message ( Device_IdAzure, Protocol_version, Message_data, Sending_date, Created_date )" +
                                            " VALUES ( @deviceId, @protocolVersion, @messageData, @sending_date, @created_date )";
                        // set parameters for sql command
                        cmd.Parameters.AddWithValue("deviceId", obj.deviceId);
                        cmd.Parameters.AddWithValue("protocolVersion", obj.protocolVersion);
                        cmd.Parameters.AddWithValue("messageData", obj.messageData);
                        cmd.Parameters.AddWithValue("sending_date", obj.sendingDateTime);
                        cmd.Parameters.AddWithValue("created_date", DateTime.UtcNow);//;Now.ToUniversalTime());

                        //           log.Info($"Query.CommandText : {cmd.CommandText}"); // For test

                        // Execute and close query
                        cmd.ExecuteNonQuery();
                        sqlConnection.Close();

                    }
                }
                catch (Exception e)
                {
                    log.Error(e.ToString());
                }
            }
            else
            {
                log.Info("myQueueItem = Service Bus Message");// For test
            }

        }

        public class TelemetryData
        {
            public int protocolVersion { get; set; }
            public DateTime sendingDateTime { get; set; }
            public string deviceId { get; set; }
            public string messageData { get; set; }
            public double temperature { get; set; }
            public double humidity { get; set; }
            public string pointInfo { get; set; }


        }
    }
}
