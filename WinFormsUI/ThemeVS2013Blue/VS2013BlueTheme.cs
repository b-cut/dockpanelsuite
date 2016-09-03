using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    using WeifenLuo.WinFormsUI.ThemeVS2012;
    using WeifenLuo.WinFormsUI.ThemeVS2012.Light;

    /// <summary>
    /// Visual Studio 2013 Light theme.
    /// </summary>
    public class VS2013BlueTheme : ThemeBase
    {
        public VS2013BlueTheme()
        {
            Skin = CreateVisualStudio2013Blue();
            PaintingService = new PaintingService();
        }

        /// <summary>
        /// Applies the specified theme to the dock panel.
        /// </summary>
        /// <param name="dockPanel">The dock panel.</param>
        public override void Apply(DockPanel dockPanel)
        {
            if (Extender != null)
            {
                return;
            }

            Extender = new DockPanelExtender(dockPanel);
            Measures.SplitterSize = 6;
            Extender.DockPaneCaptionFactory = new VS2013BlueDockPaneCaptionFactory();
            Extender.AutoHideStripFactory = new VS2013BlueAutoHideStripFactory();
            Extender.AutoHideWindowFactory = new VS2013BlueAutoHideWindowFactory();
            Extender.DockPaneStripFactory = new VS2013BlueDockPaneStripFactory();
            Extender.DockPaneSplitterControlFactory = new VS2013BlueDockPaneSplitterControlFactory();
            Extender.DockWindowSplitterControlFactory = new VS2013BlueDockWindowSplitterControlFactory();
            Extender.DockWindowFactory = new VS2013BlueDockWindowFactory();
            Extender.PaneIndicatorFactory = new VS2013BluePaneIndicatorFactory();
            Extender.PanelIndicatorFactory = new VS2013BluePanelIndicatorFactory();
            Extender.DockOutlineFactory = new VS2013BlueDockOutlineFactory();
        }

        public override void CleanUp(DockPanel dockPanel)
        {
            PaintingService.CleanUp();
            base.CleanUp(dockPanel);
        }

        private class VS2013BlueDockOutlineFactory : DockPanelExtender.IDockOutlineFactory
        {
            public DockOutlineBase CreateDockOutline()
            {
                return new VS2013BlueDockOutline();
            }

            private class VS2013BlueDockOutline : DockOutlineBase
            {
                public VS2013BlueDockOutline()
                {
                    m_dragForm = new DragForm();
                    SetDragForm(Rectangle.Empty);
                    DragForm.BackColor = Color.FromArgb(0xff, 91, 173, 255);
                    DragForm.Opacity = 0.5;
                    DragForm.Show(false);
                }

                DragForm m_dragForm;
                private DragForm DragForm
                {
                    get { return m_dragForm; }
                }

                protected override void OnShow()
                {
                    CalculateRegion();
                }

                protected override void OnClose()
                {
                    DragForm.Close();
                }

                private void CalculateRegion()
                {
                    if (SameAsOldValue)
                        return;

                    if (!FloatWindowBounds.IsEmpty)
                        SetOutline(FloatWindowBounds);
                    else if (DockTo is DockPanel)
                        SetOutline(DockTo as DockPanel, Dock, (ContentIndex != 0));
                    else if (DockTo is DockPane)
                        SetOutline(DockTo as DockPane, Dock, ContentIndex);
                    else
                        SetOutline();
                }

                private void SetOutline()
                {
                    SetDragForm(Rectangle.Empty);
                }

                private void SetOutline(Rectangle floatWindowBounds)
                {
                    SetDragForm(floatWindowBounds);
                }

                private void SetOutline(DockPanel dockPanel, DockStyle dock, bool fullPanelEdge)
                {
                    Rectangle rect = fullPanelEdge ? dockPanel.DockArea : dockPanel.DocumentWindowBounds;
                    rect.Location = dockPanel.PointToScreen(rect.Location);
                    if (dock == DockStyle.Top)
                    {
                        int height = dockPanel.GetDockWindowSize(DockState.DockTop);
                        rect = new Rectangle(rect.X, rect.Y, rect.Width, height);
                    }
                    else if (dock == DockStyle.Bottom)
                    {
                        int height = dockPanel.GetDockWindowSize(DockState.DockBottom);
                        rect = new Rectangle(rect.X, rect.Bottom - height, rect.Width, height);
                    }
                    else if (dock == DockStyle.Left)
                    {
                        int width = dockPanel.GetDockWindowSize(DockState.DockLeft);
                        rect = new Rectangle(rect.X, rect.Y, width, rect.Height);
                    }
                    else if (dock == DockStyle.Right)
                    {
                        int width = dockPanel.GetDockWindowSize(DockState.DockRight);
                        rect = new Rectangle(rect.Right - width, rect.Y, width, rect.Height);
                    }
                    else if (dock == DockStyle.Fill)
                    {
                        rect = dockPanel.DocumentWindowBounds;
                        rect.Location = dockPanel.PointToScreen(rect.Location);
                    }

                    SetDragForm(rect);
                }

                private void SetOutline(DockPane pane, DockStyle dock, int contentIndex)
                {
                    if (dock != DockStyle.Fill)
                    {
                        Rectangle rect = pane.DisplayingRectangle;
                        if (dock == DockStyle.Right)
                            rect.X += rect.Width / 2;
                        if (dock == DockStyle.Bottom)
                            rect.Y += rect.Height / 2;
                        if (dock == DockStyle.Left || dock == DockStyle.Right)
                            rect.Width -= rect.Width / 2;
                        if (dock == DockStyle.Top || dock == DockStyle.Bottom)
                            rect.Height -= rect.Height / 2;
                        rect.Location = pane.PointToScreen(rect.Location);

                        SetDragForm(rect);
                    }
                    else if (contentIndex == -1)
                    {
                        Rectangle rect = pane.DisplayingRectangle;
                        rect.Location = pane.PointToScreen(rect.Location);
                        SetDragForm(rect);
                    }
                    else
                    {
                        using (GraphicsPath path = pane.TabStripControl.GetOutline(contentIndex))
                        {
                            RectangleF rectF = path.GetBounds();
                            Rectangle rect = new Rectangle((int)rectF.X, (int)rectF.Y, (int)rectF.Width, (int)rectF.Height);
                            using (Matrix matrix = new Matrix(rect, new Point[] { new Point(0, 0), new Point(rect.Width, 0), new Point(0, rect.Height) }))
                            {
                                path.Transform(matrix);
                            }

                            Region region = new Region(path);
                            SetDragForm(rect, region);
                        }
                    }
                }

                private void SetDragForm(Rectangle rect)
                {
                    DragForm.Bounds = rect;
                    if (rect == Rectangle.Empty)
                    {
                        if (DragForm.Region != null)
                        {
                            DragForm.Region.Dispose();
                        }

                        DragForm.Region = new Region(Rectangle.Empty);
                    }
                    else if (DragForm.Region != null)
                    {
                        DragForm.Region.Dispose();
                        DragForm.Region = null;
                    }
                }

                private void SetDragForm(Rectangle rect, Region region)
                {
                    DragForm.Bounds = rect;
                    if (DragForm.Region != null)
                    {
                        DragForm.Region.Dispose();
                    }

                    DragForm.Region = region;
                }
            }
        }

        private class VS2013BluePanelIndicatorFactory : DockPanelExtender.IPanelIndicatorFactory
        {
            public DockPanel.IPanelIndicator CreatePanelIndicator(DockStyle style, ThemeBase theme)
            {
                return new VS2013BluePanelIndicator(style);
            }

            private class VS2013BluePanelIndicator : PictureBox, DockPanel.IPanelIndicator
            {
                private static Image _imagePanelLeft = Resources.DockIndicator_PanelLeft;
                private static Image _imagePanelRight = Resources.DockIndicator_PanelRight;
                private static Image _imagePanelTop = Resources.DockIndicator_PanelTop;
                private static Image _imagePanelBottom = Resources.DockIndicator_PanelBottom;
                private static Image _imagePanelFill = Resources.DockIndicator_PanelFill;
                private static Image _imagePanelLeftActive = Resources.DockIndicator_PanelLeft;
                private static Image _imagePanelRightActive = Resources.DockIndicator_PanelRight;
                private static Image _imagePanelTopActive = Resources.DockIndicator_PanelTop;
                private static Image _imagePanelBottomActive = Resources.DockIndicator_PanelBottom;
                private static Image _imagePanelFillActive = Resources.DockIndicator_PanelFill;

                public VS2013BluePanelIndicator(DockStyle dockStyle)
                {
                    m_dockStyle = dockStyle;
                    SizeMode = PictureBoxSizeMode.AutoSize;
                    Image = ImageInactive;
                }

                private DockStyle m_dockStyle;

                private DockStyle DockStyle
                {
                    get { return m_dockStyle; }
                }

                private DockStyle m_status;

                public DockStyle Status
                {
                    get { return m_status; }
                    set
                    {
                        if (value != DockStyle && value != DockStyle.None)
                            throw new InvalidEnumArgumentException();

                        if (m_status == value)
                            return;

                        m_status = value;
                        IsActivated = (m_status != DockStyle.None);
                    }
                }

                private Image ImageInactive
                {
                    get
                    {
                        if (DockStyle == DockStyle.Left)
                            return _imagePanelLeft;
                        else if (DockStyle == DockStyle.Right)
                            return _imagePanelRight;
                        else if (DockStyle == DockStyle.Top)
                            return _imagePanelTop;
                        else if (DockStyle == DockStyle.Bottom)
                            return _imagePanelBottom;
                        else if (DockStyle == DockStyle.Fill)
                            return _imagePanelFill;
                        else
                            return null;
                    }
                }

                private Image ImageActive
                {
                    get
                    {
                        if (DockStyle == DockStyle.Left)
                            return _imagePanelLeftActive;
                        else if (DockStyle == DockStyle.Right)
                            return _imagePanelRightActive;
                        else if (DockStyle == DockStyle.Top)
                            return _imagePanelTopActive;
                        else if (DockStyle == DockStyle.Bottom)
                            return _imagePanelBottomActive;
                        else if (DockStyle == DockStyle.Fill)
                            return _imagePanelFillActive;
                        else
                            return null;
                    }
                }

                private bool m_isActivated = false;

                private bool IsActivated
                {
                    get { return m_isActivated; }
                    set
                    {
                        m_isActivated = value;
                        Image = IsActivated ? ImageActive : ImageInactive;
                    }
                }

                public DockStyle HitTest(Point pt)
                {
                    return this.Visible && ClientRectangle.Contains(PointToClient(pt)) ? DockStyle : DockStyle.None;
                }
            }
        }

        private class VS2013BluePaneIndicatorFactory : DockPanelExtender.IPaneIndicatorFactory
        {
            public DockPanel.IPaneIndicator CreatePaneIndicator(ThemeBase theme)
            {
                return new VS2013BluePaneIndicator();
            }

            private class VS2013BluePaneIndicator : PictureBox, DockPanel.IPaneIndicator
            {
                private static Bitmap _bitmapPaneDiamond = Resources.Dockindicator_PaneDiamond;
                private static Bitmap _bitmapPaneDiamondLeft = Resources.Dockindicator_PaneDiamond_Fill;
                private static Bitmap _bitmapPaneDiamondRight = Resources.Dockindicator_PaneDiamond_Fill;
                private static Bitmap _bitmapPaneDiamondTop = Resources.Dockindicator_PaneDiamond_Fill;
                private static Bitmap _bitmapPaneDiamondBottom = Resources.Dockindicator_PaneDiamond_Fill;
                private static Bitmap _bitmapPaneDiamondFill = Resources.Dockindicator_PaneDiamond_Fill;
                private static Bitmap _bitmapPaneDiamondHotSpot = Resources.Dockindicator_PaneDiamond_Hotspot;
                private static Bitmap _bitmapPaneDiamondHotSpotIndex = Resources.DockIndicator_PaneDiamond_HotspotIndex;

                private static DockPanel.HotSpotIndex[] _hotSpots =
                    {
                        new DockPanel.HotSpotIndex(1, 0, DockStyle.Top),
                        new DockPanel.HotSpotIndex(0, 1, DockStyle.Left),
                        new DockPanel.HotSpotIndex(1, 1, DockStyle.Fill),
                        new DockPanel.HotSpotIndex(2, 1, DockStyle.Right),
                        new DockPanel.HotSpotIndex(1, 2, DockStyle.Bottom)
                    };

                private GraphicsPath _displayingGraphicsPath = DrawHelper.CalculateGraphicsPathFromBitmap(_bitmapPaneDiamond);

                public VS2013BluePaneIndicator()
                {
                    SizeMode = PictureBoxSizeMode.AutoSize;
                    Image = _bitmapPaneDiamond;
                    Region = new Region(DisplayingGraphicsPath);
                }

                public GraphicsPath DisplayingGraphicsPath
                {
                    get { return _displayingGraphicsPath; }
                }

                public DockStyle HitTest(Point pt)
                {
                    if (!Visible)
                        return DockStyle.None;

                    pt = PointToClient(pt);
                    if (!ClientRectangle.Contains(pt))
                        return DockStyle.None;

                    for (int i = _hotSpots.GetLowerBound(0); i <= _hotSpots.GetUpperBound(0); i++)
                    {
                        if (_bitmapPaneDiamondHotSpot.GetPixel(pt.X, pt.Y) == _bitmapPaneDiamondHotSpotIndex.GetPixel(_hotSpots[i].X, _hotSpots[i].Y))
                            return _hotSpots[i].DockStyle;
                    }

                    return DockStyle.None;
                }

                private DockStyle m_status = DockStyle.None;

                public DockStyle Status
                {
                    get { return m_status; }
                    set
                    {
                        m_status = value;
                        if (m_status == DockStyle.None)
                            Image = _bitmapPaneDiamond;
                        else if (m_status == DockStyle.Left)
                            Image = _bitmapPaneDiamondLeft;
                        else if (m_status == DockStyle.Right)
                            Image = _bitmapPaneDiamondRight;
                        else if (m_status == DockStyle.Top)
                            Image = _bitmapPaneDiamondTop;
                        else if (m_status == DockStyle.Bottom)
                            Image = _bitmapPaneDiamondBottom;
                        else if (m_status == DockStyle.Fill)
                            Image = _bitmapPaneDiamondFill;
                    }
                }
            }
        }

        private class VS2013BlueAutoHideWindowFactory : DockPanelExtender.IAutoHideWindowFactory
        {
            public DockPanel.AutoHideWindowControl CreateAutoHideWindow(DockPanel panel)
            {
                return new VS2012AutoHideWindowControl(panel);
            }
        }

        private class VS2013BlueDockPaneSplitterControlFactory : DockPanelExtender.IDockPaneSplitterControlFactory
        {
            public DockPane.SplitterControlBase CreateSplitterControl(DockPane pane)
            {
                return new VS2013BlueSplitterControl(pane);
            }
        }

        private class VS2013BlueDockWindowSplitterControlFactory : DockPanelExtender.IDockWindowSplitterControlFactory
        {
            public SplitterBase CreateSplitterControl(ISplitterHost host)
            {
                return new VS2013BlueDockWindowSplitterControl();
            }
        }

        private class VS2013BlueDockPaneStripFactory : DockPanelExtender.IDockPaneStripFactory
        {
            public DockPaneStripBase CreateDockPaneStrip(DockPane pane)
            {
                return new VS2013BlueDockPaneStrip(pane);
            }
        }

        private class VS2013BlueAutoHideStripFactory : DockPanelExtender.IAutoHideStripFactory
        {
            public AutoHideStripBase CreateAutoHideStrip(DockPanel panel)
            {
                return new VS2012AutoHideStrip(panel);
            }
        }

        private class VS2013BlueDockPaneCaptionFactory : DockPanelExtender.IDockPaneCaptionFactory
        {
            public DockPaneCaptionBase CreateDockPaneCaption(DockPane pane)
            {
                return new VS2012DockPaneCaption(pane);
            }
        }

        private class VS2013BlueDockWindowFactory : DockPanelExtender.IDockWindowFactory
        {
            public DockWindow CreateDockWindow(DockPanel dockPanel, DockState dockState)
            {
                return new VS2012DockWindow(dockPanel, dockState);
            }
        }

        public static DockPanelSkin CreateVisualStudio2013Blue()
        {
            var border      = Color.FromArgb(0xFF, 41, 57, 85);
            var specialyellow = Color.FromArgb(0xFF, 255, 242, 157);
            var hover = Color.FromArgb(0xFF, 155, 167, 183);

            var activeTab = specialyellow;
            var mouseHoverTab = Color.FromArgb(0xFF, 91, 113, 153);
            var inactiveTab = Color.FromArgb(0xFF, 54, 78, 111);
            var lostFocusTab = Color.FromArgb(0xFF, 77, 96, 130);
            var skin = new DockPanelSkin();

            skin.AutoHideStripSkin.DockStripGradient.StartColor = hover;
            skin.AutoHideStripSkin.DockStripGradient.EndColor = inactiveTab;
            skin.AutoHideStripSkin.TabGradient.TextColor = Color.White;
            skin.AutoHideStripSkin.DockStripBackground.StartColor = border;

            skin.DockPaneStripSkin.DocumentGradient.DockStripGradient.StartColor = border;
            skin.DockPaneStripSkin.DocumentGradient.DockStripGradient.EndColor = border;
            skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.StartColor = activeTab;
            skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.EndColor = lostFocusTab;
            skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient.TextColor = Color.Black;
            skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.StartColor = inactiveTab;
            skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.EndColor = mouseHoverTab;
            skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient.TextColor = Color.White;

            skin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient.StartColor = inactiveTab;
            skin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient.EndColor = inactiveTab;

            skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.StartColor = Color.White;
            skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.EndColor = Color.White;
            skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient.TextColor = Color.Black;

            skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.StartColor = border;
            skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.EndColor = border;
            skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient.TextColor = Color.White;

            skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.StartColor = specialyellow;
            skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.EndColor = specialyellow;
            //skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.LinearGradientMode = LinearGradientMode.Vertical;
            skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.TextColor = Color.Black;

            skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.StartColor = lostFocusTab;
            skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.EndColor = lostFocusTab;
            skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.LinearGradientMode = LinearGradientMode.Vertical;
            skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.TextColor = Color.White;

            skin.DockPaneStripSkin.ToolWindowGradient.HoverTabGradient.TextColor = Color.White;

            skin.DockPaneStripSkin.DocumentGradient.HoverTabGradient.StartColor = mouseHoverTab;
            skin.DockPaneStripSkin.DocumentGradient.HoverTabGradient.EndColor = mouseHoverTab;
            skin.DockPaneStripSkin.DocumentGradient.HoverTabGradient.TextColor = Color.White;

            skin.ColorPalette.MainWindowActive.Background = ColorTranslator.FromHtml("#FF293955");

            skin.ColorPalette.AutoHideStripDefault.Background = ColorTranslator.FromHtml("#FF293955");
            skin.ColorPalette.AutoHideStripDefault.Border = ColorTranslator.FromHtml("#FF465A7D");
            skin.ColorPalette.AutoHideStripDefault.Text = ColorTranslator.FromHtml("#FFFFFFFF");

            skin.ColorPalette.AutoHideStripHovered.Background = ColorTranslator.FromHtml("#FF293955");
            skin.ColorPalette.AutoHideStripHovered.Border = ColorTranslator.FromHtml("#FF9BA7B7");
            skin.ColorPalette.AutoHideStripHovered.Text = ColorTranslator.FromHtml("#FFFFFFFF");

            skin.ColorPalette.TabSelectedActive.Background = ColorTranslator.FromHtml("#FFFFF0D0");
            skin.ColorPalette.TabSelectedActive.Button = ColorTranslator.FromHtml("#FF75633D");
            skin.ColorPalette.TabSelectedActive.Text = ColorTranslator.FromHtml("#FF000000");

            skin.ColorPalette.TabSelectedInactive.Background = ColorTranslator.FromHtml("#3D5277");// TODO: from theme .FromHtml("#FF4D6082");
            skin.ColorPalette.TabSelectedInactive.Button = ColorTranslator.FromHtml("#FFCED4DD");
            skin.ColorPalette.TabSelectedInactive.Text = ColorTranslator.FromHtml("#FFFFFFFF");

            skin.ColorPalette.TabUnselected.Text = ColorTranslator.FromHtml("#FFFFFFFF");

            skin.ColorPalette.TabUnselectedHovered.Background = ColorTranslator.FromHtml("#FF4B5C74");
            skin.ColorPalette.TabUnselectedHovered.Button = ColorTranslator.FromHtml("#FFCED4DD");
            skin.ColorPalette.TabUnselectedHovered.Text = ColorTranslator.FromHtml("#FFFFFFFF");

            skin.ColorPalette.ToolWindowCaptionActive.Background = ColorTranslator.FromHtml("#FFFFF0D0");
            skin.ColorPalette.ToolWindowCaptionActive.Button = ColorTranslator.FromHtml("#FF75633D");
            skin.ColorPalette.ToolWindowCaptionActive.Grip = ColorTranslator.FromHtml("#FFFFF0D0");
            skin.ColorPalette.ToolWindowCaptionActive.Text = ColorTranslator.FromHtml("#FF000000");

            skin.ColorPalette.ToolWindowCaptionInactive.Background = ColorTranslator.FromHtml("#FF4D6082");
            skin.ColorPalette.ToolWindowCaptionInactive.Button = ColorTranslator.FromHtml("#FFCED4DD");
            skin.ColorPalette.ToolWindowCaptionInactive.Grip = ColorTranslator.FromHtml("#FF4D6082");
            skin.ColorPalette.ToolWindowCaptionInactive.Text = ColorTranslator.FromHtml("#FFFFFFFF");

            skin.ColorPalette.ToolWindowTabSelectedActive.Background = ColorTranslator.FromHtml("#FFFFFFFF");
            skin.ColorPalette.ToolWindowTabSelectedActive.Separator = ColorTranslator.FromHtml("#FF4B5C74");
            skin.ColorPalette.ToolWindowTabSelectedActive.Text = ColorTranslator.FromHtml("#FF000000");

            skin.ColorPalette.ToolWindowTabSelectedInactive.Background = ColorTranslator.FromHtml("#FFFFFFFF");
            skin.ColorPalette.ToolWindowTabSelectedInactive.Separator = ColorTranslator.FromHtml("#FF4B5C74");
            skin.ColorPalette.ToolWindowTabSelectedInactive.Text = ColorTranslator.FromHtml("#FF000000");

            skin.ColorPalette.ToolWindowTabUnselected.Separator = ColorTranslator.FromHtml("#FF4B5C74");
            skin.ColorPalette.ToolWindowTabUnselected.Text = ColorTranslator.FromHtml("#FFFFFFFF");

            skin.ColorPalette.ToolWindowTabUnselectedHovered.Background = ColorTranslator.FromHtml("#FF4B5C74");
            skin.ColorPalette.ToolWindowTabUnselectedHovered.Separator = ColorTranslator.FromHtml("#FF4B5C74");
            skin.ColorPalette.ToolWindowTabUnselectedHovered.Text = ColorTranslator.FromHtml("#FFFFFFFF");

            return skin;
        }

        internal class VS2013BlueDockWindowSplitterControl : SplitterBase
        {
            private SolidBrush _brush;

            protected override int SplitterSize
            {
                get { return Measures.SplitterSize; }
            }

            protected override void StartDrag()
            {
                DockWindow window = Parent as DockWindow;
                if (window == null)
                    return;

                window.DockPanel.BeginDrag(window, window.RectangleToScreen(Bounds));
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                Rectangle rect = ClientRectangle;

                if (rect.Width <= 0 || rect.Height <= 0)
                    return;

                DockWindow window = Parent as DockWindow;
                if (window == null)
                    return;

                if (this._brush == null)
                {
                    _brush = new SolidBrush(window.DockPanel.Theme.Skin.AutoHideStripSkin.DockStripBackground.StartColor);
                }

                switch (Dock)
                {
                    case DockStyle.Right:
                    case DockStyle.Left:
                        {
                            e.Graphics.FillRectangle(_brush, rect.X, rect.Y,
                                                             Measures.SplitterSize, rect.Height);
                        }
                        break;
                    case DockStyle.Bottom:
                    case DockStyle.Top:
                        {
                            e.Graphics.FillRectangle(_brush, rect.X, rect.Y, rect.Width, Measures.SplitterSize);
                        }
                        break;
                }

            }
        }
    }
}
