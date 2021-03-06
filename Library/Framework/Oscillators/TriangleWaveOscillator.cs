﻿namespace Macabre2D.Framework {

    using System;

    /// <summary>
    /// An oscillator that uses triangle waves to create sound.
    /// </summary>
    /// <seealso cref="Macabre2D.Framework.IOscillator"/>
    public sealed class TriangleWaveOscillator : IOscillator {

        /// <inheritdoc/>
        public float GetSignal(float time, float frequency, float amplitude) {
            return 2f * amplitude * (float)Math.Abs(2f * (time * frequency - Math.Floor(time * frequency + 0.5f))) - amplitude;
        }
    }
}