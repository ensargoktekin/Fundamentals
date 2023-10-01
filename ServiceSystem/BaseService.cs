using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BestHTTP;
using UnityEngine;

public class BaseService
{
    public async Task<OperationResult<T>> Get<T>(string url)
    {
        string username = SessionData.Instance.ActiveUser.userId;
        string password = SessionData.Instance.ActiveUser.token;
        
        string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));

        bool requestReady = false;

        OperationResult<T> operationRes = new OperationResult<T>();
        HTTPRequest httpRequest = new HTTPRequest(new Uri(url), (HTTPRequest request, HTTPResponse response) =>
        {
            if (response == null)
            {
                operationRes.successful = false;
            }
            else
            {
                string responseData = Encoding.Default.GetString(response.Data);
                //UnityEngine.Debug.LogError(responseData);
                operationRes = JsonConvert.DeserializeObject<OperationResult<T>>(responseData);
            }

            requestReady = true;
        });
        httpRequest.AddHeader("Authorization", $"Basic {svcCredentials}");
        httpRequest.Send();

        while (!requestReady)
            await Task.Delay(50);

        return operationRes;
    }

    public async Task<string> GetFrontEndVersion(string url)
    {
        string version = null;

        string username = SessionData.Instance.ActiveUser.userId;
        string password = SessionData.Instance.ActiveUser.token;
        string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));

        bool requestReady = false;

        HTTPRequest httpRequest = new HTTPRequest(new Uri(url), (HTTPRequest request, HTTPResponse response) =>
        {
            if (response == null)
            {
                version = null;
            }
            else
            {
                string responseData = Encoding.Default.GetString(response.Data); ;
                version = responseData;
            }

            requestReady = true;
        });
        httpRequest.AddHeader("Authorization", $"Basic {svcCredentials}");
        httpRequest.Send();

        while (!requestReady)
            await Task.Delay(50);

        return version;
    }

    /// <summary>
    /// Used for Post request which has no data to send to BE. 
    /// </summary>
    public async Task<OperationPostResult> Post(string url, string postData)
    {
        OperationPostResult result = new OperationPostResult();
        try
        {
            string username = SessionData.Instance.ActiveUser.userId;
            string password = SessionData.Instance.ActiveUser.token;
            string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
            string json = JsonConvert.SerializeObject(postData);
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);

            bool requestReady = false;

            HTTPRequest httpRequest = new HTTPRequest(new Uri(url), HTTPMethods.Post, (HTTPRequest request, HTTPResponse response) =>
            {
                if (response == null)
                {
                    string errLog = "Post_1\n";
                    errLog += $"url: {url}\n";
                    if (postData != null) errLog += $"postData: {postData}\n";
                    else errLog += $"postData: NULL\n";
                    errLog += $"exception: {request.Exception}\n";

                    result.successful = false;
                }
                else
                {
                    string responseData = Encoding.Default.GetString(response.Data); ;
                    result = JsonConvert.DeserializeObject<OperationPostResult>(responseData);
                }
                requestReady = true;
            });
            httpRequest.AddHeader("Content-Type", "application/json");
            httpRequest.AddHeader("Authorization", $"Basic {svcCredentials}");
            httpRequest.RawData = jsonToSend;
            httpRequest.Send();

            while (!requestReady)
                await Task.Delay(50);

        }
        catch (Exception ex)
        {
            Debug.Log("Exception in Post: " + ex.Message + " /// " + ex.StackTrace);
        }
        return result;
    }

    /// <summary>
    /// Used for Post request which has to send some data to BE without expecting usable information
    /// as result other than success and error.
    /// (i.e. when IAP occurs)
    /// </summary>
    public async Task<OperationPostResult> Post<T>(string url, T postData)
    {
        OperationPostResult result = new OperationPostResult();
        try
        {
            string username = SessionData.Instance.ActiveUser.userId;
            string password = SessionData.Instance.ActiveUser.token;
            string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
            string json = JsonConvert.SerializeObject(postData);
            byte[] jsonToSend = new UTF8Encoding().GetBytes(json);

            bool requestReady = false;

            HTTPRequest httpRequest = new HTTPRequest(new Uri(url), HTTPMethods.Post, (HTTPRequest request, HTTPResponse response) =>
            {
                if (response == null)
                {
                    string errLog = "Post_1\n";
                    errLog += $"url: {url}\n";
                    if (postData != null) errLog += $"postData: {postData}\n";
                    else errLog += $"postData: NULL\n";
                    errLog += $"exception: {request.Exception}\n";
                    Debug.LogError(errLog);

                    result.successful = false;
                }
                else
                {
                    string responseData = Encoding.Default.GetString(response.Data); ;
                    result = JsonConvert.DeserializeObject<OperationPostResult>(responseData);
                }
                requestReady = true;
            });
            httpRequest.AddHeader("Content-Type", "application/json");
            httpRequest.AddHeader("Authorization", $"Basic {svcCredentials}");
            httpRequest.RawData = jsonToSend;
            httpRequest.Send();

            while (!requestReady)
                await Task.Delay(50);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception in Post: " + ex.Message + " /// " + ex.StackTrace);
        }
        return result;
    }

    /// <summary>
    /// Used for Post request when response from BE has usable information other than success and error.
    /// </summary>
    /// <typeparam name="T">Return type of Operation Result</typeparam>
    /// <typeparam name="Y">Object type to send with request.</typeparam>
    /// <returns></returns>
    public async Task<OperationResult<T>> Post<T, Y>(string url, Y postData)
    {
        OperationResult<T> result = new OperationResult<T>();
        try
        {

            string username = SessionData.Instance.ActiveUser.userId;
            string password = SessionData.Instance.ActiveUser.token;
            string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
            string json = JsonConvert.SerializeObject(postData);
            byte[] jsonToSend = new UTF8Encoding().GetBytes(json);

            bool requestReady = false;

            HTTPRequest httpRequest = new HTTPRequest(new Uri(url), HTTPMethods.Post, (HTTPRequest request, HTTPResponse response) =>
            {
                if (response == null)
                {
                    string errLog = "Post_1\n";
                    errLog += $"url: {url}\n";
                    if (postData != null) errLog += $"postData: {postData}\n";
                    else errLog += $"postData: NULL\n";
                    errLog += $"exception: {request.Exception}\n";
                    Debug.LogError(errLog);

                    result.successful = false;
                }
                else
                {
                    string responseData = Encoding.Default.GetString(response.Data); ;
                    result = JsonConvert.DeserializeObject<OperationResult<T>>(responseData);
                }
                requestReady = true;
            });
            httpRequest.AddHeader("Content-Type", "application/json");
            httpRequest.AddHeader("Authorization", $"Basic {svcCredentials}");
            httpRequest.RawData = jsonToSend;
            httpRequest.Send();

            while (!requestReady)
                await Task.Delay(50);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception in Post: " + ex.Message + " /// " + ex.StackTrace);
        }
        return result;
    }

    /// <summary>
    /// Used for Post request when response from BE has usable information other than success and error
    /// without sending any info to the BE
    /// </summary>
    /// <typeparam name="T">Return type of Operation Result</typeparam>
    /// <returns></returns>
    public async Task<OperationResult<T>> Post<T>(string url, string postDataString)
    {
        OperationResult<T> result = new OperationResult<T>();
        try
        {
            string username = SessionData.Instance.ActiveUser.userId;
            string password = SessionData.Instance.ActiveUser.token;
            string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
            string json = JsonConvert.SerializeObject(postDataString);
            byte[] jsonToSend = new UTF8Encoding().GetBytes(json);

            bool requestReady = false;

            HTTPRequest httpRequest = new HTTPRequest(new Uri(url), HTTPMethods.Post, (HTTPRequest request, HTTPResponse response) =>
            {
                if (response == null)
                {
                    string errLog = "Post_1\n";
                    errLog += $"url: {url}\n";
                    if (postDataString != null) errLog += $"postData: {postDataString}\n";
                    else errLog += $"postData: NULL\n";
                    errLog += $"exception: {request.Exception}\n";
                    Debug.LogError(errLog);

                    result.successful = false;
                }
                else
                {
                    string responseData = Encoding.Default.GetString(response.Data);
                    result = JsonConvert.DeserializeObject<OperationResult<T>>(responseData);
                }
                requestReady = true;
            });
            httpRequest.AddHeader("Content-Type", "application/json");
            httpRequest.AddHeader("Authorization", $"Basic {svcCredentials}");
            httpRequest.RawData = jsonToSend;
            httpRequest.Send();

            while (!requestReady)
                await Task.Delay(50);
        }
        catch (Exception ex)
        {
            Debug.Log("Exception in Post: " + ex.Message + " /// " + ex.StackTrace);
        }
        return result;
    }

    /// <summary>
    /// Used for Put request which doesn't expect usable information as result.
    /// </summary>
    public async Task<OperationPostResult> Put(string url, string bodyData)
    {
        OperationPostResult result = new OperationPostResult();

        string username = SessionData.Instance.ActiveUser.userId;
        string password = SessionData.Instance.ActiveUser.token;
        string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));

        bool requestReady = false;

        HTTPRequest httpRequest = new HTTPRequest(new Uri(url), HTTPMethods.Put, (HTTPRequest request, HTTPResponse response) =>
        {
            if (response == null)
            {
                string errLog = "Post_1\n";
                errLog += $"url: {url}\n";
                if (bodyData != null) errLog += $"postData: {bodyData}\n";
                else errLog += $"bodyData: NULL\n";
                errLog += $"exception: {request.Exception}\n";
                Debug.LogError(errLog);

                result.successful = false;
            }
            else
            {
                string responseData = Encoding.Default.GetString(response.Data);
                result = JsonConvert.DeserializeObject<OperationPostResult>(responseData);
            }
            requestReady = true;
        });
        httpRequest.AddHeader("Content-Type", "application/json");
        httpRequest.AddHeader("Authorization", $"Basic {svcCredentials}");
        httpRequest.Send();

        while (!requestReady)
            await Task.Delay(50);

        return result;
    }

    /// <summary>
    /// Used for Put requests when response from BE has usable information other than success and error.
    /// (i.e. Mini game put request returns Reward amount)
    /// </summary>
    public async Task<OperationResult<T>> Put<T>(string url, string bodyData)
    {
        OperationResult<T> result = new OperationResult<T>();

        string username = SessionData.Instance.ActiveUser.userId;
        string password = SessionData.Instance.ActiveUser.token;
        string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));

        bool requestReady = false;

        HTTPRequest httpRequest = new HTTPRequest(new Uri(url), HTTPMethods.Put, (HTTPRequest request, HTTPResponse response) =>
        {
            if (response == null)
            {
                string errLog = "Post_1\n";
                errLog += $"url: {url}\n";
                if (bodyData != null) errLog += $"postData: {bodyData}\n";
                else errLog += $"bodyData: NULL\n";
                errLog += $"exception: {request.Exception}\n";
                    Debug.LogError(errLog);

                result.successful = false;
            }
            else
            {
                string responseData = Encoding.Default.GetString(response.Data);
                result = JsonConvert.DeserializeObject<OperationResult<T>>(responseData);
            }
            requestReady = true;
        });
        httpRequest.AddHeader("Content-Type", "application/json");
        httpRequest.AddHeader("Authorization", $"Basic {svcCredentials}");
        httpRequest.Send();

        while (!requestReady)
            await Task.Delay(50);

        return result;
    }

    /// <summary>
    /// Used for Put request which has to send some data to BE without expecting usable information
    /// as result other than success and error.
    /// (i.e. when user changes username)
    /// </summary>
    public async Task<OperationPostResult> Put<T>(string url, T putData) {
        OperationPostResult result = new OperationPostResult();
        try {
            string username = SessionData.Instance.ActiveUser.userId;
            string password = SessionData.Instance.ActiveUser.token;
            string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
            string json = JsonConvert.SerializeObject(putData);
            byte[] jsonToSend = new UTF8Encoding().GetBytes(json);

            bool requestReady = false;

            HTTPRequest httpRequest = new HTTPRequest(new Uri(url), HTTPMethods.Put, (HTTPRequest request, HTTPResponse response) => {
                if (response == null) {
                    string errLog = "Put_1\n";
                    errLog += $"url: {url}\n";
                    if (putData != null) errLog += $"putData: {putData}\n";
                    else errLog += $"postputDataData: NULL\n";
                    errLog += $"exception: {request.Exception}\n";
                    Debug.LogError(errLog);

                    result.successful = false;
                }
                else {
                    string responseData = Encoding.Default.GetString(response.Data);
                    result = JsonConvert.DeserializeObject<OperationPostResult>(responseData);
                }
                requestReady = true;
            });
            httpRequest.AddHeader("Content-Type", "application/json");
            httpRequest.AddHeader("Authorization", $"Basic {svcCredentials}");
            httpRequest.RawData = jsonToSend;
            httpRequest.Send();

            while (!requestReady)
                await Task.Delay(50);
        }
        catch (Exception ex) {
            Debug.Log("Exception in Post: " + ex.Message + " /// " + ex.StackTrace);
        }
        return result;
    }

    public async Task<OperationPostResult> Del(string url, string bodyData)
    {
        OperationPostResult result = new OperationPostResult();

        string username = SessionData.Instance.ActiveUser.userId;
        string password = SessionData.Instance.ActiveUser.token;
        string svcCredentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));

        bool requestReady = false;

        HTTPRequest httpRequest = new HTTPRequest(new Uri(url), HTTPMethods.Delete, (HTTPRequest request, HTTPResponse response) =>
        {
            if (response == null)
            {
                string errLog = "Post_1\n";
                errLog += $"url: {url}\n";
                if (bodyData != null) errLog += $"postData: {bodyData}\n";
                else errLog += $"bodyData: NULL\n";
                errLog += $"exception: {request.Exception}\n";
                Debug.LogError(errLog);

                result.successful = false;
            }
            else
            {
                string responseData = Encoding.Default.GetString(response.Data);
                result = JsonConvert.DeserializeObject<OperationPostResult>(responseData);
            }
            requestReady = true;
        });
        httpRequest.AddHeader("Content-Type", "application/json");
        httpRequest.AddHeader("Authorization", $"Basic {svcCredentials}");
        httpRequest.Send();

        while (!requestReady)
            await Task.Delay(50);

        return result;
    }

}
