using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfGui.Framework
{
    internal class ImageButton:Button
    {
        public static readonly RoutedEvent CheckedChangedEvent = EventManager.RegisterRoutedEvent("CheckedChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler<CheckedChangedRoutedEventArgs>), typeof(ImageButton));

        #region -> Nested Fields <-

        private Brush _originalBackground;

        protected Image ImageControl
        {
            get;
            private set;
        }

        #endregion

        #region -> Interface <-

        [Category("Property Changed")]
        public event RoutedEventHandler CheckedChanged
        {
            add { AddHandler(CheckedChangedEvent, value); }
            remove { RemoveHandler(CheckedChangedEvent, value); }
        }

        public Brush ClickedBackground
        {
            get;
            set;
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                bool originalIsChecked = _isChecked;
                _isChecked = value;
                if (originalIsChecked != _isChecked)
                {
                    InvalidateVisual();
                    RaiseEvent(new CheckedChangedRoutedEventArgs(CheckedChangedEvent, IsChecked));
                }
            }
        }
        public bool IsCanBeChecked
        {
            get;
            set;
        }
        private bool _isBehaveAsRadioButton;
        public bool IsBehaveAsRadioButton
        {
            get { return _isBehaveAsRadioButton; }
            set
            {
                _isBehaveAsRadioButton = value;
                IsCanBeChecked = value;
            }
        }
        public ImageSource Image
        {
            get { return ImageControl.Source; }
            set { ImageControl.Source = value; }
        }
        public Stretch ImageStretch
        {
            get { return ImageControl.Stretch; }
            set { ImageControl.Stretch = value; }
        }

        #endregion

        public ImageButton()
        {
            this.IsEnabledChanged += ImageButton_IsEnabledChanged;
            
            DockPanel dockPanel = new DockPanel();
            dockPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            dockPanel.VerticalAlignment = VerticalAlignment.Stretch;
            ImageControl = new Image();
            ImageControl.Stretch = Stretch.Uniform;
            dockPanel.Children.Add(ImageControl);
            /*Binding imageWidthBinding = new Binding("ActualWidth");
            imageWidthBinding.Source = ImageControl;
            dockPanel.SetBinding(DockPanel.WidthProperty, imageWidthBinding);
            Binding imageHeightBinding = new Binding("ActualHeight");
            imageHeightBinding.Source = ImageControl;
            dockPanel.SetBinding(DockPanel.HeightProperty, imageHeightBinding);*/

            this.Content = dockPanel;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            _originalBackground = this.Background;
            this.Background = this.ClickedBackground;
        }
        protected override void OnClick()
        {
            base.OnClick();
            if(IsCanBeChecked)
            {
                bool newIsChecked = !IsChecked;
                if (newIsChecked)
                {
                    if (this.Background != ClickedBackground)
                    {
                        _originalBackground = Background;
                        this.Background = ClickedBackground;
                    }
                    if (IsBehaveAsRadioButton)
                    {
                        //Uncheck all other buttons with IsBehaveAsRadioButton==true of the same container
                        DependencyObject parent= VisualTreeHelper.GetParent(this);
                        foreach(ImageButton radioButton in (parent as Panel).Children.OfType<ImageButton>().Where(b=>b.IsBehaveAsRadioButton))
                        {
                            if(radioButton!=this) radioButton.IsChecked = false;
                        }
                    }
                }
                else
                {
                    this.Background = _originalBackground;
                }
                IsChecked = newIsChecked;
            }
            else
            {
                this.Background = _originalBackground;
            }
        }

        private void ImageButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false) ImageControl.Opacity = 50;
            else ImageControl.Opacity = 100;
        }
    }
}
