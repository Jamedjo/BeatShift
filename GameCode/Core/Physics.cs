using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.DataStructures;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Collidables;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Settings;
using System.Threading;


namespace BeatShift
{
       public struct D_Arrow { public Vector3 pos; public Vector3 dir; public Vector3 col;}
       //public struct ResetColumn { public Vector3 pos; public int column; public int resetWaypointIncrement; long raceTime; }

    /// <summary>
    /// Manages the general physics simulation for collisions and motion.
    /// </summary>
    public static class Physics
    {
        public static Space space;
        public static Boolean Enabled = false;

        static public List<StaticMesh> currentPhysicsObjects = new List<StaticMesh>();

        public static StaticMesh currentTrackFloor;
        public static StaticMesh currentTrackWall;
        public static StaticMesh currentTrackInvisibleWall;

        public static CollisionGroup noSelfCollideGroup = new CollisionGroup();

        public static void Initialize()
        {
            CollisionRules.CollisionGroupRules.Add(new CollisionGroupPair(noSelfCollideGroup, noSelfCollideGroup), CollisionRule.NoNarrowPhaseUpdate);
            reset();
        }

        /// <summary>
        /// Allows the physics to update itself.
        /// </summary>
        /// <param "gameTime">Provides a snapshot of timing values.</param>
        public static void Update(GameTime gameTime)
        {
            space.Update();
            //space.Update();
        }

        public static void ApplyAngularImpulse(ref Vector3 impulse, ref CompoundBody physicsBody)
        {
            physicsBody.AngularMomentum += impulse;

            var inertiaTensorInverse = physicsBody.InertiaTensorInverse;
            Vector3 angularMomentum = physicsBody.AngularMomentum;
            float x, y, z;

            if (MotionSettings.ConserveAngularMomentum)
            {
                x = angularMomentum.X * inertiaTensorInverse.M11 + angularMomentum.Y * inertiaTensorInverse.M21 + angularMomentum.Z * inertiaTensorInverse.M31;
                y = angularMomentum.X * inertiaTensorInverse.M12 + angularMomentum.Y * inertiaTensorInverse.M22 + angularMomentum.Z * inertiaTensorInverse.M32;
                z = angularMomentum.X * inertiaTensorInverse.M13 + angularMomentum.Y * inertiaTensorInverse.M23 + angularMomentum.Z * inertiaTensorInverse.M33;
            }
            else
            {
                x = physicsBody.AngularVelocity.X + impulse.X * inertiaTensorInverse.M11 + impulse.Y * inertiaTensorInverse.M21 + impulse.Z * inertiaTensorInverse.M31;
                y = physicsBody.AngularVelocity.Y + impulse.X * inertiaTensorInverse.M12 + impulse.Y * inertiaTensorInverse.M22 + impulse.Z * inertiaTensorInverse.M32;
                z = physicsBody.AngularVelocity.Z + impulse.X * inertiaTensorInverse.M13 + impulse.Y * inertiaTensorInverse.M23 + impulse.Z * inertiaTensorInverse.M33;
            }

            physicsBody.AngularVelocity = new Vector3(x, y, z);
        }

        public static void reset()
        {

            if (space != null)
            {
                foreach (StaticMesh p in currentPhysicsObjects)
                {
                    try
                    {
                        space.Remove(p);
                    }
                    catch (Exception e)
                    {

                    }
                }


                foreach (Racer r in Race.currentRacers)
                {
                    if (r.shipPhysics == null) return;
                    space.Remove(r.shipPhysics.physicsBody);
                }
                space.Dispose();
            }
            space = new Space();

            //Give the space some threads to work with.
#if XBOX
            //Note that not all four available hardware threads are used.
            //Currently, BEPUphysics will allocate an equal amount of work to each thread.
            //If two threads are put on one core, it will bottleneck the engine and run significantly slower than using 3 hardware threads.
            //space.ThreadManager.AddThread(delegate(object information) { Thread.CurrentThread.SetProcessorAffinity(new int[] { 1 }); }, null);
            //space.ThreadManager.AddThread(delegate(object information) { Thread.CurrentThread.SetProcessorAffinity(new int[] { 3 }); }, null);
            //space.ThreadManager.AddThread(delegate(object information) { Thread.CurrentThread.SetProcessorAffinity(new int[] { 5 }); }, null);

#else
            if (Environment.ProcessorCount > 1)
            {
                //On windows, just throw a thread at every processor.  The thread scheduler will take care of where to put them.
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    //space.ThreadManager.AddThread();
                }
            }
#endif
                        //MotionSettings.DefaultPositionUpdateMode

            float gravity = 0f;
            //if (!mainGame.isRayCasting) 
            //gravity = -10f;
            space.ForceUpdater.Gravity = new Vector3(0, gravity, 0);
        }

        public static void GetVerticesAndIndicesFromModelPipeline(Model model, out Vector3[] verts, out int[] indices)
        {
            //Get data from custom model processor.
            Dictionary<string, object> tagData = model.Tag as Dictionary<string, object>;
            if (tagData == null)
            {
                throw new Exception();
            }
            verts = tagData["Vertices"] as Vector3[];
            indices = tagData["Indices"] as int[];
            if (verts == null || indices == null)
            {
                throw new Exception();
            }
        }

        public static void addMapToPhysics(Model model, ModelCategory category)
        {
            if (category == ModelCategory.Track || category == ModelCategory.Wall || category == ModelCategory.InvisibleWall)
            {
                Vector3[] vertices;
                int[] indices;

                GetVerticesAndIndicesFromModelPipeline(model, out vertices, out indices);
                StaticMesh physicsMesh = new StaticMesh(vertices, indices);

                if (category == ModelCategory.Wall)
                {
                    physicsMesh.Material.KineticFriction = 0.9f;//Improves wall collisions by slowing ship down
                    physicsMesh.Material.StaticFriction = 0.9f;//Improves wall collisions by slowing ship down
                    physicsMesh.Material.Bounciness = 0;
                    physicsMesh.CollisionRules.Personal = CollisionRule.NoSolver;
                    currentTrackWall = physicsMesh;
                }
                if (category == ModelCategory.InvisibleWall)
                {
                    physicsMesh.CollisionRules.Personal = CollisionRule.NoSolver;
                    currentTrackInvisibleWall = physicsMesh;
                }
                if (category == ModelCategory.Track)
                {
                    physicsMesh.Material.Bounciness = 0.3f;
                    physicsMesh.Material.KineticFriction = 0.1f;

                    
                    physicsMesh.CollisionRules.Personal = CollisionRule.NoSolver;//No collisions with track
                    //CollisionGroup noSelfCollideGroup = new CollisionGroup();
                    currentTrackFloor = physicsMesh;
                    // currentTrackFloor.Events.InitialCollisionDetected += new BEPUphysics.NarrowPhaseSystems.CollisionInformations.Events.InitialCollisionDetectedEventHandler<Entity>(trackCollisionFn); 
                }

                currentPhysicsObjects.Add(physicsMesh);
                Physics.space.Add(physicsMesh);
            }
        }


    }
}
