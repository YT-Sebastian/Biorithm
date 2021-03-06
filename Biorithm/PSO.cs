﻿using System;
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
        public List<Point3d> history = new List<Point3d>();
        public double particleFitnessValue;

        public Particle(Point3d startPosition, Vector3d startVelocity)
        {
            position = startPosition;
            particleBest = startPosition;
            velocity = startVelocity;
            history.Add(startPosition);
        }
    }

    public class Swarm
    {
        public List<Particle> bodies = new List<Particle>();
        public Point3d globalBest = new Point3d(0,0,0);
        public RandomUniformDistribution rnd;
        public double globalFitnessValue;

        public Swarm() { }

        public Swarm(List<Point3d> agents, Curve fitness)
        {
            rnd = new RandomUniformDistribution(agents.Count);
            fitness.ClosestPoint(globalBest, out double t);
            globalFitnessValue = globalBest.DistanceTo(fitness.PointAt(t));

            for (int i = 0; i < agents.Count; i++)
            {
                bodies.Add(new Particle(agents[i], new Vector3d((rnd.Next() * 2) - 1, (rnd.Next() * 2) - 1, (rnd.Next() * 2) - 1)));
                fitness.ClosestPoint(agents[i], out t);
                bodies[i].particleFitnessValue = agents[i].DistanceTo(fitness.PointAt(t));

                if (bodies[i].particleFitnessValue < globalFitnessValue)
                {
                    globalFitnessValue = bodies[i].particleFitnessValue;
                    globalBest = agents[i];
                }
            }
        }

        public void ParticleSwarmOptimisation(double particleLearningFactor, double globalLearningFactor, double influenceFactor, double maxVelocity, 
            List<Curve> attract, double attractFactor, Brep boundary, double boundaryFactor)
        {
            foreach (Particle i in bodies)
            {
                Vector3d toPbest = new Vector3d(i.particleBest - i.position);
                toPbest.Unitize();
                Vector3d toGbest = new Vector3d(globalBest - i.position);
                toGbest.Unitize();

                double pbestMultiplier = particleLearningFactor * rnd.Next();
                double gbestMultiplier = globalLearningFactor * rnd.Next();

                Vector3d moveVector = (toPbest * pbestMultiplier) + (toGbest * gbestMultiplier);
                Vector3d rndInfluence = new Vector3d((rnd.Next() * 2) - 1, (rnd.Next() * 2) - 1, (rnd.Next() * 2) - 1) * influenceFactor;

                Vector3d attractVector = new Vector3d();

                foreach (Curve c in attract)
                {
                    c.ClosestPoint(i.position, out double t2);
                    attractVector += (c.PointAt(t2) - i.position) * (1 / i.position.DistanceTo(c.PointAt(t2)));
                }

                attractVector.Unitize();

                Vector3d boundaryVector = new Vector3d();

                if (!boundary.IsPointInside(i.position, RhinoMath.SqrtEpsilon, false))
                {
                    boundaryVector += boundary.ClosestPoint(i.position) - i.position;
                    boundaryVector.Unitize();
                }

                i.velocity += (moveVector + rndInfluence + (attractVector * attractFactor) + (boundaryVector * boundaryFactor));

                if (i.velocity.Length > maxVelocity)
                {
                    i.velocity *= maxVelocity / i.velocity.Length;
                }
            }
        }

        public void UpdatePosition(Curve fitness)
        {
            foreach (Particle i in bodies)
            {
                i.position += i.velocity;
                i.history.Add(i.position);

                fitness.ClosestPoint(i.position, out double t);

                double particleFitnessValue = i.position.DistanceTo(fitness.PointAt(t));

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