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
        public SoundEffect? WallBounceSound { get; private set; }
        public SoundEffect? PaddleBounceSound { get; private set; }
        public SoundEffect? RocketSound { get; private set; }
        public SoundEffect? ProjectileExplosionSound { get; private set; }
        public SoundEffect? CashRegisterSound { get; private set; }
        public SoundEffect? ChargeUpSound { get; private set; }
        public SoundEffect? PowerUpSound { get; private set; }
        public SoundEffect? GameOverSound { get; private set; }
        public SoundEffect? LevelCompleteSound { get; private set; }
        public SoundEffect? VictorySound { get; private set; }
        public SoundEffect? PaddleEnlargeSound { get; private set; }
        public SoundEffect? PaddleShrinkSound { get; private set; }

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
                WallBounceSound = CreateWallBounceSound();
                PaddleBounceSound = CreatePaddleBounceSound();
                RocketSound = CreateRocketSoundEffect();
                ProjectileExplosionSound = CreateProjectileExplosionSound();
                CashRegisterSound = CreateCashRegisterSound();
                ChargeUpSound = CreateChargeUpSound();
                PowerUpSound = CreatePowerUpSound();
                GameOverSound = CreateGameOverSound();
                LevelCompleteSound = CreateLevelCompleteSound();
                VictorySound = CreateVictorySound();
                PaddleEnlargeSound = CreatePaddleEnlargeSound();
                PaddleShrinkSound = CreatePaddleShrinkSound();
            }
            catch
            {
                // Sound creation failed - continue without sound
            }
        }

        public void PlayExplosion() => ExplosionSound?.Play();
        public void PlayPaddleHit() => PaddleSound?.Play();
        public void PlayWallBounce() => WallBounceSound?.Play();
        public void PlayPaddleBounce() => PaddleBounceSound?.Play();
        public void PlayRocketLaunch() => RocketSound?.Play();
        public void PlayProjectileExplosion() => ProjectileExplosionSound?.Play();
        public void PlayCashRegister() => CashRegisterSound?.Play();
        public void PlayChargeUp() => ChargeUpSound?.Play();
        public void PlayPowerUp() => PowerUpSound?.Play();
        public void PlayGameOver() => GameOverSound?.Play();
        public void PlayLevelComplete() => LevelCompleteSound?.Play();
        public void PlayVictory() => VictorySound?.Play();
        public void PlayPaddleEnlarge() => PaddleEnlargeSound?.Play();
        public void PlayPaddleShrink() => PaddleShrinkSound?.Play();

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

        private SoundEffect CreateWallBounceSound()
        {
            const int sampleRate = 44100;
            float durationSeconds = 0.08f;
            int samples = (int)(sampleRate * durationSeconds);
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            WriteWavHeader(bw, samples);

            double amplitude = 32760 * 0.4f;
            double t = 0;
            double dt = 1.0 / sampleRate;
            
            for (int i = 0; i < samples; i++)
            {
                double progress = t / durationSeconds;
                
                // Dumpfer, tiefer Sound für Wand
                double env = Math.Exp(-15.0 * progress);
                
                // Tiefe Frequenz (150 Hz) für dumpfen Charakter
                double mainTone = Math.Sin(2.0 * Math.PI * 150 * t);
                
                // Leichtes Obertonrauschen
                double overtone = Math.Sin(2.0 * Math.PI * 300 * t) * 0.3;
                
                double mixed = (mainTone + overtone) * env;
                
                short sample = (short)(amplitude * mixed);
                bw.Write(sample);
                t += dt;
            }

            bw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return SoundEffect.FromStream(ms);
        }

        private SoundEffect CreatePaddleBounceSound()
        {
            const int sampleRate = 44100;
            float durationSeconds = 0.1f;
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
                
                // Dumpfer "Thud"-Sound für Paddle
                double env = Math.Exp(-12.0 * progress);
                
                // Mittlere Frequenz (200 Hz) für "Thud"
                double mainTone = Math.Sin(2.0 * Math.PI * 200 * t);
                
                // Obertöne für Holz/Plastik-Charakter
                double overtone1 = Math.Sin(2.0 * Math.PI * 400 * t) * 0.4;
                double overtone2 = Math.Sin(2.0 * Math.PI * 600 * t) * 0.2;
                
                double mixed = (mainTone + overtone1 + overtone2) * env;
                
                short sample = (short)(amplitude * mixed);
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

        private SoundEffect CreatePowerUpSound()
        {
            const int sampleRate = 44100;
            float durationSeconds = 0.6f;
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
                
                // Aufsteigende Tonleiter wie bei Mario Power-Up
                // C, E, G, C (höher) - Dur-Akkord
                double[] frequencies = { 523.25, 659.25, 783.99, 1046.50 }; // C5, E5, G5, C6
                int noteIndex = (int)(progress * 4);
                if (noteIndex >= 4) noteIndex = 3;
                
                double frequency = frequencies[noteIndex];
                
                // Envelope für jeden Ton
                double noteProgress = (progress * 4) % 1.0;
                double env = Math.Exp(-3.0 * noteProgress);
                
                // Hauptton mit leichter Obertonreihe für reicheren Klang
                double tone1 = Math.Sin(2.0 * Math.PI * frequency * t) * 0.6;
                double tone2 = Math.Sin(2.0 * Math.PI * frequency * 2 * t) * 0.3; // Oktave
                double tone3 = Math.Sin(2.0 * Math.PI * frequency * 3 * t) * 0.1; // Quinte
                
                double mixed = (tone1 + tone2 + tone3) * env;
                
                short sample = (short)(amplitude * mixed);
                bw.Write(sample);
                t += dt;
            }

            bw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return SoundEffect.FromStream(ms);
        }

        private SoundEffect CreateGameOverSound()
        {
            const int sampleRate = 44100;
            float durationSeconds = 1.2f;
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
                
                // Retro Game Over: Absteigende Tonfolge wie klassische Arcade-Spiele
                // E, D, C, G (tief) - trauriger, absteigender Sound
                double[] frequencies = { 659.25, 587.33, 523.25, 392.00 }; // E5, D5, C5, G4
                double[] noteDurations = { 0.25, 0.25, 0.25, 0.25 };
                
                // Welche Note spielen wir?
                int noteIndex = 0;
                double cumTime = 0;
                for (int n = 0; n < frequencies.Length; n++)
                {
                    if (progress >= cumTime && progress < cumTime + noteDurations[n])
                    {
                        noteIndex = n;
                        break;
                    }
                    cumTime += noteDurations[n];
                }
                
                double frequency = frequencies[noteIndex];
                
                // Envelope für jeden Ton mit langsamem Decay
                double noteStartTime = 0;
                for (int n = 0; n < noteIndex; n++)
                {
                    noteStartTime += noteDurations[n];
                }
                double noteProgress = (progress - noteStartTime) / noteDurations[noteIndex];
                double env = Math.Exp(-2.5 * noteProgress);
                
                // Retro-Sound: Rechteckwelle für klassischen 8-bit Charakter
                double squareWave = Math.Sign(Math.Sin(2.0 * Math.PI * frequency * t));
                
                // Leichte Sinus-Komponente für weniger harsch
                double sineWave = Math.Sin(2.0 * Math.PI * frequency * t);
                
                // Mix: Mehr Rechteck für Retro-Feeling
                double mixed = (squareWave * 0.7 + sineWave * 0.3) * env;
                
                // Fade out am Ende
                if (progress > 0.85)
                {
                    mixed *= 1.0 - ((progress - 0.85) / 0.15);
                }
                
                short sample = (short)(amplitude * mixed);
                bw.Write(sample);
                t += dt;
            }

            bw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return SoundEffect.FromStream(ms);
        }

        private SoundEffect CreateLevelCompleteSound()
        {
            const int sampleRate = 44100;
            float durationSeconds = 0.8f;
            int samples = (int)(sampleRate * durationSeconds);
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            WriteWavHeader(bw, samples);

            double amplitude = 32760 * 0.65f;
            double t = 0;
            double dt = 1.0 / sampleRate;
            
            for (int i = 0; i < samples; i++)
            {
                double progress = t / durationSeconds;
                
                // Fröhliche aufsteigende Tonfolge: C, E, G, C (höher)
                double[] frequencies = { 523.25, 659.25, 783.99, 1046.50 }; // C5, E5, G5, C6
                double[] noteDurations = { 0.15, 0.15, 0.15, 0.35 }; // Letzter Ton länger
                
                // Welche Note?
                int noteIndex = 0;
                double cumTime = 0;
                for (int n = 0; n < frequencies.Length; n++)
                {
                    if (progress >= cumTime && progress < cumTime + noteDurations[n])
                    {
                        noteIndex = n;
                        break;
                    }
                    cumTime += noteDurations[n];
                }
                
                double frequency = frequencies[noteIndex];
                
                // Note Progress berechnen
                double noteStartTime = 0;
                for (int n = 0; n < noteIndex; n++)
                {
                    noteStartTime += noteDurations[n];
                }
                double noteProgress = (progress - noteStartTime) / noteDurations[noteIndex];
                
                // Envelope: Schneller Attack, dann Sustain, dann Decay
                double env;
                if (noteIndex < 3) // Erste 3 Töne: kurz und knackig
                {
                    env = Math.Exp(-4.0 * noteProgress);
                }
                else // Letzter Ton: länger mit Sustain
                {
                    env = noteProgress < 0.1 ? noteProgress / 0.1 :
                          noteProgress < 0.6 ? 1.0 :
                          1.0 - ((noteProgress - 0.6) / 0.4);
                }
                
                // Retro-Sound: Mix aus Rechteck und Sinus
                double squareWave = Math.Sign(Math.Sin(2.0 * Math.PI * frequency * t));
                double sineWave = Math.Sin(2.0 * Math.PI * frequency * t);
                
                double mixed = (squareWave * 0.6 + sineWave * 0.4) * env;
                
                short sample = (short)(amplitude * mixed);
                bw.Write(sample);
                t += dt;
            }

            bw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return SoundEffect.FromStream(ms);
        }

        private SoundEffect CreateVictorySound()
        {
            const int sampleRate = 44100;
            float durationSeconds = 2.0f;
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
                
                // Epische Siegesmelodie: C, E, G, C, G, C (höher und höher)
                // Klassische "Fanfare"-Struktur
                double[] frequencies = { 
                    523.25,  // C5
                    659.25,  // E5
                    783.99,  // G5
                    1046.50, // C6
                    1567.98, // G6
                    2093.00  // C7
                };
                double[] noteDurations = { 0.2, 0.2, 0.2, 0.3, 0.3, 0.6 };
                
                // Welche Note?
                int noteIndex = 0;
                double cumTime = 0;
                for (int n = 0; n < frequencies.Length; n++)
                {
                    if (progress >= cumTime && progress < cumTime + noteDurations[n])
                    {
                        noteIndex = n;
                        break;
                    }
                    cumTime += noteDurations[n];
                }
                if (noteIndex >= frequencies.Length) noteIndex = frequencies.Length - 1;
                
                double frequency = frequencies[noteIndex];
                
                // Note Progress
                double noteStartTime = 0;
                for (int n = 0; n < noteIndex; n++)
                {
                    noteStartTime += noteDurations[n];
                }
                double noteProgress = (progress - noteStartTime) / noteDurations[noteIndex];
                
                // Envelope: Unterschiedlich für verschiedene Noten
                double env;
                if (noteIndex < 5) // Erste Noten: Attack-Decay
                {
                    env = noteProgress < 0.05 ? noteProgress / 0.05 : Math.Exp(-3.0 * (noteProgress - 0.05));
                }
                else // Finale Note: Lang und triumphierend
                {
                    env = noteProgress < 0.05 ? noteProgress / 0.05 :
                          noteProgress < 0.7 ? 1.0 :
                          1.0 - ((noteProgress - 0.7) / 0.3);
                }
                
                // Reicherer Sound: Hauptton + Harmonische
                double tone1 = Math.Sin(2.0 * Math.PI * frequency * t) * 0.5;
                double tone2 = Math.Sin(2.0 * Math.PI * frequency * 2 * t) * 0.25; // Oktave
                double tone3 = Math.Sin(2.0 * Math.PI * frequency * 3 * t) * 0.15; // Quinte
                double squareWave = Math.Sign(Math.Sin(2.0 * Math.PI * frequency * t)) * 0.1; // Leichte Retro-Note
                
                double mixed = (tone1 + tone2 + tone3 + squareWave) * env;
                
                short sample = (short)(amplitude * mixed);
                bw.Write(sample);
                t += dt;
            }

            bw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return SoundEffect.FromStream(ms);
        }

        private SoundEffect CreatePaddleEnlargeSound()
        {
            const int sampleRate = 44100;
            float durationSeconds = 0.3f;
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
                
                // Aufsteigende Tonleiter für "Vergrößerung"
                double frequency = 300 + progress * 500; // 300 Hz -> 800 Hz
                
                double env = 1.0 - progress * 0.5; // Leichter Fade-out
                
                double tone = Math.Sin(2.0 * Math.PI * frequency * t);
                
                short sample = (short)(amplitude * tone * env);
                bw.Write(sample);
                t += dt;
            }

            bw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return SoundEffect.FromStream(ms);
        }

        private SoundEffect CreatePaddleShrinkSound()
        {
            const int sampleRate = 44100;
            float durationSeconds = 0.25f;
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
                
                // Absteigende Tonleiter für "Verkleinerung"
                double frequency = 800 - progress * 500; // 800 Hz -> 300 Hz
                
                double env = Math.Exp(-3.0 * progress); // Schneller Fade-out
                
                double tone = Math.Sin(2.0 * Math.PI * frequency * t);
                
                short sample = (short)(amplitude * tone * env);
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
