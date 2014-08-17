using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using LiveSplit.TimeFormatters;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LiveSplit.UI.Components
{
    public class PreviousSegment : IComponent
    {
        protected InfoTimeComponent InternalComponent { get; set; }
        public PreviousSegmentSettings Settings { get; set; }

        public GraphicsCache Cache { get; set; }

        protected DeltaTimeFormatter Formatter { get; set; }

        public float PaddingTop { get { return InternalComponent.PaddingTop; } }
        public float PaddingLeft { get { return InternalComponent.PaddingLeft; } }
        public float PaddingBottom { get { return InternalComponent.PaddingBottom; } }
        public float PaddingRight { get { return InternalComponent.PaddingRight; } }

        public IDictionary<string, Action> ContextMenuControls
        {
            get { return null; }
        }

        public PreviousSegment(LiveSplitState state)
        {
            Formatter = new DeltaTimeFormatter();
            Settings = new PreviousSegmentSettings()
            {
                CurrentState = state
            };
            Formatter.Accuracy = Settings.DeltaAccuracy;
            Formatter.DropDecimals = Settings.DropDecimals;
            InternalComponent = new InfoTimeComponent(null, null, Formatter);
            Cache = new GraphicsCache();
        }

        private void PrepareDraw(LiveSplitState state)
        {
            InternalComponent.DisplayTwoRows = Settings.Display2Rows;

            Formatter.Accuracy = Settings.DeltaAccuracy;
            Formatter.DropDecimals = Settings.DropDecimals;

            InternalComponent.NameLabel.HasShadow
                = InternalComponent.ValueLabel.HasShadow
                = state.LayoutSettings.DropShadows;
            InternalComponent.NameLabel.ForeColor = Settings.OverrideTextColor ? Settings.TextColor : state.LayoutSettings.TextColor;
        }

        private void DrawBackground(Graphics g, LiveSplitState state, float width, float height)
        {
            if (Settings.BackgroundColor.ToArgb() != Color.Transparent.ToArgb()
                || Settings.BackgroundGradient != GradientType.Plain
                && Settings.BackgroundColor2.ToArgb() != Color.Transparent.ToArgb())
            {
                var gradientBrush = new LinearGradientBrush(
                            new PointF(0, 0),
                            Settings.BackgroundGradient == GradientType.Horizontal
                            ? new PointF(width, 0)
                            : new PointF(0, height),
                            Settings.BackgroundColor,
                            Settings.BackgroundGradient == GradientType.Plain
                            ? Settings.BackgroundColor
                            : Settings.BackgroundColor2);
                g.FillRectangle(gradientBrush, 0, 0, width, height);
            }
        }

        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion)
        {
            DrawBackground(g, state, width, VerticalHeight);
            PrepareDraw(state);
            InternalComponent.DrawVertical(g, state, width, clipRegion);
        }

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion)
        {
            DrawBackground(g, state, HorizontalWidth, height);
            PrepareDraw(state);
            InternalComponent.DrawHorizontal(g, state, height, clipRegion);
        }

        public float VerticalHeight
        {
            get { return InternalComponent.VerticalHeight; }
        }

        public float MinimumWidth
        {
            get { return InternalComponent.MinimumWidth; }
        }

        public float HorizontalWidth
        {
            get { return InternalComponent.HorizontalWidth; }
        }

        public float MinimumHeight
        {
            get { return InternalComponent.MinimumHeight; }
        }

        public string ComponentName
        {
            get { return "Previous Segment" + (Settings.Comparison == "Current Comparison" ? "" : " (" + CompositeComparisons.GetShortComparisonName(Settings.Comparison) + ")"); }
        }

        public Control GetSettingsControl(LayoutMode mode)
        {
            Settings.Mode = mode;
            return Settings;
        }

        public void SetSettings(System.Xml.XmlNode settings)
        {
            Settings.SetSettings(settings);
        }

        public System.Xml.XmlNode GetSettings(System.Xml.XmlDocument document)
        {
            return Settings.GetSettings(document);
        }


        public void RenameComparison(string oldName, string newName)
        {
            if (Settings.Comparison == oldName)
                Settings.Comparison = newName;
        }


        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            var comparison = Settings.Comparison == "Current Comparison" ? state.CurrentComparison : Settings.Comparison;
            if (!state.Run.Comparisons.Contains(comparison))
                comparison = state.CurrentComparison;
            var comparisonName = CompositeComparisons.GetShortComparisonName(comparison);

            var componentName = "Previous Segment" + (Settings.Comparison == "Current Comparison" ? "" : " (" + comparisonName + ")");

            InternalComponent.LongestString = componentName;
            InternalComponent.InformationName = componentName;

            if (state.CurrentPhase != TimerPhase.NotRunning)
            {
                bool liveSeg = false;
                TimeSpan? timeChange = null;
                if (state.CurrentPhase == TimerPhase.Running || state.CurrentPhase == TimerPhase.Paused)
                {
                    if (LiveSplitStateHelper.CheckBestSegment(state, true, state.LayoutSettings.ShowBestSegments, comparison, state.CurrentTimingMethod) != null)
                        liveSeg = true;
                }
                if (liveSeg)
                {
                    timeChange = LiveSplitStateHelper.GetPreviousSegment(state, state.CurrentSplitIndex, true, false, true, comparison, state.CurrentTimingMethod);
                    InternalComponent.InformationName = "Live Segment" + (Settings.Comparison == "Current Comparison" ? "" : " (" + comparisonName + ")");
                }
                else if (state.CurrentSplitIndex > 0)
                {
                    timeChange = LiveSplitStateHelper.GetPreviousSegment(state, state.CurrentSplitIndex - 1, false, false, true, comparison, state.CurrentTimingMethod);
                }
                if (timeChange != null)
                {
                    InternalComponent.TimeValue = timeChange;
                    if (liveSeg)
                        InternalComponent.ValueLabel.ForeColor = LiveSplitStateHelper.GetSplitColor(state, timeChange, 2, state.CurrentSplitIndex, comparison, state.CurrentTimingMethod).Value;
                    else
                        InternalComponent.ValueLabel.ForeColor = LiveSplitStateHelper.GetSplitColor(state, timeChange.Value, 1, state.CurrentSplitIndex - 1, comparison, state.CurrentTimingMethod).Value;
                }
                else
                {
                    InternalComponent.TimeValue = null;
                    var color = LiveSplitStateHelper.GetSplitColor(state, null, 0, state.CurrentSplitIndex - 1, comparison, state.CurrentTimingMethod);
                    if (color == null)
                        color = Settings.OverrideTextColor ? Settings.TextColor : state.LayoutSettings.TextColor;
                    InternalComponent.ValueLabel.ForeColor = color.Value;
                }
            }
            else
            {
                InternalComponent.TimeValue = null;
                InternalComponent.ValueLabel.ForeColor = Settings.OverrideTextColor ? Settings.TextColor : state.LayoutSettings.TextColor;
            }

            Cache.Restart();
            Cache["NameValue"] = InternalComponent.InformationName;
            if (Cache.HasChanged)
            {
                InternalComponent.AlternateNameText.Clear();
                if (InternalComponent.InformationName == "Previous Segment")
                {
                    InternalComponent.AlternateNameText.Add("Previous Segment");
                    InternalComponent.AlternateNameText.Add("Prev. Segment");
                    InternalComponent.AlternateNameText.Add("Prev. Seg.");
                }
                else
                {
                    InternalComponent.AlternateNameText.Add("Live Segment");
                    InternalComponent.AlternateNameText.Add("Live Seg.");
                }
            }
            Cache["TimeValue"] = InternalComponent.ValueLabel.Text;
            Cache["TimeColor"] = InternalComponent.ValueLabel.ForeColor.ToArgb();

            if (invalidator != null && Cache.HasChanged)
            {
                invalidator.Invalidate(0, 0, width, height);
            }
        }

        public void Dispose()
        {
        }
    }
}
