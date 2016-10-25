using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Data;
using System.ComponentModel;
using System.Reflection;
using System.Web;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace VoyageMapper
{
  [ServiceContract]
  [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
  [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
  //
  public partial class RestService
  {
    //
    [WebInvoke(UriTemplate = "ping", Method = "GET")]
    [Description("Returns a string \"Hello World's\" with the current \r\n server time.")]
    public void Ping()
    {
      resetJSONBuilder();
      //
      addJSONRecord();
      //
      String fullName = Assembly.GetExecutingAssembly().FullName;
      AssemblyName assemblyName = new AssemblyName(fullName);
      //
      String time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
      String serverName = System.Environment.MachineName.Substring(0, 5);
      String versionInformation = assemblyName.Version.Major + "." + assemblyName.Version.Minor + " (" + assemblyName.Version.Build + ")";
      //
      addJSONData("time", time);
      addJSONData("serverName", serverName);
      addJSONData("versionInformation", versionInformation);
      //
      finiliseJSONRecord();
      //
      finaliseJSONBuilder();
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //GetAuthorizationForExistingUser
    [WebInvoke(UriTemplate = "existingUsers/authorizations?eMail={Base64EMail}&password={Base64Password}", Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
    [Description("Returns authorization code for an existing user.")]
    public void GetAuthorizationForExistingUser(String Base64EMail, String Base64Password)
    {
      //
      resetJSONBuilder();
      //
      resetSQLCommand("Security.GetAuthorizationForExistingUser");
      //
      setupSQLCommand("EMail",     SqlDbType.VarChar, 255, decodeBASE64(Base64EMail));
      setupSQLCommand("Password",  SqlDbType.VarChar, 255, decodeBASE64(Base64Password));
      setupSQLCommand("IPAddress", SqlDbType.VarChar, 255, GetUserIPAddress());
      //
      finaliseSQLCommand();
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //GetAuthorizationForNewUser
    [WebInvoke(UriTemplate = "newUsers/authorizations?eMail={Base64EMail}&password={Base64Password}", Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
    [Description("Returns authorization code for an new user, setting up a group for them to manage.")]
    public void GetAuthorizationForNewUser(String Base64EMail, String Base64Password)
    {
      //
      resetJSONBuilder();
      //
      resetSQLCommand("Security.GetAuthorizationForNewUser");
      //
      setupSQLCommand("EMail",     SqlDbType.VarChar, 255, decodeBASE64(Base64EMail));
      setupSQLCommand("Password",  SqlDbType.VarChar, 255, decodeBASE64(Base64Password));
      setupSQLCommand("IPAddress", SqlDbType.VarChar, 255, GetUserIPAddress());
      //
      finaliseSQLCommand();
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //GetAuthorizationForAnonymousUser
    [WebInvoke(UriTemplate = "anonymousUsers/authorizations", Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
    [Description("Returns authorization code for an anonymous user")]
    public void GetAuthorizationForAnonymousUser()
    {
      //
      resetJSONBuilder();
      //
      resetSQLCommand("Security.GetAuthorizationForAnonymousUser");
      //
      setupSQLCommand("IPAddress", SqlDbType.VarChar, 255, GetUserIPAddress());
      //
      finaliseSQLCommand();
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //GetDays
    [WebInvoke(UriTemplate = "days?session={session}&date={dayDate}&count={count}", Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
    [Description("Returns the days in the diary of a person ")]
    public void GetDays(String session, String dayDate, String count = null)
    {
      Int32? dayCount = null;
      Guid sessionGUID = Guid.Parse(session);
      //
      if ((count != null) && (count != String.Empty))
      {
        dayCount = Int32.Parse(count);
      }
      //
      resetJSONBuilder();
      //
      resetSQLCommand("diary.GetDay");
      //
      setupSQLCommand("SessionGuid", SqlDbType.UniqueIdentifier, null, sessionGUID);
      setupSQLCommand("DayDate", SqlDbType.Date, null, dayDate);
      //
      if (dayCount != null)
      {
        setupSQLCommand("DayCount", SqlDbType.Int, null, dayCount);
      }
      //
      finaliseSQLCommand();
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //GetDetails
    [WebInvoke(UriTemplate = "details?session={session}&date={dayDate}", Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
    [Description("Returns the details about a day in the diary of a person ")]
    public void GetDetails(String session, String dayDate)
    {
      //
      Guid sessionGUID = Guid.Parse(session);
      //
      resetJSONBuilder();
      //
      resetSQLCommand("diary.GetDetails");
      //
      setupSQLCommand("SessionGuid", SqlDbType.UniqueIdentifier, null, sessionGUID);
      setupSQLCommand("DayDate", SqlDbType.Date,                 null, dayDate    );
      //
      finaliseSQLCommand();
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //GetMatchingFoodNames
    [WebInvoke(UriTemplate = "matchingFoodNames?match={uriEncodedSearch}&sources={uriEncodedSources}&maxResults={maxResults}", Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
    [Description("Returns a list of matching foods, use base64 Encoding for the search. pork pie is 'pork%20pie' or comma separated 'pork,pie' ")]
    public void GetMatchingFoodNames(String uriEncodedSearch, String uriEncodedSources, String maxResults)
    {
      //
      resetJSONBuilder();
      //
      resetSQLCommand("search.GetMatchingFoodNames");
      //
      setupSQLCommand("Search", SqlDbType.VarChar, 255, HttpUtility.UrlDecode(uriEncodedSearch));
      //
      if (uriEncodedSources != null)
      {
        setupSQLCommand("Sources", SqlDbType.VarChar, 255, HttpUtility.UrlDecode(uriEncodedSources));
      }
      //
      if (maxResults != null)
      {
        setupSQLCommand("MaxResults", SqlDbType.Int, null, Int32.Parse(maxResults));
      }
      //
      finaliseSQLCommand();
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //GetFoods
    [WebInvoke(UriTemplate = "foods?ids={ids}", Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
    [Description("Returns foods details for a comma separated list of foods")]
    public void GetFoods(String ids)
    {
      //
      resetJSONBuilder();
      //
      resetSQLCommand("search.GetFoods");
      //
      setupSQLCommand("IDs", SqlDbType.VarChar, -1, ids);
      //
      finaliseSQLCommand();
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //GetUnits
    [WebInvoke(UriTemplate = "units?showFullDetails={ShowFullDetails}", Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
    [Description("Returns unit details for all foods")]
    public void GetUnits(Boolean ShowFullDetails = true)
    {
      //
      resetJSONBuilder();
      //
      resetSQLCommand("search.GetUnits");
      //
      setupSQLCommand("ShowFullDetails", SqlDbType.Bit, null, ShowFullDetails);
      //
      finaliseSQLCommand();
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //GetFoodEntryCalculation
    [WebInvoke(UriTemplate = "foodCalculations?foodid={FoodID}&amount={Amount}&unitName={UnitName}", Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
    [Description("Returns unit details for all foods")]
    public void GetFoodEntryCalculation(String FoodID, String Amount, String UnitName)
    {
      //
      Double amount = Double.Parse(Amount);
      //
      resetJSONBuilder();
      //
      resetSQLCommand("search.GetFoodEntryCalculation");
      //
      setupSQLCommand("FoodID",    SqlDbType.Int, null, FoodID);
      setupSQLCommand("Amount",    SqlDbType.Float, null, amount);
      setupSQLCommand("UnitName",  SqlDbType.VarChar, 255, UnitName);
      //
      finaliseSQLCommand();
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //PostFoodEntry
    //[WebInvoke(UriTemplate = "days/foodCalculations?session={session}&foodDescription={foodDescription}&amountDescription={amountDescription}&dayDate={dayDate}&mealtime={mealtime}&energy={energy}&protein={protein}&carbohydrate={carbohydrate}&sugar={sugar}&starch={starch}&fat={fat}&saturatedFat={saturatedFat}&unsaturatedFat={unsaturatedFat}&cholesterol={cholesterol}&transFat={transFat}&dietaryFibre={dietaryFibre}&solubleFibre={solubleFibre}&insolubleFibre={insolubleFibre}&sodium={sodium}&alcohol={alcohol}", Method = "GET", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
    [WebInvoke(UriTemplate = "days/foodCalculations?session={session}&dayDate={dayDate}&mealName={mealName}", Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
    [Description("Save a food calculation to the diary, and return the updated day")]
    //public void PostFoodEntry(String session, String foodDescription, String amountDescription, String dayDate, String mealtime, String energy = null, String protein = null, String carbohydrate = null, String sugar = null, String starch = null, String fat = null, String saturatedFat = null, String unsaturatedFat = null, String cholesterol = null, String transFat = null, String dietaryFibre = null, String solubleFibre = null, String insolubleFibre = null, String sodium = null, String alcohol = null)
    public void PostFoodEntry(String session, String dayDate, String mealName)
    {

      JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

      String                     body  = Encoding.UTF8.GetString(OperationContext.Current.RequestContext.RequestMessage.GetBody<byte[]>());
      Dictionary<String, String> data  = javaScriptSerializer.Deserialize<Dictionary<String, String>>(body);

      //
      Guid sessionGUID = Guid.Parse(session);
      //
      resetJSONBuilder();
      //
      resetSQLCommand("diary.PostFoodEntry");
      //
      setupSQLCommand("SessionGuid",           SqlDbType.UniqueIdentifier, null, sessionGUID);
      //
      setupSQLCommand("DayDate",               SqlDbType.Date,    null, dayDate);
      setupSQLCommand("MealName",              SqlDbType.VarChar, 255,  mealName);
      //
      setupSQLCommand("FoodDescription",       SqlDbType.VarChar, 255, data["foodDescription"]);
      setupSQLCommand("AmountDescription",     SqlDbType.VarChar, 255, data["amountDescription"]);
      //                                                                                                                          }
      if ( data.ContainsKey("energy")          ) { setupSQLCommand("energyKiloJoulesPerEntry",     SqlDbType.Float,  null, data["energy"]);            }
      if ( data.ContainsKey("protein")         ) { setupSQLCommand("proteinGramsPerEntry",         SqlDbType.Float,  null, data["protein"]);           }
      if ( data.ContainsKey("carbohydrate")    ) { setupSQLCommand("carbohydrateGramsPerEntry",    SqlDbType.Float,  null, data["carbohydrate"]);      }
      if ( data.ContainsKey("sugar")           ) { setupSQLCommand("sugarGramsPerEntry",           SqlDbType.Float,  null, data["sugar"]);             }
      if ( data.ContainsKey("starch")          ) { setupSQLCommand("starchGramsPerEntry",          SqlDbType.Float,  null, data["starch"]);            }
      if ( data.ContainsKey("fat")             ) { setupSQLCommand("fatGramsPerEntry",             SqlDbType.Float,  null, data["fat"]);               }
      if ( data.ContainsKey("saturatedFat")    ) { setupSQLCommand("saturatedFatGramsPerEntry",    SqlDbType.Float,  null, data["saturatedFat"]);      }
      if ( data.ContainsKey("unsaturatedFat")  ) { setupSQLCommand("unsaturatedFatGramsPerEntry",  SqlDbType.Float,  null, data["unsaturatedFat"]);    }
      if ( data.ContainsKey("cholesterol")     ) { setupSQLCommand("cholesterolGramsPerEntry",     SqlDbType.Float,  null, data["cholesterol"]);       }
      if ( data.ContainsKey("transFat")        ) { setupSQLCommand("transFatGramsPerEntry",        SqlDbType.Float,  null, data["transFat"]);          }
      if ( data.ContainsKey("dietaryFibre")    ) { setupSQLCommand("dietaryFibreGramsPerEntry",    SqlDbType.Float,  null, data["dietaryFibre"]);      }
      if ( data.ContainsKey("solubleFibre")    ) { setupSQLCommand("solubleFibreGramsPerEntry",    SqlDbType.Float,  null, data["solubleFibre"]);      }
      if ( data.ContainsKey("insolubleFibre")  ) { setupSQLCommand("insolubleFibreGramsPerEntry",  SqlDbType.Float,  null, data["insolubleFibre"]);    }
      if ( data.ContainsKey("sodium")          ) { setupSQLCommand("sodiumGramsPerEntry",          SqlDbType.Float,  null, data["sodium"]);            }
      if ( data.ContainsKey("alcohol")         ) { setupSQLCommand("alcoholGramsPerEntry",         SqlDbType.Float,  null, data["alcohol"]);           }
      //
      finaliseSQLCommand();
    }
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  }
}

//
//String signalText = Encoding.UTF8.GetString(OperationContext.Current.RequestContext.RequestMessage.GetBody<byte[]>()); ;






