﻿using System;
using System.Collections.Generic;
using System.IO;
using ImGui.OSAbstraction.Window;
using ImGui.Rendering;
using BigGustave;

namespace ImGui
{
    /// <summary>
    /// Represents a window that makes up an application's user interface.
    /// </summary>
    public partial class Form
    {
        private readonly struct ConstructionParameters
        {
            public ConstructionParameters(string title, Point position, Size size, WindowTypes type)
            {
                Title = title;
                Position = position;
                Size = size;
                Type = type;
            }

            public string Title { get; }
            public Point Position { get; }
            public Size Size { get; }
            public WindowTypes Type { get; }
        }

        public static Form current;
        
        internal string debugName => constructionParameters.Title;
        
        private readonly ConstructionParameters constructionParameters;

        private IWindow nativeWindow;

        internal IWindow NativeWindow => nativeWindow;

        internal int ID;
        internal int Idx;
        internal long LastFrameActive;
        internal bool PlatformWindowCreated;
        internal Window Window;
        internal string LastName;
        internal Point LastPos;
        internal Size LastSize;
        internal Size LastRendererSize;
        internal Point Pos;
        internal Size Size;
        internal bool PlatformRequestMove;    // Platform window requested move (e.g. window was moved by the OS / host window manager, authoritative position will be OS window position)
        internal bool PlatformRequestResize;  // Platform window requested resize (e.g. window was resized by the OS / host window manager, authoritative size will be OS window size)
        internal bool PlatformRequestClose;   // Platform window requested closure (e.g. window was moved by the OS / host window manager, e.g. pressing ALT-F4)
        internal float Alpha;
        internal float LastAlpha;
        internal ImGuiViewportFlags Flags;// See ImGuiViewportFlags
        internal int LastFrontMostStampCount;  // Last stamp number from when a window hosted by this viewport was made front-most (by comparing this value between two viewport we have an implicit viewport z-order
        
        public Point LastPlatformPos { get; set; }
        public Size LastPlatformSize { get; set; }
        public Color BackgroundColor { get; set; } = Color.Argb(255, 114, 144, 154);

        private List<Window> windows = new List<Window>(8);
        internal MeshBuffer MeshBuffer { get; } = new MeshBuffer(); //draw data of this form


        /// <summary>
        /// Initializes a new instance of the <see cref="Form"/> class at specific rectangle.
        /// </summary>
        /// <param name="rect">initial rectangle of the form</param>
        public Form(Rect rect):this(rect.TopLeft, rect.Size)
        {
        }

        internal Form(Point position, Size size, WindowTypes type)
            : this(position, size, "ImGui Form", type)
        {

        }

        internal Form(Rect rect, string title, WindowTypes type)
            : this(rect.TopLeft, rect.Size, title, type)
        {
        }

