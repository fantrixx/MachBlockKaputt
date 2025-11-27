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
        public SoundEffect? CashRegisterSound { get; private set; }
        public SoundEffect? ChargeUpSound { get; private set; }

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
                CashRegisterSound = CreateCashRegisterSound();
                ChargeUpSound = CreateChargeUpSound();
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
        public void PlayCashRegister() => CashRegisterSound?.Play();
        public void PlayChargeUp() => ChargeUpSound?.Play();

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
            float durationSeconds = 0.25f;
            int samples = (int)(sampleRate * durationSeconds);
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            WriteWavHeader(bw, samples);

            double amplitude = 32760 * 0.8f;
            double t = 0;
            double dt = 1.0 / sampleRate;
            
            for (int i = 0; i < samples; i++)
            {
                double progress = t / durationSeconds;
                double env = Math.Exp(-6.0 * progress);
                
                // Tiefere Frequenzen für "Boom"-Effekt
                double boom = Math.Sin(2.0 * Math.PI * 80 * t) * 0.5;
                double crackle = Math.Sin(2.0 * Math.PI * 200 * t) * 0.3;
                
                // Mehr Rauschen für Explosions-Charakter
                double noise = (_random.NextDouble() - 0.5) * 0.4;
                double mixed = (boom + crackle + noise * 0.5) * env;
                
                short sample = (short)(amplitude * mixed);
                bw.Write(sample);
                t += dt;
            }

            bw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return SoundEffect.FromStream(ms);
        }

        private SoundEffect CreateCashRegisterSound()
        {
            const int sampleRate = 44100;
            float durationSeconds = 0.35f;
            int samples = (int)(sampleRate * durationSeconds);
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            WriteWavHeader(bw, samples);

            double amplitude = 32760 * 0.6f;
            double t = 0;
            double dt = 1.0 / sampleRate;
            
            for (int i = 0; i < samples; i++)
            {
                double progress = t / durationSeconds;
                
                // "Tsching" Sound: Hohe Frequenz mit schnellem Attack
                double env1 = progress < 0.05 ? progress / 0.05 : Math.Exp(-5.0 * (progress - 0.05));
                double bell = Math.Sin(2.0 * Math.PI * 2000 * t) * env1 * 0.5;
                
                // Zweiter "Kling" etwas später
                double env2 = progress < 0.15 ? 0 : 
                             progress < 0.20 ? (progress - 0.15) / 0.05 : 
                             Math.Exp(-4.0 * (progress - 0.20));
                double bell2 = Math.Sin(2.0 * Math.PI * 1500 * t) * env2 * 0.4;
                
                // Metallisches Klirren
                double metallic = Math.Sin(2.0 * Math.PI * 3000 * t) * env1 * 0.2;
                
                double mixed = bell + bell2 + metallic;
                
                short sample = (short)(amplitude * mixed);
                bw.Write(sample);
                t += dt;
            }

            bw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return SoundEffect.FromStream(ms);
        }

        private SoundEffect CreateChargeUpSound()
        {
            const int sampleRate = 44100;
            float durationSeconds = 1.5f;
            int samples = (int)(sampleRate * durationSeconds);
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            WriteWavHeader(bw, samples);

            double amplitude = 32760 * 0.5f;
            double t = 0;
            double dt = 1.0 / sampleRate;
            
            // Schnelle Klicks wie beim Geldzählen
            double clickRate = 20; // 20 Klicks pro Sekunde
            
            for (int i = 0; i < samples; i++)
            {
                double currentClickTime = Math.Floor(t * clickRate);
                double clickProgress = (t * clickRate) - currentClickTime;
                
                // Kurzer Attack für Klick-Geräusch
                double clickEnv = 0;
                if (clickProgress < 0.15)
                {
                    clickEnv = Math.Exp(-30.0 * clickProgress);
                }
                
                // Gemischte Frequenzen für papierartiges Geld-Geräusch
                double paper1 = Math.Sin(2.0 * Math.PI * 800 * t) * clickEnv;
                double paper2 = Math.Sin(2.0 * Math.PI * 1200 * t) * clickEnv * 0.6;
                double paper3 = Math.Sin(2.0 * Math.PI * 1600 * t) * clickEnv * 0.4;
                
                // Leichtes Rauschen für Papier-Textur
                double noise = (_random.NextDouble() - 0.5) * 0.2 * clickEnv;
                
                double mixed = paper1 + paper2 + paper3 + noise;
                
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
