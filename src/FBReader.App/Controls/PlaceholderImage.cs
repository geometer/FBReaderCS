/*
 * Author: CactusSoft (http://cactussoft.biz/), 2013
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
 * 02110-1301, USA.
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace FBReader.App.Controls
{
    public class PlaceholderImage : Control
    {
        private bool _isImageLoaded;

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
            "ImageSource",
            typeof(ImageSource),
            typeof(PlaceholderImage),
            new PropertyMetadata(null, PropertyChangedCallback));

        public static readonly DependencyProperty PlaceholderSourceProperty =
                DependencyProperty.Register("PlaceholderSource", typeof(ImageSource), typeof(PlaceholderImage), null);

        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
            "Stretch", typeof(Stretch), typeof(PlaceholderImage), new PropertyMetadata(Stretch.Uniform));

        public static readonly DependencyProperty IsFrozenProperty = DependencyProperty.Register(
            "IsFrozen", typeof(bool), typeof(PlaceholderImage), new PropertyMetadata(false));

        private Image _image;

        private Image _placeholder;

        private Storyboard _animation;

        private static void PropertyChangedCallback(
            DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            
            
            var oldBmp = (BitmapImage)e.OldValue;
            var newBmp = (BitmapImage)e.NewValue;
            bool anyImageIsNull = oldBmp == null || newBmp == null;
            bool equalsUriSources = !anyImageIsNull && (oldBmp.UriSource != newBmp.UriSource);
            bool anyUriSourceIsNull = !anyImageIsNull && (string.IsNullOrEmpty(oldBmp.UriSource.ToString()) || string.IsNullOrEmpty(newBmp.UriSource.ToString()));
            if (anyImageIsNull || equalsUriSources || anyUriSourceIsNull)
            {
                // Optimization: Reset only if UI control is reused for another image source.

                var imageCtrl = (PlaceholderImage)dependencyObject;
                imageCtrl.Reset();
            }

            if (newBmp != null && string.IsNullOrEmpty(newBmp.UriSource.ToString()))
            {
                var imageCtrl = (PlaceholderImage)dependencyObject;
                imageCtrl.OnImageOpened(imageCtrl, null);
            }
        }

        public PlaceholderImage()
        {
            DefaultStyleKey = typeof(PlaceholderImage);

            Loaded += PlaceholderImage_Loaded;
        }

        private void PlaceholderImage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_isImageLoaded)
            {
                if (ImageSource != null)
                {
                    var uriSource = (ImageSource as BitmapImage).UriSource;
                    if (string.IsNullOrEmpty(uriSource.ToString()))
                    {
                        _image.Source = ImageSource;
                        OnImageOpened(this, null);
                        return;
                    }

                    _image.Source = new BitmapImage(uriSource)
                                        {
                                            CreateOptions = BitmapCreateOptions.DelayCreation
                                        };
                }
            }
        }


    public ImageSource ImageSource
        {
            get { return (ImageSource) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public ImageSource PlaceholderSource
        {
            get { return (ImageSource) GetValue(PlaceholderSourceProperty); }
            set { SetValue(PlaceholderSourceProperty, value); }
        }

        public Stretch Stretch
        {
            get { return (Stretch) GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public bool IsFrozen
        {
            get { return (bool) GetValue(IsFrozenProperty); }
            set { SetValue(IsFrozenProperty, value); }
        }

        public event EventHandler<RoutedEventArgs> ImageOpened;

        public event EventHandler<ExceptionRoutedEventArgs> ImageFailed;

        public event EventHandler<RoutedEventArgs> PlaceholderOpened;

        public event EventHandler<ExceptionRoutedEventArgs> PlaceholderFailed;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _image = GetTemplateChild("Image") as Image;
            _placeholder = GetTemplateChild("Placeholder") as Image;

            _image.ImageOpened += OnImageOpened;
            _image.ImageFailed += OnImageFailed;
            _image.Source = ImageSource;
            _placeholder.ImageOpened += OnPlaceholderOpened;
            _placeholder.ImageFailed += OnPlaceholderFailed;
            _placeholder.Source = PlaceholderSource;

            _animation = new Storyboard();
            var hideAnimation = new DoubleAnimation() {To = 0};
            Storyboard.SetTarget(hideAnimation, _placeholder);
            Storyboard.SetTargetProperty(hideAnimation, new PropertyPath("(UIElement.Opacity)"));
            
            var showAnimation = new DoubleAnimation() { To = 1 };
            Storyboard.SetTarget(showAnimation, _image);
            Storyboard.SetTargetProperty(showAnimation, new PropertyPath("(UIElement.Opacity)"));
            
            _animation.Children.Add(hideAnimation);
            _animation.Children.Add(showAnimation);

        }

        private void Reset()
        {
            if (_image != null && _placeholder != null)
            {
                _isImageLoaded = false;
                _image.ImageOpened -= OnImageOpened;
                _image.ImageFailed -= OnImageFailed;
                _image.ImageOpened += OnImageOpened;
                _image.ImageFailed += OnImageFailed;
                
                _image.Source = ImageSource;

                _animation.Stop();
                _image.Opacity = 0;
                _placeholder.Opacity = 1;
            }
        }

        private void OnImageOpened(object sender, RoutedEventArgs args)
        {
            _isImageLoaded = true;

            EventHandler<RoutedEventArgs> imageOpenedCopy = ImageOpened;



            if (!IsFrozen)
            {
                //VisualStateManager.GoToState(this, "ImageState", true);
                _animation.Completed += delegate
                                            {
                                                if (imageOpenedCopy != null)
                                                {
                                                    imageOpenedCopy(this, args);
                                                }
                                            };
                _animation.Begin();
                
            }
            else
            {
                _image.Opacity = 1.0;
                _placeholder.Opacity = 0.0;

                if (imageOpenedCopy != null)
                {
                    imageOpenedCopy(this, args);
                }
            }

            
        }

        private void OnImageFailed(object sender, ExceptionRoutedEventArgs args)
        {
            EventHandler<ExceptionRoutedEventArgs> imageFailedCopy = ImageFailed;

            if (imageFailedCopy != null)
            {
                imageFailedCopy(this, args);
            }
        }

        private void OnPlaceholderOpened(object sender, RoutedEventArgs args)
        {
            EventHandler<RoutedEventArgs> placeholderOpenedCopy = PlaceholderOpened;

            if (placeholderOpenedCopy != null)
            {
                placeholderOpenedCopy(this, args);
            }
        }


        private void OnPlaceholderFailed(object sender, ExceptionRoutedEventArgs args)
        {
            EventHandler<ExceptionRoutedEventArgs> placeholderFailedCopy = PlaceholderFailed;

            if (placeholderFailedCopy != null)
            {
                placeholderFailedCopy(this, args);
            }
        }
    }
}