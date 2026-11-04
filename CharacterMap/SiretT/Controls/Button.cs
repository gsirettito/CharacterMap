using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

namespace SiretT.Controls {
    public class Button : ContentControl, ICommandSource {
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(object), typeof(Button), new PropertyMetadata(OnPropertyChange));

        public static readonly DependencyProperty IconDockProperty = DependencyProperty.Register(
            "IconDock", typeof(Dock), typeof(Button), new PropertyMetadata(OnPropertyChange));

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(object), typeof(Button), new PropertyMetadata(OnPropertyChange));

        public static readonly RoutedEvent ClickEvent;
        private bool _isMouseDown;
        private Point _mouseDownPosition;
        private const double MaxMoveTolerance = 5.0;

        [Category("Behavior")]
        public event RoutedEventHandler Click;

        public Button() {
            MouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
            MouseLeftButtonUp += Button_PreviewMouseLeftButtonUp;
            MouseMove += Button_PreviewMouseMove;
            MouseLeave += Button_PreviewMouseLeave;
        }

        private void Button_PreviewMouseMove(object sender, MouseEventArgs e) {
            if (_isMouseDown && e.LeftButton == MouseButtonState.Released)
                _isMouseDown = false;
        }

        private void Button_PreviewMouseLeave(object sender, MouseEventArgs e) {
            _isMouseDown = false;
        }

        private void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (_isMouseDown) {
                var currentPosition = e.GetPosition(this);
                double distance = (_mouseDownPosition - currentPosition).Length;
                if (distance <= MaxMoveTolerance)
                    Click?.Invoke(this, new RoutedEventArgs());
                _isMouseDown = false;
            }
            e.Handled = true;
        }

        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _isMouseDown = true;
            _mouseDownPosition = e.GetPosition(this);
            e.Handled = true;
        }

        private static void OnPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var self = d as Button;
        }

        [Category("Common Properties")]
        public object Icon {
            get {
                return GetValue(IconProperty);
            }
            set { SetValue(IconProperty, value); }
        }

        [Category("Common Properties")]
        public Dock IconDock {
            get {
                return (Dock)GetValue(IconDockProperty);
            }
            set { SetValue(IconDockProperty, value); }
        }

        [Category("Common Properties")]
        public object Header {
            get {
                return GetValue(HeaderProperty);
            }
            set { SetValue(HeaderProperty, value); }
        }

        public ICommand Command => throw new NotImplementedException();

        public object CommandParameter => throw new NotImplementedException();

        public IInputElement CommandTarget => throw new NotImplementedException();

    }
}