using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TheManager_GUI.Styles;

namespace TheManager_GUI
{
    public class Theme
    {
        private readonly static List<Theme> _themes = new List<Theme>();
        public static List<Theme> themes => _themes;
        public static Theme current { get; set; }


        private readonly string _name;
        private readonly string _backgroundColor;
        private readonly string _mainColor;
        private readonly string _secondaryColor;
        private readonly string _promotionColor;
        private readonly string _upperPlayOffColor;
        private readonly string _bottomPlayOffColor;
        private readonly string _relegationColor;
        private readonly string _fontFamily;
        private readonly string _dateColor;

        public string name => _name;
        public string backgroundColor => _backgroundColor;
        public string mainColor => _mainColor;
        public string secondaryColor => _secondaryColor;
        public string fontFamily => _fontFamily;

        public string promotionColor => _promotionColor;
        public string relegationColor => _relegationColor;
        public string upperPlayOffColor => _upperPlayOffColor;
        public string bottomPlayOffColor => _bottomPlayOffColor;

        public string DateColor => _dateColor;

        /// <summary>
        /// Create a new theme
        /// </summary>
        /// <param name="name">Name of the theme</param>
        /// <param name="backgroundColor">Background hexa color</param>
        /// <param name="mainColor">Main hexa color</param>
        /// <param name="secondaryColor">Secondary hexa color</param>
        /// <param name="fontFamily">Font family of the theme</param>
        /// <param name="dateColor">Tertiary color for dates</param>
        public Theme(string name, string backgroundColor, string mainColor, string secondaryColor, string promotionColor, string upperPlayOffColor, string bottomPlayOffColor, string relagationColor, string fontFamily, string dateColor)
        {
            _name = name;
            _backgroundColor = backgroundColor;
            _mainColor = mainColor;
            _secondaryColor = secondaryColor;
            _promotionColor = promotionColor;
            _upperPlayOffColor = upperPlayOffColor;
            _bottomPlayOffColor = bottomPlayOffColor;
            _relegationColor = relagationColor;
            _fontFamily = fontFamily;
            _dateColor = dateColor;
        }

        public void SetAsCurrentTheme()
        {
            current = this;
        }

        public override string ToString()
        {
            return this.name;
        }


    }
}
