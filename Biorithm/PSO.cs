using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;

namespace Biorithm
{
    public class Particle
    {
        Point3d position;
        Point3d particleBest;
        Vector3d velocity;
        List<Point3d> history;

        public Particle(Point3d startPosition, Vector3d startVelocity)
        {
            position = startPosition;
            particleBest = startPosition;
            velocity = startVelocity;
            history.Add(position);
        }
    }

    public class Swarm
    {
        List<Particle> bodies;
        Point3d globalBest = new Point3d(0,0,0);
        RandomUniformDistribution rnd;
        double globalFitnessValue;

        public Swarm(List<Point3d> agents, Point3d fitness)
        {
            rnd = new RandomUniformDistribution(agents.Count);
            globalFitnessValue = globalBest.DistanceTo(fitness);

            foreach (Point3d i in agents)
            {
                bodies.Add(new Particle(i, new Vector3d(rnd.Next(), rnd.Next(), rnd.Next())));
                double particleFitnessValue = i.DistanceTo(fitness);

                if (particleFitnessValue < globalFitnessValue)
                {
                    globalFitnessValue = particleFitnessValue;
                    globalBest = i;
                }
            }
        }

        public void ParticleSwarmOptimisation()
        {

        }
    }

    public class RandomUniformDistribution
    {
        MersenneTwister rnd;

        public RandomUniformDistribution(int seed)
        {
            rnd = new MersenneTwister(seed);
        }

        public double Next()
        {
            return rnd.NextDouble();
        }
    }
}