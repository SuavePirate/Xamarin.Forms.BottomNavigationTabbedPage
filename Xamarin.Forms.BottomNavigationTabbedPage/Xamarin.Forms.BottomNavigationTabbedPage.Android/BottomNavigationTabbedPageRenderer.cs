using System;
using Xamarin.Forms.Platform.Android.AppCompat;
using Xamarin.Forms;
using Xamarin.Forms.BottomNavigationTabbedPage;
using Xamarin.Forms.BottomNavigationTabbedPage.Android;
using Xamarin.Forms.Platform.Android;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Android.Content;
using System.Linq;

[assembly: ExportRenderer(typeof(BottomNavigationTabbedPage), typeof(BottomNavigationTabbedPageRenderer))]
namespace Xamarin.Forms.BottomNavigationTabbedPage.Android
{
    public class BottomNavigationTabbedPageRenderer : VisualElementRenderer<BottomNavigationTabbedPage>, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        BottomNavigationView _bottomNavigationView;
        IPageController _pageController;
        FrameLayout _frameLayout;
        ObservableCollection<IMenuItem> _menuItems;

        public static void Initialize()
        {

        }

        public BottomNavigationTabbedPageRenderer()
        {
            AutoPackage = false;
            _menuItems = new ObservableCollection<IMenuItem>();
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            _pageController.SendAppearing();
        }

        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();
            _pageController.SendDisappearing();
        }

        protected virtual void SwitchContent(Page view)
        {
            Context.HideKeyboard(this);

            _frameLayout.RemoveAllViews();

            if (view == null)
            {
                return;
            }

            if (Platform.Android.Platform.GetRenderer(view) == null)
            {
                Platform.Android.Platform.SetRenderer(view, Platform.Android.Platform.CreateRenderer(view));
            }

            _frameLayout.AddView(Platform.Android.Platform.GetRenderer(view).ViewGroup);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == nameof(TabbedPage.CurrentPage))
            {
                SwitchContent(Element.CurrentPage);
                UpdateSelectedTabIndex(Element.CurrentPage);
            }

        }

        void UpdateSelectedTabIndex(Page page)
        {
            var index = Element.Children.IndexOf(page);
            _bottomNavigationView.SelectedItemId = _menuItems[index].ItemId;
        }


        void SetTabItems()
        {
            _bottomNavigationView.InflateMenu(Resource.Menu.bottom_navigation_menu);
            _menuItems.Clear();
            foreach(var page in Element.Children)
            {
                var tab = _bottomNavigationView.Menu.Add(page.Title);

                tab.SetIcon(ResourceManager.GetDrawable(Context.Resources, page.Icon));
                _menuItems.Add(tab);
            }
        }


        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            int width = r - l;
            int height = b - t;

            var context = Context;

            _bottomNavigationView.Measure(MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly), MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.AtMost));
            int tabsHeight = Math.Min(height, Math.Max(_bottomNavigationView.MeasuredHeight, _bottomNavigationView.MinimumHeight));

            if (width > 0 && height > 0)
            {
                _pageController.ContainerArea = new Rectangle(0, 0, context.FromPixels(width), context.FromPixels(_frameLayout.Height));
                ObservableCollection<Element> internalChildren = _pageController.InternalChildren;

                for (var i = 0; i < internalChildren.Count; i++)
                {
                    var child = internalChildren[i] as VisualElement;

                    if (child == null)
                    {
                        continue;
                    }

                    NavigationPageRenderer navigationRenderer;
                    using (IVisualElementRenderer renderer = Platform.Android.Platform.GetRenderer(child))
                    {
                        navigationRenderer = renderer as NavigationPageRenderer;
                    }

                    if (navigationRenderer != null)
                    {
                        // navigationRenderer.ContainerPadding = tabsHeight;
                    }
                }

                _bottomNavigationView.Measure(MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly), MeasureSpec.MakeMeasureSpec(tabsHeight, MeasureSpecMode.Exactly));
                _bottomNavigationView.Layout(0, 0, width, tabsHeight);
            }

            base.OnLayout(changed, l, t, r, b);
        }

        protected override void OnElementChanged(ElementChangedEventArgs<BottomNavigationTabbedPage> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {

                var bottomBarPage = e.NewElement;

                if (_bottomNavigationView == null)
                {
                    _pageController = PageController.Create(bottomBarPage);

                    // create a view which will act as container for Page's
                    _frameLayout = new FrameLayout(Forms.Context);
                    _frameLayout.LayoutParameters = new FrameLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent, GravityFlags.Fill);
                    AddView(_frameLayout, 0);

                    // create bottomBar control
                    _bottomNavigationView = new BottomNavigationView(Context);
                    AddView(_bottomNavigationView, 1);
                  
                    _bottomNavigationView.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
                    _bottomNavigationView.SetOnNavigationItemSelectedListener(this);

                    SetTabItems();
                }

                if (bottomBarPage.CurrentPage != null)
                {
                    SwitchContent(bottomBarPage.CurrentPage);
                }
            }
        }


        public bool OnNavigationItemSelected(IMenuItem item)
        {
            var position = _menuItems.IndexOf(_menuItems.FirstOrDefault(i => i.ItemId == item.ItemId));
            SwitchContent(Element.Children[position]);
            var bottomBarPage = Element as BottomNavigationTabbedPage;
            bottomBarPage.CurrentPage = Element.Children[position];
            return true;
        }
    }
}
