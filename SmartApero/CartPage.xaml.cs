using Cdiscount.OpenApi.ProxyClient.Contract.Search;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SmartApero
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CartPage : Page
    {
        List<Question> _questions;

        public CartPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                var questions = e.Parameter as List<Question>;

                if (questions != null)
                {
                    _questions = questions;
                    LoadProducts();
                }
            }
        }

        private async void LoadProducts()
        {
            Cdiscount.OpenApi.ProxyClient.Config.ProxyClientConfig config = new Cdiscount.OpenApi.ProxyClient.Config.ProxyClientConfig { ApiKey = "e62a3122-2d61-462a-bef4-7403a408b5eb" };
            Cdiscount.OpenApi.ProxyClient.OpenApiClient client = new Cdiscount.OpenApi.ProxyClient.OpenApiClient(config);

            SearchRequest request = new SearchRequest();

            if (_questions.Single(e => e.Key == "child").Value.ToString() == "oui")
            {
                request.Keyword = "fraise tagada";
            }

            var response = client.Search(request);
        }
    }
}
