using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BingTranslator
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            Dictionary<string, string> languageDic = new Dictionary<string, string>()
            {
                {"zh-CHT","zh-CHT"},
                {"zh-CHS","zh-CHS"},
                {"en","en"},
            };

            cboLanguageFrom.DisplayMemberPath = cboLanguageTo.DisplayMemberPath = "Key";
            cboLanguageFrom.SelectedValuePath = cboLanguageTo.SelectedValuePath = "Value";
            cboLanguageFrom.ItemsSource = languageDic;
            cboLanguageTo.ItemsSource = languageDic;
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            BingTranslator();
        }

        private void BingTranslator()
        {
            AdmAccessToken admToken;
            string headerValue;
            //Get Client Id and Client Secret from https://datamarket.azure.com/developer/applications/
            //Refer obtaining AccessToken (http://msdn.microsoft.com/en-us/library/hh454950.aspx) 
            AdmAuthentication admAuth = new AdmAuthentication("KgsResourceHelper", "QUFdjQbYu17Lg4Aw/bTDvih2dIbnW/Heyz9b2m0GbZc=");
            try
            {
                admToken = admAuth.GetAccessToken();
                // Create a header with the access_token property of the returned token
                headerValue = "Bearer " + admToken.access_token;
                TranslateMethod(headerValue);
            }
            catch (WebException e)
            {
                ProcessWebException(e);
                //Console.WriteLine("Press any key to continue...");
                //Console.ReadKey(true);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                //Console.WriteLine("Press any key to continue...");
                //Console.ReadKey(true);
                MessageBox.Show(ex.Message);
            }
        }

        //private static void TranslateMethod(string authToken)
        private void TranslateMethod(string authToken)
        {
            //string text = "Use pixels to express measurements for padding and margins.";
            //string from = "en";
            //string to = "de";
            string text = txtInput.Text;
            string from = cboLanguageFrom.SelectedValue.ToString(); // zh-CHT   zh-CHS  en  de
            string to = cboLanguageTo.SelectedValue.ToString();

            string uri = "http://api.microsofttranslator.com/v2/Http.svc/Translate?text=" + System.Web.HttpUtility.UrlEncode(text) + "&from=" + from + "&to=" + to;

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add("Authorization", authToken);
            WebResponse response = null;
            try
            {
                response = httpWebRequest.GetResponse();
                using (Stream stream = response.GetResponseStream())
                {
                    System.Runtime.Serialization.DataContractSerializer dcs = new System.Runtime.Serialization.DataContractSerializer(Type.GetType("System.String"));
                    string translation = (string)dcs.ReadObject(stream);
                    //Console.WriteLine("Translation for source text '{0}' from {1} to {2} is", text, "en", "de");
                    //Console.WriteLine(translation);
                    //MessageBox.Show(string.Format("Translation for source text '{0}' from {1} to {2} is", text, "en", "de"));
                    //MessageBox.Show(translation);
                    txtOutput.Text = translation;

                }
                //Console.WriteLine("Press any key to continue...");
                //Console.ReadKey(true);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }
        private static void ProcessWebException(WebException e)
        {
            //Console.WriteLine("{0}", e.ToString());
            MessageBox.Show(string.Format("{0}", e.ToString()));

            // Obtain detailed error information
            string strResponse = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)e.Response)
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(responseStream, System.Text.Encoding.ASCII))
                    {
                        strResponse = sr.ReadToEnd();
                    }
                }
            }
            //Console.WriteLine("Http status code={0}, error message={1}", e.Status, strResponse);
            MessageBox.Show(string.Format("Http status code={0}, error message={1}", e.Status, strResponse));
        }

        [DataContract]
        public class AdmAccessToken
        {
            [DataMember]
            public string access_token { get; set; }
            [DataMember]
            public string token_type { get; set; }
            [DataMember]
            public string expires_in { get; set; }
            [DataMember]
            public string scope { get; set; }
        }

        public class AdmAuthentication
        {
            public static readonly string DatamarketAccessUri = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
            private string clientId;
            private string cientSecret;
            private string request;

            public AdmAuthentication(string clientId, string clientSecret)
            {
                this.clientId = clientId;
                this.cientSecret = clientSecret;
                //If clientid or client secret has special characters, encode before sending request
                this.request = string.Format("grant_type=client_credentials&client_id={0}&client_secret={1}&scope=http://api.microsofttranslator.com", HttpUtility.UrlEncode(clientId), HttpUtility.UrlEncode(clientSecret));
            }

            public AdmAccessToken GetAccessToken()
            {
                return HttpPost(DatamarketAccessUri, this.request);
            }

            private AdmAccessToken HttpPost(string DatamarketAccessUri, string requestDetails)
            {
                //Prepare OAuth request 
                WebRequest webRequest = WebRequest.Create(DatamarketAccessUri);
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.Method = "POST";
                byte[] bytes = Encoding.ASCII.GetBytes(requestDetails);
                webRequest.ContentLength = bytes.Length;
                using (Stream outputStream = webRequest.GetRequestStream())
                {
                    outputStream.Write(bytes, 0, bytes.Length);
                }
                using (WebResponse webResponse = webRequest.GetResponse())
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AdmAccessToken));
                    //Get deserialized object from JSON stream
                    AdmAccessToken token = (AdmAccessToken)serializer.ReadObject(webResponse.GetResponseStream());
                    return token;
                }
            }
        }
    }
}
