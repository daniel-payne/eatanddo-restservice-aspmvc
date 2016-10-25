using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Net;

namespace VoyageMapper
{
  //
  public partial class RestService
  {
    //
    public RestService()
    {
      try
      {
        String connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["LocalTestDataBase"].ConnectionString;
        //
        connection = new SqlConnection(connectionString);
        command    = new SqlCommand();
        //
        command.Connection     = connection;
        command.CommandType    = CommandType.StoredProcedure;
        command.CommandTimeout = 600;
        //
        //connection.Open();
        //
        builder = new StringBuilder();
      }
      catch (Exception e)
      {
        LogException("REST Connection Error", e);
      }
    }
    //~REST()
    //{
    //  if ((connection != null) && (connection.State != ConnectionState.Closed))
    //  {
    //    connection.Close();
    //  }
    //}
    //
    private SqlCommand    command;
    private SqlConnection connection;
    private StringBuilder builder;
    private SqlParameter  sqlParameterReturn;
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    protected String GetUserIPAddress()
    {
      string VisitorsIPAddr = string.Empty;
      //
      if (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
      {
        VisitorsIPAddr = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
      }
      else if (HttpContext.Current.Request.UserHostAddress.Length != 0)
      {
        VisitorsIPAddr = HttpContext.Current.Request.UserHostAddress;
      }
      //
      return VisitorsIPAddr;
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    private String GetAuthorizationData()
    {
      //
      WebHeaderCollection headers = WebOperationContext.Current.IncomingRequest.Headers;
      //
      String authorizationData = headers["Authorization"];
      //
      if ( (authorizationData == null) || (authorizationData == String.Empty))
      {
        return null;
      }
      else
      {
        return decodeBASE64(authorizationData);
      }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    private void SetAuthorizationData(String AuthorizationData)
    {
      //
      WebHeaderCollection headers = WebOperationContext.Current.OutgoingResponse.Headers;
      //
      headers["Authorization"] = decodeBASE64(AuthorizationData);
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void resetJSONBuilder()
    {
      builder.Clear();
      //
      builder.AppendLine("[");
    }
    //
    private void addJSONRecord()
    {
      builder.Append("{");
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    private void addJSONData(String FieldName, Object FieldData)
    {
      if (FieldData.ToString() != String.Empty)
      {
        //
        StringBuilder data = new StringBuilder(FieldData.ToString());
        //
        data.Replace("\"", "\\\"");
        //
        if ((FieldData is Double) || (FieldData is Int32) ){
          builder.Append(" \"" + FieldName + "\": " + data.ToString() + ", ");
        } else {
          builder.Append(" \"" + FieldName + "\": \"" + data.ToString() + "\", ");
        };
      }
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    private void finiliseJSONRecord()
    {
      //
      builder.Remove(builder.Length - 2, 1);
      builder.AppendLine("},");
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    private void finaliseJSONBuilder()
    {
      //
      if (builder.Length > 3)
        builder.Remove(builder.Length - 3, 1);
      //
      builder.AppendLine("]");
      //
      String userAgent = HttpContext.Current.Request.Headers["User-Agent"];
      //
      if ((userAgent == null) || ((userAgent.IndexOf("MSIE ") > 0) && (userAgent.IndexOf("MSIE 10") == -1)))
        HttpContext.Current.Response.ContentType = "text/plain; charset=utf-8";
      else
        HttpContext.Current.Response.ContentType = "application/json; charset=utf-8";
      //
      HttpContext.Current.Response.Headers.Add("Cache-Control", "no-store, must-revalidate, no-cache, max-age=0");
      HttpContext.Current.Response.Headers.Add("Expires", "Mon, 01 Jan 1990 00:00:00 GMT");
      HttpContext.Current.Response.Headers.Add("Pragma", "no-cache");
      //
      HttpContext.Current.Response.Write(builder.ToString());
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    private void resetSQLCommand(String CommandName)
    {
      //
      command.CommandText = CommandName;
      //
      command.Parameters.Clear();
      //
      sqlParameterReturn = new SqlParameter();
      //
      sqlParameterReturn.Direction = ParameterDirection.ReturnValue;
      //
      command.Parameters.Add(sqlParameterReturn);
      //
      resetJSONBuilder();
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    private void setupSQLCommand(String Name, SqlDbType Type, Int32? Length, Object Value)
    {
      //
      SqlParameter parmUser;
      //
      if (!Length.HasValue)
        parmUser = new SqlParameter("@" + Name, Type);
      else
        parmUser = new SqlParameter("@" + Name, Type, Length.Value);
      //
      parmUser.Direction = ParameterDirection.Input;
      //
      parmUser.Value = Value;
      //
      command.Parameters.Add(parmUser);
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    private void finaliseSQLCommand()
    {
      //
      if (connection.State == ConnectionState.Closed)
      {
        connection.Open();
      }
      //
      SqlDataReader dataReader = null;
      //
      try
      {
        //
        dataReader = command.ExecuteReader(CommandBehavior.SequentialAccess);
        //
        if ((sqlParameterReturn.Value != null) && ((Int32)sqlParameterReturn.Value == 0))
        {
          respondWithNoData();
          return;
        };
        //
        while (dataReader.Read())
        {
          //
          addJSONRecord();
          //
          for (int i = 0; i < dataReader.FieldCount; i++)
          {
            addJSONData(dataReader.GetName(i).ToString(), dataReader[i]);
          }
          //
          finiliseJSONRecord();
        };
        //
        finaliseJSONBuilder();
      }
      catch (Exception e)
      {
        respondWithError(e);
      }
      finally
      {
        //
        if (dataReader != null)
        {
          dataReader.Close();
        }
        //
        if (connection.State == ConnectionState.Open)
        {
          connection.Close();
        }
      };
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    private String decodeBASE64(String Input)
    {
      //
      Byte[] encodedDataAsBytes = Convert.FromBase64String(Input);
      //
      String returnValue = Encoding.UTF8.GetString(encodedDataAsBytes);
      //
      return returnValue;
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    private String encodeBASE64(String Input)
    {
      //
      Byte[] encodedDataAsBytes = Encoding.UTF8.GetBytes(Input);
      //
      String returnValue = Convert.ToBase64String(encodedDataAsBytes);
      //
      return returnValue;
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    private Dictionary<String, String> decodeDescription(String Input)
    {
      //
      Dictionary<String, String> result = new Dictionary<string, string>();
      //
      String[] data = Input.Split('[')
                           .Where(s => s.Length > 0)
                           .Select(s => s.Replace("]", String.Empty))
                           .ToArray();
      //
      foreach (String s in data)
      {
        //
        String[] recoard = s.Split('=');
        //
        if ((recoard.Length == 2) && (!result.ContainsKey(recoard[0])))
        {
          result.Add(recoard[0], recoard[1]);
        };
      }
      //
      return result;
    }
    //
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void LogException(String ErrorLog, Exception e, String Information = null)
    {
      //
      String FileName = Guid.NewGuid().ToString();
      //
      String Message = e.Message.ToString();
      String Inner = String.Empty;
      String StackTrace = e.StackTrace;
      //
      if (e.InnerException != null)
        Inner = e.InnerException.Message.ToString();
      //
      try
      {
        //
        StringBuilder stringBuilder = new StringBuilder(ErrorLog);
        //
        stringBuilder.AppendLine(String.Empty);
        stringBuilder.AppendLine("========================================================================================================================");
        stringBuilder.AppendLine("IP Address");
        stringBuilder.AppendLine("========================================================================================================================");
        stringBuilder.AppendLine(String.Empty);
        //
        stringBuilder.AppendLine(HttpContext.Current.Request.UserHostAddress);
        //
        stringBuilder.AppendLine(Message);
        stringBuilder.AppendLine(Inner);
        stringBuilder.AppendLine(StackTrace);
        //
        if (!String.IsNullOrEmpty(Information))
        {
          stringBuilder.AppendLine(String.Empty);
          stringBuilder.AppendLine("========================================================================================================================");
          stringBuilder.AppendLine("Information");
          stringBuilder.AppendLine("========================================================================================================================");
          stringBuilder.AppendLine(Information);
          stringBuilder.AppendLine(String.Empty);
        };
        //
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\EatAndDo.ClientErrors\" + FileName + ".txt", true))
        {
          //
          file.Write(stringBuilder.ToString());
          //
          file.Close();
        };
      }
      catch
      {
        //DO NOTHING
      };
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    private void respondWithSystemError(String Message = "System Error")
    {
      ////
      OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
      //
      response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
      //
      response.StatusDescription = Message;
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    private void respondWithNoAuthentication(String Message = "No Authentication")
    {
      ////
      OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
      //
      response.StatusCode = System.Net.HttpStatusCode.Unauthorized;
      //
      response.StatusDescription = Message;
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    private void respondWithNoAuthorization(String Message = "No Authorization")
    {
      ////
      OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
      //
      response.StatusCode = System.Net.HttpStatusCode.Unauthorized;
      //
      response.StatusDescription = Message;
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    private void respondWithNoData(String Message = "No Data")
    {
      ////
      OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
      //
      response.StatusCode = System.Net.HttpStatusCode.NoContent;
      //
      response.StatusDescription = Message;
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //
    private void respondWithError(Exception e)
    {
      ////
      OutgoingWebResponseContext response = WebOperationContext.Current.OutgoingResponse;
      //
      response.StatusCode = System.Net.HttpStatusCode.BadRequest;
      //
      response.StatusDescription = e.Message;
      //
      LogException("REST Client Error", e, WebOperationContext.Current.IncomingRequest.ToString());
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

  }
}

