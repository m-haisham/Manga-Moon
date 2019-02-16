namespace Mago
{
    public class GenreItemViewModel : BaseViewModel
    {
        private string _text;
        
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value) return;
                _text = value;
            }
        }
    }
}