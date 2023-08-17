﻿using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TheManager;
using TheManager_GUI.Styles;

namespace TheManager_GUI.views
{

    public struct PieChartValue
    {
        public string label { get; set; }
        public double value { get; set; }
        public string displayValue { get; set; }
        public PieChartValue(string label, double value, string displayValue)
        {
            this.label = label;
            this.value = value;
            this.displayValue = displayValue;
        }
    }

    public enum ChartType
    {
        LINE_CHART = 1,
        PIE_CHART = 2
    }

    public class ChartView
    {

        private ChartType chartType { get; }

        private string[] labels { get; set; }

        private string title { get; }
        private string axisYtitle { get; }
        private string axisXtitle { get; }

        private bool representsMoney { get; }

        private float sizeMultiplier { get; }

        private List<double> values { get; set; }

        private float minValue { get; }
        private float maxValue { get; }
        private float width { get; }
        private float height { get; }

        public ChartView(ChartType chartType, string title, string axisYtitle, string axisXtitle, List<string> labels, bool representsMoney, float sizeMultiplier, List<double> values, float width, float height, float minValue = -1, float maxValue = -1)
        {
            this.chartType = chartType;
            this.labels = labels.ToArray();
            this.title = title;
            this.representsMoney = representsMoney;
            this.sizeMultiplier = sizeMultiplier;
            this.values = values;
            this.axisYtitle = axisYtitle;
            this.axisXtitle = axisXtitle;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.width = width;
            this.height = height;
        }

        public Chart RenderChart(StackPanel host)
        {
            switch (chartType)
            {
                case ChartType.LINE_CHART:
                    return RenderLineChart(host);
                    break;
                case ChartType.PIE_CHART: default:
                    return RenderPieChart(host);
                    break;
            }
        }

        public PieChart RenderPieChart(StackPanel host)
        {

            var pieChartMapper = Mappers.Xy<PieChartValue>()
                .X((value, index) => index) // lets use the position of the item as X
                .Y(value => value.value); //and PurchasedItems property as Y

            //lets save the mapper globally
            Charting.For<PieChartValue>(pieChartMapper);

            Func<ChartPoint, string> labelFormatter = value => ((PieChartValue)value.Instance).displayValue;
            SeriesCollection series = new SeriesCollection();
            for (int i = 0; i < values.Count; i++)
            {
                series.Add(new PieSeries
                {
                    Title = labels[i],
                    DataLabels = true,
                    Stroke = Brushes.Transparent,
                    StrokeThickness = 5,
                    LabelPoint = labelFormatter, //Used when display ToolTip, but will be eventually customized
                    Values = new ChartValues<PieChartValue> { new PieChartValue(labels[i], values[i], Utils.FormatMoney((float)values[i])) },
                    Style = Application.Current.FindResource(StyleDefinition.styleLiveChartPieSerie) as Style
                });
            }

            PieChart pc = new PieChart();
            pc.Width = width * sizeMultiplier;
            pc.Height = height * sizeMultiplier;
            pc.SeriesColors = new ColorsCollection();
            pc.Style = Application.Current.FindResource(StyleDefinition.styleLiveChartPieChart) as Style;
            pc.InnerRadius = height / 4;
            pc.DataContext = series;
            //pc.DataTooltip = new ChartTooltip();


            List<System.Windows.Media.Color> availableColors = new List<System.Windows.Media.Color>();
            availableColors.Add((System.Windows.Media.Color)Application.Current.FindResource(StyleDefinition.colorViewBorder1));
            availableColors.Add((System.Windows.Media.Color)Application.Current.FindResource(StyleDefinition.colorViewBorder2));
            availableColors.Add((System.Windows.Media.Color)Application.Current.FindResource(StyleDefinition.colorViewBorder3));

            for(int i = 0; i < values.Count; i++)
            {
                pc.SeriesColors.Add(availableColors[i%availableColors.Count]);
            }

            pc.Series = series;

            host.Children.Add(pc);

            return null;
        }

        public CartesianChart RenderLineChart(StackPanel host)
        {
            double fontSize = (double)Application.Current.FindResource(StyleDefinition.fontSizeRegular);
            TextBlock labelTitle = ViewUtils.CreateTextBlock(title, StyleDefinition.styleTextPlainCenter, fontSize, -1);

            host.Children.Add(labelTitle);

            ChartValues<double> chartValues = new ChartValues<double>(values.ToArray());

            SeriesCollection serieCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = title,
                    PointGeometrySize = 12,
                    PointForeground = Brushes.Transparent,
                    StrokeThickness = 7,
                    Fill = Brushes.Transparent,
                    Stroke = Application.Current.FindResource(StyleDefinition.solidColorBrushColorButtonOver) as SolidColorBrush,
                    /*Configuration = new CartesianMapper<Point>()
                        .X(point => point.X)
                        .Y(point => point.Y)
                        .Stroke(point => Brushes.Red) //Can make conditional plotting prom point value
                        .Fill(point => Brushes.Transparent),*/
                    Values = chartValues
                }
            };

            CartesianChart cc = new CartesianChart();
            cc.Width = width * sizeMultiplier;
            cc.Height = height * sizeMultiplier;
            cc.Series = serieCollection;
            cc.LegendLocation = LegendLocation.None;
            cc.Style = Application.Current.FindResource(StyleDefinition.styleLiveChartCartesianChart) as Style;

            SolidColorBrush linesColor = Application.Current.FindResource(StyleDefinition.solidColorBrushColorTitle1) as SolidColorBrush;

            Axis axisY = new Axis();
            axisY.Title = axisYtitle;
            axisY.MinValue = minValue;
            axisY.MaxValue = maxValue;
            axisY.Style = Application.Current.FindResource(StyleDefinition.styleLiveChartAxis) as Style;
            axisY.Sections.Add(new AxisSection() { Stroke = linesColor, StrokeThickness = 1, Value = minValue });
            axisY.Separator.IsEnabled = false; // Hide axis grid

            if (representsMoney)
            {
                Func<double, string> YFormatter = value => value.ToString("C");
                axisY.LabelFormatter = YFormatter;
            }

            Axis axisX = new Axis();
            axisX.Title = axisXtitle;
            axisX.Labels = labels;
            axisX.Style = Application.Current.FindResource(StyleDefinition.styleLiveChartAxis) as Style;
            axisX.Sections.Add(new AxisSection() { Stroke = linesColor, StrokeThickness = 1, Value = 0 });
            axisX.Separator.IsEnabled = false; // Hide axis grid

            cc.AxisY.Add(axisY);
            cc.AxisX.Add(axisX);
            host.Children.Add(cc);

            return cc;
        }

    }
}
