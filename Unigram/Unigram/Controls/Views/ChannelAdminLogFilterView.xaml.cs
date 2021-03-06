﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unigram.Views;
using Unigram.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using LinqToVisualTree;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Composition;
using System.Diagnostics;
using Windows.UI.ViewManagement;
using Windows.Foundation.Metadata;
using Windows.UI;
using Template10.Utils;
using Unigram.ViewModels.Channels;
using Unigram.ViewModels.Supergroups;

namespace Unigram.Controls.Views
{
    public sealed partial class ChannelAdminLogFilterView : ContentDialogBase
    {
        public SupergroupEventLogFilterViewModel ViewModel => DataContext as SupergroupEventLogFilterViewModel;

        private ChannelAdminLogFilterView()
        {
            InitializeComponent();
            DataContext = TLContainer.Current.Resolve<SupergroupEventLogFilterViewModel>();
        }

        private static ChannelAdminLogFilterView _current;
        public static ChannelAdminLogFilterView Current
        {
            get
            {
                if (_current == null)
                    _current = new ChannelAdminLogFilterView();

                return _current;
            }
        }

        public ItemClickEventHandler ItemClick { get; set; }

        //public IAsyncOperation<ContentDialogBaseResult> ShowAsync(TLStickerSet parameter)
        //{
        //    return ShowAsync(parameter, null);
        //}

        //public IAsyncOperation<ContentDialogBaseResult> ShowAsync(TLStickerSet parameter, ItemClickEventHandler callback)
        //{
        //    return ShowAsync(new TLInputStickerSetID { Id = parameter.Id, AccessHash = parameter.AccessHash }, callback);
        //}

        //public IAsyncOperation<ContentDialogBaseResult> ShowAsync(TLStickerSetCoveredBase parameter)
        //{
        //    return ShowAsync(parameter, null);
        //}

        //public IAsyncOperation<ContentDialogBaseResult> ShowAsync(TLStickerSetCoveredBase parameter, ItemClickEventHandler callback)
        //{
        //    return ShowAsync(new TLInputStickerSetID { Id = parameter.Set.Id, AccessHash = parameter.Set.AccessHash }, callback);
        //}

        //public IAsyncOperation<ContentDialogBaseResult> ShowAsync(TLInputStickerSetBase parameter)
        //{
        //    return ShowAsync(parameter, null);
        //}

        public IAsyncOperation<ContentDialogBaseResult> ShowAsync(object parameter)
        {
            //ViewModel.IsLoading = true;
            //ViewModel.StickerSet = new TLStickerSet();
            //ViewModel.Items.Clear();

            RoutedEventHandler handler = null;
            handler = new RoutedEventHandler(async (s, args) =>
            {
                Loaded -= handler;
                await ViewModel.OnNavigatedToAsync(parameter, NavigationMode.New, null);
            });

            Loaded += handler;
            return ShowAsync();
        }

        //public IAsyncOperation<ContentDialogBaseResult> ShowAsync(TLMessagesStickerSet parameter)
        //{
        //    return ShowAsync(parameter, null);
        //}

        //public IAsyncOperation<ContentDialogBaseResult> ShowAsync(TLMessagesStickerSet parameter, ItemClickEventHandler callback)
        //{
        //    //ViewModel.IsLoading = false;
        //    //ViewModel.StickerSet = parameter.Set;
        //    //ViewModel.Items.Clear();
        //    //ViewModel.Items.Add(parameter);

        //    return ShowAsync();
        //}

        private Border LineTop;
        private Border LineAccent;

        private ScrollViewer _scrollingHost;

        private SpriteVisual _backgroundVisual;
        private ExpressionAnimation _expression;
        private ExpressionAnimation _expressionClip;

        private void GridView_Loaded(object sender, RoutedEventArgs e)
        {
            var scroll = List.Descendants<ScrollViewer>().FirstOrDefault() as ScrollViewer;
            if (scroll != null)
            {
                _scrollingHost = scroll;
                _scrollingHost.ChangeView(null, 0, null, true);
                scroll.ViewChanged += Scroll_ViewChanged;
                Scroll_ViewChanged(scroll, null);

                var brush = App.Current.Resources["SystemControlBackgroundChromeMediumLowBrush"] as SolidColorBrush;
                var props = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scroll);

                if (_backgroundVisual == null)
                {
                    _backgroundVisual = ElementCompositionPreview.GetElementVisual(BackgroundPanel).Compositor.CreateSpriteVisual();
                    ElementCompositionPreview.SetElementChildVisual(BackgroundPanel, _backgroundVisual);
                }

                _backgroundVisual.Brush = _backgroundVisual.Compositor.CreateColorBrush(brush.Color);
                _backgroundVisual.Size = new System.Numerics.Vector2((float)BackgroundPanel.ActualWidth, (float)BackgroundPanel.ActualHeight);
                _backgroundVisual.Clip = _backgroundVisual.Compositor.CreateInsetClip();

                _expression = _expression ?? _backgroundVisual.Compositor.CreateExpressionAnimation("Max(Maximum, Scrolling.Translation.Y)");
                _expression.SetReferenceParameter("Scrolling", props);
                _expression.SetScalarParameter("Maximum", -(float)BackgroundPanel.Margin.Top + 1);
                _backgroundVisual.StopAnimation("Offset.Y");
                _backgroundVisual.StartAnimation("Offset.Y", _expression);

                _expressionClip = _expressionClip ?? _backgroundVisual.Compositor.CreateExpressionAnimation("Min(0, Maximum - Scrolling.Translation.Y)");
                _expressionClip.SetReferenceParameter("Scrolling", props);
                _expressionClip.SetScalarParameter("Maximum", -(float)BackgroundPanel.Margin.Top + 1);
                _backgroundVisual.Clip.StopAnimation("Offset.Y");
                _backgroundVisual.Clip.StartAnimation("Offset.Y", _expressionClip);
            }

