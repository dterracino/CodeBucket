using System;
using UIKit;
using CodeBucket.ViewControllers;
using CodeBucket.Core.ViewModels.App;
using Foundation;
using SDWebImage;
using CodeBucket.Core.Utils;

namespace CodeBucket.Views
{
    public class StartupView : ViewModelDrivenDialogViewController
    {
        const float imageSize = 128f;

        public static UIColor TextColor = UIColor.FromWhiteAlpha(0.34f, 1f);
        public static UIColor SpinnerColor = UIColor.FromWhiteAlpha(0.33f, 1f);

        private UIImageView _imgView;
        private UILabel _statusLabel;
        private UIActivityIndicatorView _activityView;

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();

            _imgView.Frame = new CoreGraphics.CGRect(View.Bounds.Width / 2 - imageSize / 2, View.Bounds.Height / 2 - imageSize / 2 - 30f, imageSize, imageSize);
            _statusLabel.Frame = new CoreGraphics.CGRect(0, _imgView.Frame.Bottom + 10f, View.Bounds.Width, 15f);
            _activityView.Center = new CoreGraphics.CGPoint(View.Bounds.Width / 2, _statusLabel.Frame.Bottom + 16f + 16F);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.AutosizesSubviews = true;

            _imgView = new UIImageView();
            _imgView.Layer.CornerRadius = imageSize / 2;
            _imgView.Layer.MasksToBounds = true;
            _imgView.Image = Images.Avatar;
            _imgView.TintColor = TextColor;
            _imgView.Layer.BorderWidth = 2f;
            _imgView.Layer.BorderColor = UIColor.White.CGColor;
            Add(_imgView);

            _statusLabel = new UILabel();
            _statusLabel.TextAlignment = UITextAlignment.Center;
            _statusLabel.Font = UIFont.FromName("HelveticaNeue", 13f);
            _statusLabel.TextColor = TextColor;
            Add(_statusLabel);

            _activityView = new UIActivityIndicatorView() { HidesWhenStopped = true };
            _activityView.Color = SpinnerColor;
            Add(_activityView);

            View.BackgroundColor = UIColor.FromRGB(51, 88, 162);
           
			var vm = (StartupViewModel)ViewModel;
			vm.Bind(x => x.IsLoggingIn, x =>
			{
				if (x)
				{
                    _activityView.StartAnimating();
				}
				else
				{
                    _activityView.StopAnimating();
				}
			});

            vm.Bind(x => x.Avatar, UpdatedImage);
            vm.Bind(x => x.Status, x => _statusLabel.Text = x);

        }

        public void UpdatedImage(Avatar avatar)
        {
            var avatarUrl = avatar?.ToUrl(Convert.ToInt32(imageSize));
            if (avatarUrl == null) return;
            var placeholder = Images.Avatar;
            _imgView.SetImage(new NSUrl(avatarUrl), placeholder, 0, (img, err, cache, type) => {
                UIView.Transition(_imgView, 0.50f, UIViewAnimationOptions.TransitionCrossDissolve, () => _imgView.Image = img, null);
            });
        }

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			var vm = (StartupViewModel)ViewModel;
			vm.StartupCommand.Execute(null);
		}

        public override bool ShouldAutorotate()
        {
            return true;
        }

        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.Default;
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone)
                return UIInterfaceOrientationMask.Portrait | UIInterfaceOrientationMask.PortraitUpsideDown;
            return UIInterfaceOrientationMask.All;
        }

        /// <summary>
        /// A custom navigation controller specifically for iOS6 that locks the orientations to what the StartupControler's is.
        /// </summary>
        protected class CustomNavigationController : UINavigationController
        {
            readonly StartupView _parent;
            public CustomNavigationController(StartupView parent, UIViewController root) : base(root) 
            { 
                _parent = parent;
            }

            public override bool ShouldAutorotate()
            {
                return _parent.ShouldAutorotate();
            }

            public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
            {
                return _parent.GetSupportedInterfaceOrientations();
            }
        }
    }
}

