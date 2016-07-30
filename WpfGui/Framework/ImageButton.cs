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
        #region -> Nested Fields <-

        private bool _originalBackgroundSet = false;
        private Brush _originalBackground;
        //private Brush _previousBackground;

        protected Image ImageControl
        {
            get;
            private set;
        }

        #endregion

        #region -> Interface <-

        public static readonly RoutedEvent CheckedChangedEvent = EventManager.RegisterRoutedEvent("CheckedChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler<CheckedChangedRoutedEventArgs>), typeof(ImageButton));
        [Category("Property Changed")]
        public event RoutedEventHandler<CheckedChangedRoutedEventArgs> CheckedChanged
        {
            add { AddHandler(CheckedChangedEvent, value); }
            remove { RemoveHandler(CheckedChangedEvent, value); }
        }

        public static readonly DependencyProperty MouseHoverBackgroundProperty = DependencyProperty.Register("MouseHoverBackground", typeof(Brush), typeof(ImageButton));
        public Brush MouseHoverBackground
        {
            get { return (Brush)GetValue(MouseHoverBackgroundProperty); }
            set
            {
                SetValue(MouseHoverBackgroundProperty, value);
                (this.Content as Panel).Background = value;
            }
        }
        public static readonly DependencyProperty ClickedBackgroundProperty = DependencyProperty.Register("ClickedBackground", typeof(Brush), typeof(ImageButton));
        public Brush ClickedBackground
        {
            get { return (Brush)GetValue(ClickedBackgroundProperty); }
            set
            {
                SetValue(ClickedBackgroundProperty, value);
                (this.Content as Panel).Background = value;
            }
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
                    if(!_isChecked) RevertBackground();
                    InvalidateVisual();
                    RaiseEvent(new CheckedChangedRoutedEventArgs(CheckedChangedEvent, IsChecked));
                }
            }
        }
        private bool _isCanBeChecked;
        public bool IsCanBeChecked
        {
            get { return _isCanBeChecked; }
            set
            {
                bool originalValue = value;
                _isCanBeChecked = value;
                if (originalValue && !_isCanBeChecked) IsChecked = false;
            }
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

        #region -> Constructors <-

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

            Binding backgroundBinding = new Binding("Background");
            backgroundBinding.Source = this;
            dockPanel.SetBinding(DockPanel.BackgroundProperty, backgroundBinding);

            this.Content = dockPanel;
        }

        #endregion

        private void TemporaryUpdateBackground(Brush UpdatedBackground)
        {
            if (!_originalBackgroundSet)
            {
                _originalBackground = Background;
                _originalBackgroundSet = true;
            }
            //_previousBackground = this.Background;
            this.Background = UpdatedBackground;
        }
        private void RevertBackground()
        {
            /*if (this.Background == _previousBackground)
            {
                this.Background = _originalBackground;
                _originalBackground = null;
            }
            else
            {
                this.Background = _previousBackground;
            }*/
            if (!IsChecked)
            {
                this.Background = _originalBackground;
                _originalBackgroundSet = false;
                _originalBackground = null;
            }
            else
            {
                this.Background = ClickedBackground;
            }
        }
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            if(!IsChecked)
            {
                TemporaryUpdateBackground(MouseHoverBackground);
            }
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if(!IsChecked)
            {
                RevertBackground();
            }
        }
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            TemporaryUpdateBackground(ClickedBackground);
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
                        TemporaryUpdateBackground(ClickedBackground);
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
                    RevertBackground();
                }
                IsChecked = newIsChecked;
            }
            else
            {
                RevertBackground();
            }
        }

        private void ImageButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false) ImageControl.Opacity = 50;
            else ImageControl.Opacity = 100;
        }
    }
}
