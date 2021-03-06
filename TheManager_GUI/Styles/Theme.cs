﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TheManager_GUI
{
    public class Theme
    {
        private readonly static List<Theme> _themes = new List<Theme>();
        public static List<Theme> themes => _themes;

        private readonly string _name;
        private readonly string _backgroundColor;
        private readonly string _mainColor;
        private readonly string _secondaryColor;
        private readonly string _promotionColor;
        private readonly string _upperPlayOffColor;
        private readonly string _bottomPlayOffColor;
        private readonly string _relegationColor;
        private readonly string _fontFamily;

        public string name => _name;
        public string backgroundColor => _backgroundColor;
        public string mainColor => _mainColor;
        public string secondaryColor => _secondaryColor;
        public string fontFamily => _fontFamily;

        public string promotionColor => _promotionColor;
        public string relegationColor => _relegationColor;
        public string upperPlayOffColor => _upperPlayOffColor;
        public string bottomPlayOffColor => _bottomPlayOffColor;

        /// <summary>
        /// Create a new theme
        /// </summary>
        /// <param name="name">Name of the theme</param>
        /// <param name="backgroundColor">Background hexa color</param>
        /// <param name="mainColor">Main hexa color</param>
        /// <param name="secondaryColor">Secondary hexa color</param>
        /// <param name="fontFamily">Font family of the theme</param>
        public Theme(string name, string backgroundColor, string mainColor, string secondaryColor, string promotionColor, string upperPlayOffColor, string bottomPlayOffColor, string relagationColor, string fontFamily)
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
        }


    }
}
