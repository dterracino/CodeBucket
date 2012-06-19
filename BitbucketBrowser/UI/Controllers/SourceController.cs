using System;
using MonoTouch.Dialog;
using BitbucketSharp.Models;
using System.Collections.Generic;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using System.Threading;
using RedPlum;


namespace BitbucketBrowser.UI
{
    public class SourceController : Controller<SourceModel>
    {
        public string Username { get; private set; }

        public string Slug { get; private set; }

        public string Branch { get; private set; }

        public string Path { get; private set; }

        public SourceController(string username, string slug, string branch = "master", string path = "")
            : base(true, false)
        {
            Style = MonoTouch.UIKit.UITableViewStyle.Plain;
            Username = username;
            Slug = slug;
            Branch = branch;
            Path = path;
            Root.Add(new Section());

            if (string.IsNullOrEmpty(path))
                Title = "Source";
            else
            {
                Title = path.Substring(path.LastIndexOf('/') + 1);
            }
        }


        protected override void OnRefresh ()
        {
            var items = new List<Element>(Model.Files.Count + Model.Directories.Count);
            Model.Directories.ForEach(d => 
            {
                items.Add(new ItemElement(d, () => NavigationController.PushViewController(new SourceController(Username, Slug, Branch, Path + "/" + d), true),
                                                 UIImage.FromBundle("/Images/folder.png"))
                          { Accessory = MonoTouch.UIKit.UITableViewCellAccessory.DisclosureIndicator,  });
            });

            Model.Files.ForEach(f =>
            {
                var i = f.Path.LastIndexOf('/') + 1;
                var p = f.Path.Substring(i);
                items.Add(new ItemElement(p,() => NavigationController.PushViewController(
                                          new SourceInfoController(Username, Slug, Branch, f.Path) { Title = p}, true), 
                                          UIImage.FromBundle("/Images/file.png")));
            });


            InvokeOnMainThread(delegate {
                Root[0].Clear();
                Root[0].AddAll(items);
            });
        }

        protected override SourceModel OnUpdate ()
        {
            var client = new BitbucketSharp.Client("thedillonb", "djames");
            return client.Users[Username].Repositories[Slug].Branches[Branch].Source[Path].GetInfo();
        }

        public class ItemElement : ImageStringElement
        {
            public ItemElement(string cap, NSAction act, UIImage img)
                : base(cap, act, img)
            {
            }

            public override UITableViewCell GetCell(UITableView tv)
            {
                var cell = base.GetCell(tv);
                cell.TextLabel.Font = UIFont.BoldSystemFontOfSize(15f);
                return cell;
            }
        }
    }

    public class SourceInfoController : UIViewController
    {
        private UIWebView _web;

        private string _user, _slug, _branch, _path;


        public SourceInfoController(string user, string slug, string branch, string path)
            : base()
        {
            _user = user;
            _slug = slug;
            _branch = branch;
            _path = path;

            _web = new UIWebView();
            _web.DataDetectorTypes = UIDataDetectorType.None;
            this.Add(_web);

            Title = path.Substring(path.LastIndexOf('/') + 1);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Request();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            _web.Frame = this.View.Bounds;
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);

            var bounds = View.Bounds;
            _web.Frame = bounds;
        }

        private void Request()
        {
            var hud = new MBProgressHUD(this.View); 
            hud.Mode = MBProgressHUDMode.Indeterminate;
            hud.TitleText = "Loading...";
            this.View.AddSubview(hud);
            hud.Show(true);

            ThreadPool.QueueUserWorkItem(delegate {
                var c = new BitbucketSharp.Client("thedillonb", "djames");
                var d = c.Users[_user].Repositories[_slug].Branches[_branch].Source.GetFile(_path);
                var data = System.Security.SecurityElement.Escape(d.Data);

                InvokeOnMainThread(delegate {
                    _web.LoadHtmlString("<pre>" + data + "</pre>", null);
                    hud.Hide(true);
                    hud.RemoveFromSuperview();
                });
            });
        }


    }
}
