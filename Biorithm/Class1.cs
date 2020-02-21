using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace Biorithm
{
    struct Particle
    {
        Point3d position;
        Point3d particleBest;
        Vector3d velocity;
        List<Point3d> history;
    }

    struct Swarm
    {
        List<Particle> bodies;
        Point3d globalBest;
    }


}