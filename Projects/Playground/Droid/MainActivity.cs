using Android.App;
using Android.Widget;
using Android.OS;
using Playground.Core;

namespace Playground.Droid
{
    [Activity(Label = "Playground", MainLauncher = true, Icon = "@mipmap/icon")]
    public class MainActivity : Activity
    {
        private MainViewModel ViewModel { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            ViewModel = new MainViewModel();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.myButton);

            button.Click += delegate { ViewModel.SendRequestCommand.Execute(null); };
        }
    }
}