        internal Form(Point position, Size size, string title = "ImGui Form",
            WindowTypes type = WindowTypes.Regular)
        {
            if (size.Width <=0 || size.Height <=0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
            MeshBuffer.OwnerName = title;
            constructionParameters = new ConstructionParameters(title, position, size, type);
            PlatformWindowCreated = false;
        }

        internal void InitializeForm()
        {
            Profile.Start("Initialize Form");
            this.nativeWindow = Application.PlatformContext.CreateWindow(
                constructionParameters.Position,
                constructionParameters.Size,
                constructionParameters.Type);
            Size = constructionParameters.Size;
            Pos = constructionParameters.Position;
            if (constructionParameters.Type == WindowTypes.Regular)
            {
                this.nativeWindow.Title = constructionParameters.Title;
            }
            PlatformWindowCreated = true;
            Profile.End();
        }

        internal void InitializeRenderer()
        {
            this.InitializeBackForegroundRenderContext();
        }

        internal void MainLoop(Action loopMethod)
        {
            this.nativeWindow.MainLoop(loopMethod);
        }

        #region window management

        internal bool Closed { get; private set; }

        internal IntPtr Pointer => this.nativeWindow.Pointer;

        internal Size PlatformSize
        {
            get => this.nativeWindow.Size;
            set => this.nativeWindow.Size = value;
        }

        internal Point ClientPosition
        {
            get => this.nativeWindow.ClientPosition;
            set => this.nativeWindow.ClientPosition = value;
        }

        internal Size ClientSize
        {
            get => this.nativeWindow.ClientSize;
            set => this.nativeWindow.ClientSize = value;
        }

        internal Rect ClientRect => new Rect(ClientPosition, ClientSize);

        internal Point PlatformPosition
        {
            get => this.nativeWindow.Position;
            set => this.nativeWindow.Position = value;
        }

        internal Rect Rect => new Rect(this.PlatformPosition, this.Size);

        internal void Show()
        {
            this.nativeWindow.Show();
        }

        internal void Hide()
        {
            this.nativeWindow.Hide();
        }

        internal void Close()
        {
            this.nativeWindow.Close();
            this.Closed = true;
        }

        internal void Platform_SetWindowTitle(string title)
        {
            nativeWindow.Title = title;
        }

        #endregion

        internal void Renderer_SetWindowSize(Size size)
        {
            Application.renderer.OnSizeChanged(size);
        }

        internal Point ScreenToClient(Point point)
        {
            return this.nativeWindow.ScreenToClient(point);
        }

        internal Point ClientToScreen(Point point)
        {
            return this.nativeWindow.ClientToScreen(point);
        }

        internal void Dev_SaveClientAreaToPng(string filePath)
        {
            byte[] data = Application.renderer.GetRawBackBuffer(out var width, out var height);
            var image = PngBuilder.Create(data, width, height, true);
            using var stream = File.OpenWrite(filePath);
            image.Save(stream);
        }

        public bool IsMinimized => nativeWindow.Minimized;

        public void Platform_SetWindowAlpha(float alpha)
        {
            nativeWindow.Opacity = alpha;
        }

        public void ClearRequestFlags()
        {
            PlatformRequestClose = PlatformRequestMove = PlatformRequestResize = false;
        }

        public bool Platform_GetWindowFocus()
        {
            return nativeWindow.GetFocus();
        }
    }

    [Flags]
    enum ImGuiViewportFlags
    {
        None                     = 0,
        NoDecoration             = 1 << 0,   // Platform Window: Disable platform decorations: title bar, borders, etc. (generally set all windows, but if ImGuiConfigFlags_ViewportsDecoration is set we only set this on popups/tooltips)
        NoTaskBarIcon            = 1 << 1,   // Platform Window: Disable platform task bar icon (generally set on popups/tooltips, or all windows if ImGuiConfigFlags_ViewportsNoTaskBarIcon is set)
        NoFocusOnAppearing       = 1 << 2,   // Platform Window: Don't take focus when created.
        NoFocusOnClick           = 1 << 3,   // Platform Window: Don't take focus when clicked on.
        NoInputs                 = 1 << 4,   // Platform Window: Make mouse pass through so we can drag this window while peaking behind it.
        NoRendererClear          = 1 << 5,   // Platform Window: Renderer doesn't need to clear the framebuffer ahead (because we will fill it entirely).
        TopMost                  = 1 << 6,   // Platform Window: Display on top (for tooltips only).
        Minimized                = 1 << 7,   // Platform Window: Window is minimized, can skip render. When minimized we tend to avoid using the viewport pos/size for clipping window or testing if they are contained in the viewport.
        NoAutoMerge              = 1 << 8,   // Platform Window: Avoid merging this window into another host window. This can only be set via ImGuiWindowClass viewport flags override (because we need to now ahead if we are going to create a viewport in the first place!).
        CanHostOtherWindows      = 1 << 9    // Main viewport: can host multiple imgui windows (secondary viewports are associated to a single window).
    }

}