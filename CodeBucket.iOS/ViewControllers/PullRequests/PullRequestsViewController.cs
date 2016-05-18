using CodeBucket.Core.ViewModels.PullRequests;
using UIKit;
using CodeBucket.DialogElements;
using CodeBucket.TableViewCells;
using System;
using CodeBucket.Views;
using ReactiveUI;
using CodeBucket.TableViewSources;

namespace CodeBucket.ViewControllers.PullRequests
{
    public class PullRequestsViewController : BaseTableViewController<PullRequestsViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new PullRequestTableViewSource(TableView, ViewModel.Items);
            TableView.Source = source;
            TableView.EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(AtlassianIcon.Devtoolspullrequest.ToImage(64f), "There are no pull requests."));

            var viewSegment = new UISegmentedControl(new object[] { "Open", "Merged", "Declined" });
            NavigationItem.TitleView = viewSegment;

            OnActivation(disposable =>
            {
                viewSegment.GetChangedObservable()
                    .Subscribe(x => ViewModel.SelectedFilter = (BitbucketSharp.Models.V2.PullRequestState)x)
                    .AddTo(disposable);

                ViewModel.WhenAnyValue(x => x.SelectedFilter)
                    .Subscribe(x => viewSegment.SelectedSegment = (int)x)
                    .AddTo(disposable);

                this.WhenAnyValue(x => x.ViewModel.IsEmpty)
                    .Subscribe(x => TableView.IsEmpty = x)
                    .AddTo(disposable);
            });
        }
    }
}
