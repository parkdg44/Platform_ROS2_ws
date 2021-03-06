// License: Apache 2.0. See LICENSE file in root directory.
// Copyright(c) 2017 Intel Corporation. All Rights Reserved.

namespace Intel.RealSense
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using Base;

    /// <summary>
    /// Class to store the profile of stream
    /// </summary>
    public class StreamProfile : Base.PooledObject
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal static readonly Base.Deleter StreamProfileReleaser = NativeMethods.rs2_delete_stream_profile;

        internal override void Initialize()
        {
            object error;
            NativeMethods.rs2_get_stream_profile_data(Handle, out stream, out format, out index, out uniqueId, out framerate, out error);
            IsDefault = NativeMethods.rs2_is_stream_profile_default(Handle, out error) > 0;
        }

        internal StreamProfile(IntPtr ptr)
            : base(ptr, null)
        {
            this.Initialize();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (clone != null)
                {
                    clone.Dispose();
                    clone = null;
                }
            }

            base.Dispose(disposing);
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Stream stream;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Format format;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int framerate;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int index;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int uniqueId;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal DeleterHandle clone;

        /// <summary>
        /// Gets the stream type of the profile
        /// </summary>
        public Stream Stream
        {
            get { return stream; }
        }

        /// <summary>
        /// Gets the binary data format of the profile
        /// </summary>
        public Format Format
        {
            get { return format; }
        }

        /// <summary>
        /// Gets the expected rate for data frames to arrive, meaning expected number of frames per second
        /// </summary>
        public int Framerate
        {
            get { return framerate; }
        }

        /// <summary>
        /// Gets the stream index the input profile in case there are multiple streams of the same type
        /// </summary>
        public int Index
        {
            get { return index; }
        }

        /// <summary>
        /// Gets the identifier for the stream profile, unique within the application
        /// </summary>
        public int UniqueID
        {
            get { return uniqueId; }
        }

        /// <summary>
        /// Gets a value indicating whether the profile is recommended for the sensor
        /// <para>
        /// This is an optional hint we offer to suggest profiles with best performance-quality tradeof
        /// </para>
        /// </summary>
        public bool IsDefault { get; private set; }

        public bool IsCloned => clone?.IsInvalid == false;

        /// <summary>
        /// Gets the extrinsics from this profile to the other
        /// </summary>
        /// <param name="other">target stream profile</param>
        /// <returns>extrinsics from this to target</returns>
        public Extrinsics GetExtrinsicsTo(StreamProfile other)
        {
            object error;
            Extrinsics extrinsics;
            NativeMethods.rs2_get_extrinsics(Handle, other.Handle, out extrinsics, out error);
            return extrinsics;
        }

        public void RegisterExtrinsicsTo(StreamProfile other, Extrinsics extrinsics)
        {
            object error;
            NativeMethods.rs2_register_extrinsics(Handle, other.Handle, extrinsics, out error);
        }

        /// <summary>
        /// Clone the current profile and change the type, index and format to input parameters
        /// </summary>
        /// <param name="type">will change the stream type from the cloned profile.</param>
        /// <param name="index">will change the stream index from the cloned profile.</param>
        /// <param name="format">will change the stream format from the cloned profile.</param>
        /// <returns>the cloned stream profile.</returns>
        public StreamProfile Clone(Stream type, int index, Format format)
        {
            object error;
            var ptr = NativeMethods.rs2_clone_stream_profile(Handle, type, index, format, out error);
            var p = StreamProfile.Create(ptr);
            p.clone = new DeleterHandle(ptr, StreamProfileReleaser);
            return p;
        }

        /// <summary>
        /// Try to extend stream profile to an extension type
        /// </summary>
        /// <param name="e">extension type</param>
        /// <returns>true if profile is extendable to specified extension</returns>
        public bool Is(Extension e)
        {
            object error;
            return NativeMethods.rs2_stream_profile_is(Handle, e, out error) > 0;
        }

        public T As<T>()
            where T : StreamProfile
        {
            return Create<T>(Handle);
        }

        public T Cast<T>()
            where T : StreamProfile
        {
            using (this)
            {
                return Create<T>(Handle);
            }
        }

        internal static T Create<T>(IntPtr ptr)
            where T : StreamProfile
        {
            return ObjectPool.Get<T>(ptr);
        }

        internal static StreamProfile Create(IntPtr ptr)
        {
            return Create<StreamProfile>(ptr);
        }
    }
}
