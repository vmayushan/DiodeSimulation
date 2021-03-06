﻿using PICSolver.Domain;

namespace PICSolver.Abstract
{
    public interface IEmitter
    {
        Particle[] Inject();
        double Length { get; }
        int ParticlesCount { get; }
        double ParticleCharge { get; }
    }
    public enum EmissionType
    {
        QuietStart,
        RandomStart,
        RandomStartInSection,
        HammersleyStart
    }
}
