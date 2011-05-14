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
using System.Collections;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using System.Diagnostics;


namespace BeatShift
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class MapData
    {
        float waypointWidth = 4f;//60f;
        float verticalOffset = 4f;

        public List<MapPoint> mapPoints = new List<MapPoint>();


        public MapData(String mapName,float waypoint_Width, float vertical_Offset)
        {
            readMapData(mapName);//Change to map name
            waypointWidth = waypoint_Width;
            verticalOffset = vertical_Offset;
        }

        void readMapData( String mapName ) {
            try
            {
                System.IO.Stream stream = TitleContainer.OpenStream(BeatShift.contentManager.RootDirectory + "/MapPointData/" + mapName + ".bsmp");
                System.IO.StreamReader sreader = new System.IO.StreamReader(stream);

                String line;
                int i = 0;
                while ((line = sreader.ReadLine()) != null)
                {
                    String[] coordinates = line.Split(';');
                    if (coordinates.Length == 4)
                    {
                        String[] position = coordinates[0].Split(':');
                        Vector3 positionV = new Vector3((float)Convert.ToDouble(position[0]), (float)Convert.ToDouble(position[1]), (float)Convert.ToDouble(position[2]));
                        
                        String[] up = coordinates[1].Split(':');
                        Vector3 upRollV = Vector3.Normalize(new Vector3((float)Convert.ToDouble(up[0]), (float)Convert.ToDouble(up[1]), (float)Convert.ToDouble(up[2])));

                        String[] tangent = coordinates[2].Split(':');
                        Vector3 tangentV = Vector3.Normalize(new Vector3((float)Convert.ToDouble(tangent[0]), (float)Convert.ToDouble(tangent[1]), (float)Convert.ToDouble(tangent[2])));

                        String[] road = coordinates[3].Split(':');
                        Vector3 roadV = Vector3.Normalize(new Vector3((float)Convert.ToDouble(road[0]), (float)Convert.ToDouble(road[1]), (float)Convert.ToDouble(road[2])));


                        try
                        {
                            mapPoints.Add(new MapPoint(i,positionV, upRollV, tangentV, roadV,waypointWidth,verticalOffset));
                        }
                        catch (FormatException fe)
                        {
                            Debug.WriteLine(fe);
                        } catch (OverflowException oe)
                        {
                            Debug.WriteLine(oe);
                        }
                        i++;
                    }
                }

                stream.Close();
                stream.Dispose();
                sreader.Close();
                sreader.Dispose();
            }
            catch (System.IO.FileNotFoundException fnfe)
            {
                // this will be thrown by OpenStream if mapName.txt
                // doesn't exist in the title storage location
                System.Diagnostics.Debug.WriteLine("Problem with loading map points: " + fnfe.ToString());
            }
        }

        public MapPoint getStartPoint(){
            return mapPoints[0];
        }
        public MapPoint nextPoint(MapPoint current)
        {
            int nextIndex = current.getIndex() + 1;
            if (nextIndex == mapPoints.Count) nextIndex = 0;
            return mapPoints[nextIndex];
        }
        public int previousIndex(int nextIndex)
        {
            if (nextIndex == 0) return mapPoints.Count-1;
            return nextIndex-1;
        }
        public MapPoint previousPoint(MapPoint current)
        {
            int nextIndex = current.getIndex();
            return mapPoints[previousIndex(nextIndex)];
        }
        public MapPoint wrongwayPoint(MapPoint current)
        {
            int nextIndex = current.getIndex();
            return mapPoints[previousIndex(previousIndex(previousIndex(previousIndex(nextIndex))))];
        }

        MapPoint[] testPoint = new MapPoint[4];//, testPoint1, testPoint2, testPoint3;
        /// <summary>
        /// Algorithm to find the nearest map point to a position given knowledge about its previous position to limit the search.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="searchDepth">
        /// Default searches 'searchDepth' MapPoints back and 'searchDepth' forward.
        /// So search depth of 2 creates 5 distance comparisions including the lastNearetPoint.
        /// TODO: If point within the threshold is not found the search is expanded to 12 either side, giving 25 places.</param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public MapPoint nearestMapPoint(Vector3 position, MapPoint lastNearestPoint, int searchDepth, float threshold)
        {
            MapPoint nearest = lastNearestPoint;
            float distance = distanceToMapPoint(position,lastNearestPoint);
            float tempDistance;
            testPoint[0] = nextPoint(lastNearestPoint);
            testPoint[1] = nextPoint(testPoint[0]);
            testPoint[2] = previousPoint(lastNearestPoint);
            testPoint[3] = previousPoint(testPoint[2]);

            for (int i = 0; i < testPoint.Length; i++)
            {
                if (nearestMapPoint(position, distance, testPoint[i], out tempDistance))
                {
                    distance = tempDistance;
                    nearest = testPoint[i];
                }
            }
            return nearest;
        }

        //Compares which map point is nearest out of the two given points
        //returns true if second point is nearest
        Boolean nearestMapPoint(Vector3 position, float firstMapPointDistance, MapPoint secondMapPoint, out float secondDistance)
        {
            secondDistance = distanceToMapPoint(position, secondMapPoint);
            if (secondDistance < firstMapPointDistance) return true;
            return false;
        }
        float distanceToMapPoint(Vector3 position, MapPoint point)
        {
            return (point.position-position).Length();
        }

        /// <summary>
        /// Algorithm to find the nearest map point to a position given knowledge about its previous position to limit the search.
        /// Default searches 2 MapPoints back and 2 forward meaning that 5 MapPoints are checked for the nearest one.
        /// TODO: If point within the threshold is not found the search is expanded to 12 either side, giving 25 places.
        /// </summary>
        /// <param name="position">The world space position to search for.</param>
        /// <param name="lastNearestPoint">The MapPoint that it is expected that 'position' will be near</param>
        /// <returns></returns>
        public MapPoint nearestMapPoint(Vector3 position, MapPoint lastNearestPoint)
        {
            return nearestMapPoint(position, lastNearestPoint, 2, 60f);
        }
    }

    public class MapPoint
    {
        int index;
        Vector3 curveposition;
        public Vector3 position;
        //public Vector3 normal;//not sure what this is as the normal&binormal could be any vectors in the plane defined by the tangent
        public Vector3 tangent;//the direction forwards/backwards direction if the racetrack
        //Vector3 roll;
        public Vector3 roadSurface;//the left/right direction of the racetrack
        public Vector3 trackUp;//the up direction of the racetrack
        
        //float width;
        //Vector3 colour;
        Vector3 orange = Color.OrangeRed.ToVector3();
        Vector3 green = Color.LawnGreen.ToVector3();
        Boolean hitToggle = false;
        float waypointWidth;
        
        public MapPoint(int pointIndex, Vector3 pos, Vector3 up, Vector3 tang, Vector3 road,float waypointSize, float vOffset)
        {
            index = pointIndex;
            waypointWidth = waypointSize;
            curveposition = pos;
            trackUp = up;
            roadSurface = road;
            tangent = tang;

            trackUp.Normalize();
            roadSurface.Normalize();
            tangent.Normalize();
            trackUp = Vector3.Cross(roadSurface, tang);

            //position is 3 above curve direction, where above is in the direction of track up
            position = pos + Vector3.Transform(new Vector3(0, vOffset, 0), Matrix.CreateWorld(new Vector3(0, 0, 0), tangent, trackUp));

            //normal = n;
            //roll = r;
            //Vector3 rollVector = Vector3.Up;//Rotate up around the tangent by roll degrees
            //roadSurface = Vector3.Cross(tangent,rollVector);//cros of tangent and roll(up) lies at 90degrees to tangent 
            //trackUp = -Vector3.Cross(tangent, roadSurface);
        }

        public int getIndex()
        {
            return index;
        }

        public float getWidth()
        {
            return waypointWidth;
        }

        public void pointHit()
        {
            hitToggle = !hitToggle;
        }

        public Vector3 getColour()
        {
            if(hitToggle)
            {
                return green;
            }
            else 
            { 
                return orange;
            }
        }
    }
}
