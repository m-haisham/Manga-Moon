using System;
using System.Windows.Media.Imaging;

namespace Mago
{
    public class PageViewModel : BaseViewModel
    {
        
        private BitmapImage _source;
        private float _zoom;
        private int _width;
        private int _margin;
        
        public PageViewModel(BitmapImage source)
        {
            _source = source;
            _width = Source.PixelWidth;
        }
        
        public float Zoom
        {
            get { return _zoom; }
            set
            {
                if (_zoom == value) return;
                _zoom = value;
                Width = (int)(Source.PixelWidth * _zoom);
            }
        }

        public BitmapImage Source
        {
            get { return _source; }
            set
            {
                if (_source == value) return;
                _source = value;
            }
        }

        public int Width
        {
            get { return _width; }
            set
            {
                if (_width == value) return;
                _width = value;
            }
        }

        public int Margin
        {
            get { return _margin; }
            set
            {
                if (_margin == value) return;
                _margin = value;
            }
        }

    }
}