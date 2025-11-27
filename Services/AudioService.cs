using Microsoft.Xna.Framework.Audio;
using System;
using System.IO;

namespace AlleywayMonoGame.Services
{
    /// <summary>
    /// Service responsible for creating and playing audio effects.
    /// </summary>
    public class AudioService
    {
        private readonly Random _random;
        
        public SoundEffect? ExplosionSound { get; private set; }
        public SoundEffect? PaddleSound { get; private set; }
        public SoundEffect? RocketSound { get; private set; }
        public SoundEffect? ProjectileExplosionSound { get; private set; }

        public AudioService()
        {
            _random = new Random();
            InitializeSounds();
        }

        private void InitializeSounds()
        {
            try
            {
                ExplosionSound = CreateExplosionSoundEffect(440, 0.12f, 0.6f);
                PaddleSound = CreateExplosionSoundEffect(1000, 0.06f, 0.85f);
                RocketSound = CreateRocketSoundEffect();
                ProjectileExplosionSound = CreateProjectileExplosionSound();
            }
            catch
            {
                // Sound creation failed - continue without sound
            }
        }

        public void PlayExplosion() => ExplosionSound?.Play();
        public void PlayPaddleHit() => PaddleSound?.Play();
        public void PlayRocketLaunch() => RocketSound?.Play();
        public void PlayProjectileExplosion() => ProjectileExplosionSound?.Play();

        private SoundEffect CreateExplosionSoundEffect(int frequency, float durationSeconds, float volume)
        {
            const int sampleRate = 44100;
            int samples = (int)(sampleRate * durationSeconds);
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            WriteWavHeader(bw, samples);

            double amplitude = 32760 * volume;
            double t = 0;
            double dt = 1.0 / sampleRate;
            
            for (int i = 0; i < samples; i++)
            {
                double env = Math.Exp(-3.0 * t);
                short sample = (short)(amplitude * env * Math.Sin(2.0 * Math.PI * frequency * t));
                bw.Write(sample);
                t += dt;
            }

            bw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return SoundEffect.FromStream(ms);
        }

        private SoundEffect CreateRocketSoundEffect()
        {
            const int sampleRate = 44100;
            float durationSeconds = 0.3f;
            int samples = (int)(sampleRate * durationSeconds);
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            WriteWavHeader(bw, samples);

            double amplitude = 32760 * 0.7f;
            double t = 0;
            double dt = 1.0 / sampleRate;
            
            for (int i = 0; i < samples; i++)
            {
                double progress = t / durationSeconds;
                double frequency = 80 + progress * 200;
                
                double env = progress < 0.1 ? progress / 0.1 
                    : progress < 0.7 ? 1.0 
                    : 1.0 - ((progress - 0.7) / 0.3);
                
                double sineWave = Math.Sin(2.0 * Math.PI * frequency * t);
                double noise = (_random.NextDouble() - 0.5) * 0.3;
                double mixed = (sineWave * 0.7 + noise * 0.3) * env;
                
                short sample = (short)(amplitude * mixed);
                bw.Write(sample);
                t += dt;
            }

            bw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return SoundEffect.FromStream(ms);
        }

        private SoundEffect CreateProjectileExplosionSound()
        {
            const int sampleRate = 44100;
            float durationSeconds = 0.08f;
            int samples = (int)(sampleRate * durationSeconds);
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            WriteWavHeader(bw, samples);

            double amplitude = 32760 * 0.5f;
            double t = 0;
            double dt = 1.0 / sampleRate;
            
            for (int i = 0; i < samples; i++)
            {
                double progress = t / durationSeconds;
                double env = Math.Exp(-8.0 * progress);
                
                double wave = Math.Sin(2.0 * Math.PI * 300 * t) * 0.6 + 
                             Math.Sin(2.0 * Math.PI * 180 * t) * 0.4;
                
                double noise = (_random.NextDouble() - 0.5) * 0.15;
                double mixed = (wave * 0.85 + noise * 0.15) * env;
                
                short sample = (short)(amplitude * mixed);
                bw.Write(sample);
                t += dt;
            }

            bw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return SoundEffect.FromStream(ms);
        }

        private void WriteWavHeader(BinaryWriter bw, int samples)
        {
            short bitsPerSample = 16;
            short channels = 1;
            int sampleRate = 44100;
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            short blockAlign = (short)(channels * bitsPerSample / 8);

            bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            bw.Write((int)(36 + samples * channels * bitsPerSample / 8));
            bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            bw.Write((int)16);
            bw.Write((short)1);
            bw.Write(channels);
            bw.Write(sampleRate);
            bw.Write(byteRate);
            bw.Write(blockAlign);
            bw.Write(bitsPerSample);
            bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            bw.Write((int)(samples * channels * bitsPerSample / 8));
        }
    }
}
