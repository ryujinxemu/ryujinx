﻿using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using OpenTK.Input;
using Ryujinx.Profiler;
using Ryujinx.Profiler.UI.SharpFontHelpers;

namespace Ryujinx
{
    public class ProfileWindow : GameWindow
    {
        private bool visible = true, initComplete = false;
        public bool visibleChanged;
        private FontService fontService;
        private Dictionary<ProfileConfig, TimingInfo> profileData;

        private float scrollPos = 0;
        private float maxScroll = 0;

        public ProfileWindow()
            : base(400, 720)
        {
            //Keyboard.KeyDown += Keyboard_KeyDown;
            Location = new Point(DisplayDevice.Default.Width - 400, (DisplayDevice.Default.Height - 720) / 2);
            Title = "Profiler";
        }

        #region Public Methods
        public void ToggleVisible()
        {
            visible = !visible;
            visibleChanged = true;
        }
        #endregion

        #region OnLoad
        /// <summary>
        /// Setup OpenGL and load resources
        /// </summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color.MidnightBlue);
            fontService = new FontService();
            fontService.InitalizeTextures();
        }
        #endregion

        #region OnResize
        /// <summary>
        /// Respond to resize events
        /// </summary>
        /// <param name="e">Contains information on the new GameWindow size.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, 0, Height, 0.0, 4.0);
        }
        #endregion

        #region OnClose
        /// <summary>
        /// Intercept close event and hide instead
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            visible = false;
            visibleChanged = true;
            e.Cancel = true;
            base.OnClosing(e);
        }
        #endregion

        #region OnUpdateFrame
        /// <summary>
        /// Profile Update Loop
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            initComplete = true;
            profileData = Profile.GetProfilingData();
        }
        #endregion

        #region OnRenderFrame
        /// <summary>
        /// Profile Render Loop
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        /// <remarks>There is no need to call the base implementation.</remarks>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (visibleChanged)
            {
                Visible = visible;
                visibleChanged = false;
            }

            if (!visible || !initComplete)
            {
                return;
            }

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.ClearColor(Color.Black);
            fontService.fontColor = Color.White;
            int verticalIndex = 0;
            int lineHeight = 12;
            int linePadding = 2;

            float maxWidth = 0;
            float yOffset = scrollPos;

            // Background lines to make reading easier
            GL.Begin(PrimitiveType.Triangles);
            GL.Color3(0.2f, 0.2f, 0.2f);
            for (int i = 0; i < profileData.Count; i += 2)
            {
                float top = yOffset + Height - linePadding - (lineHeight + linePadding) * (i + 1);
                float bottom = yOffset + Height - (linePadding * 2) - (lineHeight + linePadding) * i;
                GL.Vertex2(0, bottom);
                GL.Vertex2(0, top);
                GL.Vertex2(Width, top);

                GL.Vertex2(Width, top);
                GL.Vertex2(Width, bottom);
                GL.Vertex2(0, bottom);
            }
            GL.End();
            maxScroll = (lineHeight + linePadding) * (profileData.Count - 1);

            // Display tags
            foreach (var entry in profileData)
            {
                float y = yOffset + Height - (lineHeight + linePadding) * (verticalIndex++ + 1);
                float width = fontService.DrawText(entry.Key.Tag, 50, y, lineHeight);
                if (width > maxWidth)
                {
                    maxWidth = width;
                }
            }

            // Display timestamps
            verticalIndex = 0;
            foreach (var entry in profileData)
            {
                float y = yOffset + Height - (lineHeight + 2) * (verticalIndex++ + 1);
                fontService.DrawText($"{entry.Value.AverageTime:F3}", 75 + maxWidth, y, lineHeight);
                fontService.DrawText($"{entry.Value.LastTime:F3}", 175 + maxWidth, y, lineHeight);
            }

            SwapBuffers();
        }
        #endregion

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            scrollPos += e.Delta * -30;
            if (scrollPos < 0)
                scrollPos = 0;
            if (scrollPos > maxScroll)
                scrollPos = maxScroll;
        }
    }
}