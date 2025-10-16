using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager_GUI.Styles
{
    public static class StyleDefinition
    {
        public static readonly string styleTextNavigation = "textNavigation";
        public static readonly string styleTextPlain = "textPlain";
        public static readonly string styleTextPlainCenter = "textPlainCenter";
        public static readonly string styleTextButton = "textButton";
        public static readonly string styleTextTitle = "textTitle";
        public static readonly string styleTextSecondary = "textSecondary";
        public static readonly string styleLiveChartAxis = "liveChartAxisStyle";
        public static readonly string styleLiveChartPieChart = "liveChartPieChartStyle";
        public static readonly string styleLiveChartPieSerie = "liveChartPieSerieStyle";
        public static readonly string styleLiveChartCartesianChart = "liveChartCartesianChartStyle";
        public static readonly string styleCheckBox = "checkBoxStyle";
        public static readonly string styleToolTip = "toolTipStyle";
        public static readonly string styleButtonMenu = "buttonMenu";
        public static readonly string styleButtonMenuTitle = "buttonMenuTitle";
        public static readonly string comboBoxFlatStyle = "ComboBoxFlatStyle";
        public static readonly string comboBoxStyle = "comboBoxStyle";
        public static readonly string tabItemStyle = "tabItemStyle";

        public static readonly string fontSizeTitle = "fontSizeTitle";
        public static readonly string fontSizeSecondary = "fontSizeSecondary";
        public static readonly string fontSizeRegular = "fontSizeRegular";
        public static readonly string fontSizeNavigation = "fontSizeNavigation";

        public static readonly string solidColorBrushColorTitle1 = "colorTitle1";
        public static readonly string solidColorBrushColorTitle2 = "colorTitle2";
        public static readonly string solidColorBrushColorPlainText11 = "colorPlainText1";
        public static readonly string solidColorBrushColorPanel1 = "colorPanel1";
        public static readonly string solidColorBrushColorPanel2 = "colorPanel2";
        public static readonly string solidColorBrushColorPanel3 = "colorPanel3";
        public static readonly string solidColorBrushColorButtonOver = "colorButtonOver";
        public static readonly string solidColorBrushColorBorderLight = "colorBorderLight";
        public static readonly string solidColorBrushColorTransparent = "colorTransparent";
        public static readonly string solidColorBrushColorLight = "colorLight";
        
        public static readonly string colorViewBorder1 = "colorViewBorder1";
        public static readonly string colorViewBorder2 = "colorViewBorder2";
        public static readonly string colorViewBorder3 = "colorViewBorder3";
        public static readonly string colorPositive = "colorPositive";
        public static readonly string colorNegative = "colorNegative";

        public static readonly string slotPromotion = "promotionColor";
        public static readonly string slotRetrogradation = "retrogradationColor";
        public static readonly string slotBarrageRelegation = "barrageRelegationColor";
        public static readonly string slotBackground = "backgroundColor";
        public static readonly string slotRelegation = "relegationColor";
        public static readonly string slotBarrage = "barrageColor";
        public static readonly string slotQualification1a = "cl1Color";
        public static readonly string slotQualification1b = "cl2Color";
        public static readonly string slotQualification2a = "el1Color";
        public static readonly string slotQualification2b = "el2Color";
        public static readonly string slotQualification3a = "ecl1Color";

        public static Dictionary<string, string> slotLightShade = new Dictionary<string, string>()
        {
            {slotPromotion, slotQualification1b },
            {slotRetrogradation, slotRetrogradation },
            {slotBarrageRelegation, slotBarrageRelegation },
            {slotBackground, slotBackground },
            {slotRelegation, slotBarrageRelegation },
            {slotBarrage, slotBarrage },
            {slotQualification1a, slotQualification1b },
            {slotQualification1b, slotQualification2a }, // Other shade ?
            {slotQualification2a, slotQualification2b },
            {slotQualification2b, slotQualification3a }, // Other shade ?
            {slotQualification3a, slotQualification3a }, // Other shade ?
        };
    }
}
