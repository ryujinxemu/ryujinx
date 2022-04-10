﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.OpenGL;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using OpenTK.Graphics.OpenGL;
using Ryujinx.Ava.Ui.Backend.OpenGl;
using Ryujinx.Common.Configuration;
using SkiaSharp;
using SPB.Graphics;
using SPB.Graphics.OpenGL;
using SPB.Platform;
using SPB.Windowing;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace Ryujinx.Ava.Ui.Controls
{
    public class RendererControl : Control
    {
        protected int Image { get; set; }

        public event EventHandler<EventArgs> GlInitialized;
        public event EventHandler<Size> SizeChanged;

        protected Size RenderSize { get;private set; }
        public bool IsStarted { get; private set; }

        public int Major { get; }
        public int Minor { get; }
        public GraphicsDebugLevel DebugLevel { get; }
        public OpenGLContextBase GameContext { get; set; }

        public OpenGLContextBase PrimaryContext =>
                AvaloniaLocator.Current.GetService<OpenGLContextBase>();

        private SwappableNativeWindowBase _gameBackgroundWindow;

        private bool _isInitialized;
        private IntPtr _fence;

        private GlDrawOperation _glDrawOperation;
        private AutoResetEvent _swapEvent;

        public RendererControl(int major, int minor, GraphicsDebugLevel graphicsDebugLevel)
        {
            Major = major;
            Minor = minor;
            DebugLevel = graphicsDebugLevel;
            IObservable<Rect> resizeObservable = this.GetObservable(BoundsProperty);

            resizeObservable.Subscribe(Resized);

            Focusable = true;
        }

        private void Resized(Rect rect)
        {
            SizeChanged?.Invoke(this, rect.Size);

            RenderSize = rect.Size * Program.WindowScaleFactor;

            _glDrawOperation?.Dispose();
            _glDrawOperation = new GlDrawOperation(this);
        }

        public override void Render(DrawingContext context)
        {
            if (!_isInitialized)
            {
                Task.Run(() =>
                {
                    CreateWindow();
                }).Wait();

                OnGlInitialized();
                _isInitialized = true;
            }

            if (GameContext == null || !IsStarted || Image == 0)
            {
                return;
            }

            if (_glDrawOperation != null)
            {
                context.Custom(_glDrawOperation);
            }

            base.Render(context);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            _swapEvent = OpenGlSurface.GetWindowSwapEvent(((this.VisualRoot as TopLevel).PlatformImpl as IWindowImpl).Handle.Handle);
        }

        protected void OnGlInitialized()
        {
            GlInitialized?.Invoke(this, EventArgs.Empty);
        }

        public void QueueRender()
        {
            try
            {
                InvalidateVisual();
            }
            catch (Exception) { }

            Program.RenderTimer.TickNow();
        }

        internal bool Present(int image)
        {
            Image = image;

            if (_fence != IntPtr.Zero)
            {
                GL.DeleteSync(_fence);
            }

            _fence = GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);

            QueueRender();

            _swapEvent?.WaitOne();

            return true;
        }

        internal void Start()
        {
            IsStarted = true;
            QueueRender();
        }

        internal void Stop()
        {
            IsStarted = false;
        }

        public void DestroyBackgroundContext()
        {
            Image = 0;

            if (_fence != IntPtr.Zero)
            {
                _glDrawOperation.Dispose();
                GL.DeleteSync(_fence);
            }

            GameContext?.Dispose();

            _gameBackgroundWindow?.Dispose();
        }

        internal void MakeCurrent()
        {
            GameContext.MakeCurrent(_gameBackgroundWindow);
        }

        internal void MakeCurrent(SwappableNativeWindowBase window)
        {
            GameContext.MakeCurrent(window);
        }

        protected void CreateWindow()
        {
            var flags = OpenGLContextFlags.Compat;
            if(DebugLevel != GraphicsDebugLevel.None)
            {
                flags |= OpenGLContextFlags.Debug;
            }
            _gameBackgroundWindow = PlatformHelper.CreateOpenGLWindow(FramebufferFormat.Default, 0, 0, 100, 100);
            _gameBackgroundWindow.Hide();

            GameContext = PlatformHelper.CreateOpenGLContext(FramebufferFormat.Default, Major, Minor, flags, shareContext: PrimaryContext);
            GameContext.Initialize(_gameBackgroundWindow);
            MakeCurrent();
            GL.LoadBindings(new OpenToolkitBindingsContext(GameContext.GetProcAddress));
            MakeCurrent(null);
        }

        private class GlDrawOperation : ICustomDrawOperation
        {
            private int _framebuffer;

            public Rect Bounds { get; }

            private readonly RendererControl _control;

            public GlDrawOperation(RendererControl control)
            {
                _control = control;
                Bounds = _control.Bounds;
            }

            public void Dispose()
            {
                GL.DeleteFramebuffer(_framebuffer);
            }

            public bool Equals(ICustomDrawOperation other)
            {
                return other is GlDrawOperation operation && Equals(this, operation) && operation.Bounds == Bounds;
            }

            public bool HitTest(Point p)
            {
                return Bounds.Contains(p);
            }

            private void CreateRenderTarget()
            {
                _framebuffer = GL.GenFramebuffer();
            }

            public void Render(IDrawingContextImpl context)
            {
                if (_control.Image == 0)
                    return;

                if (_framebuffer == 0)
                {
                    CreateRenderTarget();
                }

                int currentFramebuffer = GL.GetInteger(GetPName.FramebufferBinding);

                var image = _control.Image;
                var fence = _control._fence;

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, image, 0);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, currentFramebuffer);

                if (context is not ISkiaDrawingContextImpl skiaDrawingContextImpl)
                    return;

                var imageInfo = new SKImageInfo((int)_control.RenderSize.Width, (int)_control.RenderSize.Height, SKColorType.Rgba8888);
                var glInfo = new GRGlFramebufferInfo((uint)_framebuffer, SKColorType.Rgba8888.ToGlSizedFormat());

                GL.WaitSync(fence, WaitSyncFlags.None, ulong.MaxValue);

                using (var backendTexture = new GRBackendRenderTarget(imageInfo.Width, imageInfo.Height, 1, 0, glInfo))
                using (var surface = SKSurface.Create(skiaDrawingContextImpl.GrContext, backendTexture,
                    GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888))
                {
                    if (surface == null)
                        return;

                    var rect = new Rect(new Point(), _control.RenderSize);

                    using (var snapshot = surface.Snapshot())
                        skiaDrawingContextImpl.SkCanvas.DrawImage(snapshot, rect.ToSKRect(), _control.Bounds.ToSKRect(), new SKPaint());
                }
            }
        }
    }
}
