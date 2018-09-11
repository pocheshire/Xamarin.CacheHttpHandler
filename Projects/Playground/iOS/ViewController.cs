using System;
using Playground.Core;
using UIKit;

namespace Playground.iOS
{
    public partial class ViewController : UIViewController
    {
        private MainViewModel ViewModel { get; set; }

        public ViewController(IntPtr handle) 
            : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            ViewModel = new MainViewModel();

            // Perform any additional setup after loading the view, typically from a nib.
            Button.AccessibilityIdentifier = "myButton";
            Button.TouchUpInside += delegate
            {
                ViewModel.SendRequestCommand.Execute(null);
            };
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.		
        }
    }
}
