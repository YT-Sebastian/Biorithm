using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Biorithm
{
    public class ParticleSwarmOptimisation : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ParticleSwarmOptimisation()
          : base("Particle Swarm Optimisation", "PSO",
              "PSO",
              "Biorithm", "Swarm")
        {
        }
        
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "True to run, False to stop", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "Reset", "Reset the iterations", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Iterations", "Iternations", "Number of iterations. -1 for Infinite", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Interval", "Interval", "Interval of update in milliseconds", GH_ParamAccess.item);
            pManager.AddPointParameter("Swarm", "Swarm", "Swarm as a list of points", GH_ParamAccess.list);
            pManager.AddCurveParameter("Fitness", "Fitness", "Fitness as a curve", GH_ParamAccess.item);
            pManager.AddNumberParameter("Particle learning factor", "C1", "Learning factor for particles(agents)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Swarm learning factor", "C2", "Learning factor for swarm(global)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Influence factor", "C3", "Influence factor for movement", GH_ParamAccess.item);
            pManager.AddNumberParameter("Max Velocity", "Max", "Max speed Velocity of particles(agents)", GH_ParamAccess.item);
            pManager.AddCurveParameter("Attractor", "Attractor", "Attractor Curve", GH_ParamAccess.list);
            pManager.AddNumberParameter("Attractor factor", "Attractor factor", "Attraction factor", GH_ParamAccess.item);
            pManager.AddBrepParameter("Boundary", "Boundary", "Boundary", GH_ParamAccess.item);
            pManager.AddNumberParameter("Boundary factor", "Boundary factor", "Boundary factor", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Iteration number", "Iteration", "iteration number complete", GH_ParamAccess.item);
            pManager.AddCurveParameter("Output", "Output", "Output", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Document GrasshopperDocument = base.OnPingDocument();
            bool run =  new bool();
            bool reset =  new bool();
            int iterations = new int();
            int interval = new int();
            List<Point3d> agents = new List<Point3d>();

            if (!DA.GetData(0, ref run)) return;
            if (!DA.GetData(1, ref reset)) return;
            if (!DA.GetData(2, ref iterations)) return;
            if ((!DA.GetData(3, ref interval)) | (interval < 1)) return;
            if (!DA.GetDataList(4, agents)) return;
            if (!DA.GetData(5, ref fitness)) return;
            if (!DA.GetData(6, ref c1)) return;
            if (!DA.GetData(7, ref c2)) return;
            if (!DA.GetData(8, ref influence)) return;
            if (!DA.GetData(9, ref max)) return;
            if (!DA.GetDataList(10, attract)) return;
            if (!DA.GetData(11, ref attractFactor)) return;
            if (!DA.GetData(12, ref boundary)) return;
            if (!DA.GetData(13, ref boundaryFactor)) return;

            if (iterations != _maximum)
            {
                _maximum = iterations;
                _counter = iterations;
                swarm = new Swarm(agents, fitness);
            }
            
            DA.SetData(0, _maximum - _counter);
            DA.SetDataList(1, output);

            _run = run;

            if (_counter == 0)
            {
                _run = false;
            }

            if (_run)
            {
                GrasshopperDocument.ScheduleSolution(interval, ScheduleCallback);
            }

            if (reset)
            {
                _maximum = iterations;
                _counter = iterations;
                swarm = new Swarm(agents, fitness);
                GrasshopperDocument.ScheduleSolution(interval, ScheduleCallback);
            }
        }

        private int _maximum = int.MinValue;
        private int _counter = -1;
        private bool _run = false;

        private Swarm swarm =  new Swarm();
        private List<Polyline> output = new List<Polyline>();

        Curve fitness;
        double c1 = new double();
        double c2 = new double();
        double influence = new double();
        double max = new double();

        List<Curve> attract =  new List<Curve>();
        double attractFactor = new double();
        Brep boundary;
        double boundaryFactor = new double();

        private void ScheduleCallback(GH_Document document)
        {
            _counter--;

            swarm.ParticleSwarmOptimisation(c1, c2, influence, max, attract, attractFactor, boundary, boundaryFactor);
            swarm.UpdatePosition(fitness);

            output.Clear();
            foreach (Particle i in swarm.bodies)
            {
                if (i.history.Count > 1)
                {
                    Polyline path = new Polyline(i.history);
                    output.Add(path);
                }
            }
            
            this.ExpireSolution(false);
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("bfc506a0-37de-48ab-991d-5e5f96b2baeb"); }
        }
    }
}
