﻿// This code was generated from the template provided from craftworkgames MonoGame.WpfCore: github.com/craftworkgames/MonoGame.WpfCore

namespace Macabre2D.Wpf.MonoGameIntegration {

    using Microsoft.Xna.Framework.Graphics;
    using SharpDX.Direct3D9;
    using System;
    using System.Windows;
    using System.Windows.Interop;

    public class MonoGameGraphicsDeviceService : IGraphicsDeviceService, IDisposable {
        private bool _isDisposed = false;

        // Store the current device settings.
        private PresentationParameters _parameters;

        public MonoGameGraphicsDeviceService() {
        }

        public event EventHandler<EventArgs> DeviceCreated;

        public event EventHandler<EventArgs> DeviceDisposing;

        public event EventHandler<EventArgs> DeviceReset;

        public event EventHandler<EventArgs> DeviceResetting;

        public Direct3DEx Direct3DContext { get; private set; }

        public DeviceEx Direct3DDevice { get; private set; }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public GraphicsDevice CreateGraphicsDevice(IntPtr windowHandle, int width, int height) {
            this._parameters = new PresentationParameters {
                BackBufferWidth = Math.Max(width, 1),
                BackBufferHeight = Math.Max(height, 1),
                BackBufferFormat = SurfaceFormat.Color,
                DepthStencilFormat = DepthFormat.Depth24,
                DeviceWindowHandle = windowHandle,
                PresentationInterval = Microsoft.Xna.Framework.Graphics.PresentInterval.Immediate,
                IsFullScreen = false
            };

            return new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, this._parameters);
        }

        public void Dispose() {
            this.Dispose(true);
        }

        /// <summary>
        /// Resets the graphics device to whichever is bigger out of the specified resolution or its
        /// current size. This behavior means the device will demand-grow to the largest of all its
        /// GraphicsDeviceControl clients.
        /// </summary>
        public void ResetDevice(int width, int height) {
            var newWidth = Math.Max(this._parameters.BackBufferWidth, width);
            var newHeight = Math.Max(this._parameters.BackBufferHeight, height);

            if (newWidth != this._parameters.BackBufferWidth || newHeight != this._parameters.BackBufferHeight) {
                DeviceResetting?.Invoke(this, EventArgs.Empty);

                this._parameters.BackBufferWidth = newWidth;
                this._parameters.BackBufferHeight = newHeight;

                this.GraphicsDevice.Reset(this._parameters);

                this.DeviceReset?.Invoke(this, EventArgs.Empty);
            }
        }

        public void StartDirect3D(Window window) {
            this.Direct3DContext = new Direct3DEx();

            var presentParameters = new PresentParameters {
                Windowed = true,
                SwapEffect = SwapEffect.Discard,
                DeviceWindowHandle = new WindowInteropHelper(window).Handle,
                PresentationInterval = SharpDX.Direct3D9.PresentInterval.Default
            };

            this.Direct3DDevice = new DeviceEx(this.Direct3DContext, 0, DeviceType.Hardware, IntPtr.Zero,
                CreateFlags.HardwareVertexProcessing | CreateFlags.Multithreaded | CreateFlags.FpuPreserve,
                presentParameters);

            // Create the device using the main window handle, and a placeholder size (1,1). The
            // actual size doesn't matter because whenever we render using this GraphicsDevice, we
            // will make sure the back buffer is large enough for the window we're rendering into.
            // Also, the handle doesn't matter because we call GraphicsDevice.Present(...) with the
            // actual window handle to render into.
            this.GraphicsDevice = this.CreateGraphicsDevice(new WindowInteropHelper(window).Handle, 1, 1);
            this.DeviceCreated?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void Dispose(bool disposing) {
            if (!this._isDisposed) {
                if (disposing) {
                    DeviceDisposing?.Invoke(this, EventArgs.Empty);
                    this.GraphicsDevice.Dispose();
                    this.Direct3DDevice?.Dispose();
                    this.Direct3DContext?.Dispose();
                }

                this._isDisposed = true;
            }
        }
    }
}