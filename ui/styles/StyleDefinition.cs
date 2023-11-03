using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager_GUI.Styles
{
    public static class StyleDefinition
    {
        public static string styleTextNavigation = "textNavigation";
        public static string styleTextPlain = "textPlain";
        public static string styleTextPlainCenter = "textPlainCenter";
        public static string styleTextButton = "textButton";
        public static string styleTextTitle = "textTitle";
        public static string styleTextSecondary = "textSecondary";
        public static string styleLiveChartAxis = "liveChartAxisStyle";
        public static string styleLiveChartPieChart = "liveChartPieChartStyle";
        public static string styleLiveChartPieSerie = "liveChartPieSerieStyle";
        public static string styleLiveChartCartesianChart = "liveChartCartesianChartStyle";
        public static string styleCheckBox = "checkBoxStyle";
        public static string styleToolTip = "toolTipStyle";
        public static string styleButtonMenu = "buttonMenu";
        public static string styleButtonMenuTitle = "buttonMenuTitle";
        public static string comboBoxFlatStyle = "ComboBoxFlatStyle";
        public static string comboBoxStyle = "comboBoxStyle";
        public static string tabItemStyle = "tabItemStyle";

        public static string fontSizeTitle = "fontSizeTitle";
        public static string fontSizeSecondary = "fontSizeSecondary";
        public static string fontSizeRegular = "fontSizeRegular";
        public static string fontSizeNavigation = "fontSizeNavigation";

        public static string solidColorBrushColorTitle1 = "colorTitle1";
        public static string solidColorBrushColorTitle2 = "colorTitle2";
        public static string solidColorBrushColorPlainText11 = "colorPlainText1";
        public static string solidColorBrushColorPanel1 = "colorPanel1";
        public static string solidColorBrushColorPanel2 = "colorPanel2";
        public static string solidColorBrushColorPanel3 = "colorPanel3";
        public static string solidColorBrushColorButtonOver = "colorButtonOver";
        public static string solidColorBrushColorBorderLight = "colorBorderLight";
        public static string solidColorBrushColorTransparent = "colorTransparent";
        public static string solidColorBrushColorLight = "colorLight";
        
        public static string colorViewBorder1 = "colorViewBorder1";
        public static string colorViewBorder2 = "colorViewBorder2";
        public static string colorViewBorder3 = "colorViewBorder3";
        public static string colorPositive = "colorPositive";
        public static string colorNegative = "colorNegative";

        public static string slotPromotion = "promotionColor";
        public static string slotRetrogradation = "retrogradationColor";
        public static string slotBarrageRelegation = "barrageRelegationColor";
        public static string slotBackground = "backgroundColor";
        public static string slotRelegation = "relegationColor";
        public static string slotBarrage = "barrageColor";
        public static string slotQualification1a = "cl1Color";
        public static string slotQualification1b = "cl2Color";
        public static string slotQualification2a = "el1Color";
        public static string slotQualification2b = "el2Color";
        public static string slotQualification3a = "ecl1Color";

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
