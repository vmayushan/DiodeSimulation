using System;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using PICSolver.Abstract;
using PICSolver.Domain;

namespace PICSolver.Emitter
{
    public class Emitter2D : IEmitter
    {
        private readonly double fromX;
        private readonly double toX;
        private readonly double fromY;
        private readonly double toY;
        private readonly double px;
        private readonly double py;
        private readonly double currentDensity;
        private readonly double dx;
        private readonly double dy;
        private readonly double step;
        private readonly Particle[] particles;
        private readonly Normal distribution;
        private readonly EmissionType emissionType;
        private readonly Random random = new MersenneTwister();
        private readonly double[] positions;

        public Emitter2D(double x0, double y0, double xN, double yN, int particlesCount, double energyX, double energyY, double currentDensity, double step, EmissionType emissionType)
        {
            fromX = x0;
            toX = xN;
            fromY = y0;
            toY = yN;
            ParticlesCount = particlesCount;

            var gammaX = Constants.KineticEnergyToLorentzFactor(energyX);
            var betaX = Constants.LorentzFactorToBeta(gammaX);
            px = Constants.Momentum(betaX);

            var gammaY = Constants.KineticEnergyToLorentzFactor(energyY);
            var betaY = Constants.LorentzFactorToBeta(gammaY);
            py = Constants.Momentum(betaY);

            dx = (toX - fromX) / (ParticlesCount);
            dy = (toY - fromY) / (ParticlesCount);

            this.currentDensity = currentDensity;
            this.step = step;
            particles = new Particle[ParticlesCount];
            var sigma = Math.Sqrt(-10.0 * Constants.ElectronCharge / Constants.ElectronMass);
            distribution = new Normal(0, sigma, new Random());
            this.emissionType = emissionType;

            this.positions = LineHammersley(particlesCount);
        }

        public int ParticlesCount { get; }
        public double ParticleCharge { get; private set; }
        public double Length => Math.Sqrt(Math.Pow((toX - fromX), 2) + Math.Pow((toY - fromY), 2));

        public Particle[] Inject()
        {
            ParticleCharge = currentDensity * Length * step / ParticlesCount;
            for (var i = 0; i < ParticlesCount; i++)
            {
                double y;
                switch (emissionType)
                {
                    case EmissionType.QuietStart:
                        y = fromY + (i + 0.5) * dy;
                        break;
                    case EmissionType.RandomStart:
                        y = fromY + random.NextDouble() * (toY - fromY);
                        break;
                    case EmissionType.RandomStartInSection:
                        y = fromY + (i + random.NextDouble()) * dy;
                        break;
                    case EmissionType.HammersleyStart:
                        y = fromY + positions[i] * (toY - fromY);
                        break;
                    default:
                        throw new NotSupportedException();
                }
                var x = fromX + (i + 0.5) * dx;
                //+ Constants.Momentum(Constants.VelocityToBeta(distribution.Sample()))
                particles[i] = new Particle(x, y, px, py , 0, 0, ParticleCharge);

            }

            return particles;
        }

        public double[] LineHammersley(int n)
        {
            var result = new double[n];
            double p, u;
            int k, kk, pos;
            for (k = 0, pos = 0; k < n; k++)
            {
                u = 0;
                for (p = 0.5, kk = k; kk > 0; p *= 0.5, kk >>= 1)
                    if (kk % 2 != 0) // kk mod 2 == 1
                        u += p;
                result[pos++] = u;
            }
            return result;
        }

    }


}