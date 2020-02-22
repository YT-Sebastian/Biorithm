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
        public Point3d position;
        public Point3d particleBest;
        public Vector3d velocity;
        public List<Point3d> history;
        public double particleFitnessValue;

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
        public List<Particle> bodies;
        public Point3d globalBest = new Point3d(0,0,0);
        public RandomUniformDistribution rnd;
        public double globalFitnessValue;

        public Swarm(List<Point3d> agents, Point3d fitness)
        {
            rnd = new RandomUniformDistribution(agents.Count);
            globalFitnessValue = globalBest.DistanceTo(fitness);

            for (int i = 0; i < agents.Count; i++)
            {
                bodies.Add(new Particle(agents[i], new Vector3d(rnd.Next(), rnd.Next(), rnd.Next())));
                bodies[i].particleFitnessValue = agents[i].DistanceTo(fitness);

                if (bodies[i].particleFitnessValue < globalFitnessValue)
                {
                    globalFitnessValue = bodies[i].particleFitnessValue;
                    globalBest = agents[i];
                }
            }
        }

        public void ParticleSwarmOptimisation(double particleLearningFactor, double globalLearningFactor, double influenceFactor, double maxVelocity)
        {
            foreach (Particle i in bodies)
            {
                Vector3d toPbest = new Vector3d(i.particleBest - i.position);
                Vector3d toGbest = new Vector3d(globalBest - i.position);

                double pbestMultiplier = particleLearningFactor * rnd.Next();
                double gbestMultiplier = globalLearningFactor * rnd.Next();

                Vector3d moveVector = (toPbest * pbestMultiplier) + (toGbest * gbestMultiplier);

                i.velocity += moveVector;

                if (i.velocity.Length > maxVelocity)
                {
                    i.velocity *= maxVelocity / i.velocity.Length;
                }
            }
        }

        public void UpdatePosition(Point3d fitness)
        {
            foreach (Particle i in bodies)
            {
                i.position += i.velocity;
                i.history.Add(i.position);

                double particleFitnessValue = i.position.DistanceTo(fitness);

                if (particleFitnessValue < i.particleFitnessValue)
                {
                    i.particleFitnessValue = particleFitnessValue;
                    i.particleBest = i.position;
                }

                if (particleFitnessValue < globalFitnessValue)
                {
                    globalFitnessValue = particleFitnessValue;
                    globalBest = i.position;
                }
            }
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