            var panel = List.ItemsPanelRoot as ItemsWrapGrid;
            if (panel != null)
            {
                panel.SizeChanged += (s, args) =>
                {
                    Scroll_ViewChanged(scroll, null);
                };
            }
        }

        private void GroupHeader_Loaded(object sender, RoutedEventArgs e)
        {
            var groupHeader = sender as Grid;
            if (groupHeader != null)
            {
                LineTop = groupHeader.FindName("LineTop") as Border;
                LineAccent = groupHeader.FindName("LineAccent") as Border;

                if (_scrollingHost != null)
                {
                    Scroll_ViewChanged(_scrollingHost, null);
                }
            }
        }

        private void Scroll_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scroll = sender as ScrollViewer;
            var top = 1;
            var accent = 0;
            var bottom = 1;

            if (scroll.VerticalOffset <= BackgroundPanel.Margin.Top)
            {
                top = 0;
            }
            if (scroll.VerticalOffset < BackgroundPanel.Margin.Top)
            {
                accent = 1;
            }
            if (scroll.VerticalOffset == scroll.ScrollableHeight)
            {
                bottom = 0;
            }

            //if (LineTop.BorderThickness.Bottom != top)
            //{
            //    if (top == 0)
            //    {
            //        MaskTitleAndStatusBar();
            //    }
            //    else
            //    {
            //        SetupTitleAndStatusBar();
            //    }
            //}

            if (LineTop != null)
            {
                LineTop.BorderThickness = new Thickness(0, 0, 0, top);
                LineAccent.BorderThickness = new Thickness(0, accent, 0, 0);
                LineBottom.BorderThickness = new Thickness(0, bottom, 0, 0);
            }
        }

        // SystemControlBackgroundChromeMediumLowBrush

        private void SetupTitleAndStatusBar()
        {
            var titlebar = ApplicationView.GetForCurrentView().TitleBar;
            var backgroundBrush = Application.Current.Resources["SystemControlBackgroundChromeMediumLowBrush"] as SolidColorBrush;
            var foregroundBrush = Application.Current.Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;

            titlebar.BackgroundColor = backgroundBrush.Color;
            titlebar.ForegroundColor = foregroundBrush.Color;
            titlebar.ButtonBackgroundColor = backgroundBrush.Color;
            titlebar.ButtonForegroundColor = foregroundBrush.Color;

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = backgroundBrush.Color;
                statusBar.ForegroundColor = foregroundBrush.Color;
            }
        }

        private void List_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var itemWidth = (e.NewSize.Width - 24) / 5d;
            var minHeigth = itemWidth * 3d - 12 + 48;
            var top = Math.Max(0, e.NewSize.Height - minHeigth);

            if (!IsFullScreenMode())
            {
                top = 0;
            }

            if (top == 0)
            {
                Header.Visibility = Visibility.Collapsed;
            }
            else
            {
                Header.Visibility = Visibility.Visible;
            }

            Header.Height = top;

            BackgroundPanel.Height = e.NewSize.Height;
            BackgroundPanel.Margin = new Thickness(0, top, 0, -top);

            if (_backgroundVisual != null && _expression != null && _expressionClip != null)
            {
                var brush = App.Current.Resources["SystemControlBackgroundChromeMediumLowBrush"] as SolidColorBrush;

                _backgroundVisual.Brush = _backgroundVisual.Compositor.CreateColorBrush(brush.Color);
                _backgroundVisual.Size = new System.Numerics.Vector2((float)e.NewSize.Width, (float)e.NewSize.Height);
                _backgroundVisual.Clip = _backgroundVisual.Compositor.CreateInsetClip();

                _expression.SetScalarParameter("Maximum", -(float)top + 1);
                _backgroundVisual.StopAnimation("Offset.Y");
                _backgroundVisual.StartAnimation("Offset.Y", _expression);

                _expressionClip.SetScalarParameter("Maximum", -(float)top + 1);
                _backgroundVisual.Clip.StopAnimation("Offset.Y");
                _backgroundVisual.Clip.StartAnimation("Offset.Y", _expressionClip);
            }
        }

        //protected override void UpdateView(Rect bounds)
        //{
        //    if (BackgroundElement == null) return;

        //    BackgroundElement.MinHeight = bounds.Height;
        //    BackgroundElement.BorderThickness = new Thickness(0);
        //}

        private void LightDismiss_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Hide(ContentDialogBaseResult.None);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Hide(ContentDialogBaseResult.Cancel);
        }

        private void List_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ItemClick != null)
            {
                ItemClick.Invoke(this, e);
                Hide(ContentDialogBaseResult.OK);
            }
        }
    }
}
