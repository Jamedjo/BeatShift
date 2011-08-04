using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BeatShift
{
    public class KdTree3
    {
        KdTreeNode root;

        public KdTree3(List<MapPoint> input)
        {
            Random r = new Random(242);
            root = buildTree(input.OrderBy((m) => r.Next()).ToList(), 0);//build tree after randomizing input order
        }

        KdTreeNode buildTree(List<MapPoint> points, int depth)
        {
            if (points.Count > 0)
            {
                int axis = depth % 3;
                
                List<MapPoint> leftList = new List<MapPoint>();
                List<MapPoint> rightList = new List<MapPoint>();
                
                KdTreeNode node = new KdTreeNode();
                MapPoint selected = points[0];

                for (int i = 1; i < points.Count; i++)
                {
                    if (isLeftOfAxis(axis, selected.position, points[i].position))
                        leftList.Add(points[i]);
                    else
                        rightList.Add(points[i]);
                }
                node.value = selected;
                node.left = buildTree(leftList, depth + 1);
                node.right = buildTree(rightList, depth + 1);
                node.axis = axis;
                return node;

            }
            else return null;
        }

        bool isLeftOfAxis(int axis, Vector3 axisSpliter, Vector3 queryPoint)
        {
            switch (axis)
            {
                case 0:
                    if (queryPoint.X < axisSpliter.X)
                        return true;
                    else
                        return false;
                //break;
                case 1:
                    if (queryPoint.Y < axisSpliter.Y)
                        return true;
                    else
                        return false;

                //break;
                case 2:
                    if (queryPoint.Z < axisSpliter.Z)
                        return true;
                    else
                        return false;
                //break;
                default:
                    throw new Exception();
            }
        }

        float distance_axis(Vector3 queryPoint, KdTreeNode axisSpliter)
        {
            switch (axisSpliter.axis)
            {
                case 0:
                    return Math.Abs(axisSpliter.value.position.X - queryPoint.X);
                //break;
                case 1:
                    return Math.Abs(axisSpliter.value.position.Y - queryPoint.Y);
                //break;
                case 2:
                    return Math.Abs(axisSpliter.value.position.Z - queryPoint.Z);
                default:
                    throw new Exception();
            }
        }

        float distance(Vector3 position, MapPoint point)
        {
            return (point.position - position).Length();
        }

        public MapPoint getNearestNeighbour(Vector3 queryPoint)
        {
            return getNearestNeighbour(queryPoint,root,root).value;
        }

        KdTreeNode getNearestNeighbour(Vector3 find, KdTreeNode searchNode, KdTreeNode best)
        {
            if (searchNode == null)
                return best;

            if (best == null)
                best = searchNode;

            bool isNearLeft = isLeftOfAxis(searchNode.axis, searchNode.value.position, find);
            var nearChild = isNearLeft ? searchNode.left : searchNode.right;
            var farChild = isNearLeft ? searchNode.right : searchNode.left;
            
            //consider currenet node
            if (distance(find, searchNode.value) < distance(find, best.value))
                best = searchNode;

            //search the near branch
            best = getNearestNeighbour(find, nearChild, best);
            

            //search away branch maybe
            if(distance_axis(find,searchNode)<distance(find,best.value))
                best = getNearestNeighbour(find, farChild, best);
            
            return best;
        }

    }

    public class KdTreeNode
    {
        public KdTreeNode left;
        public KdTreeNode right;
        public MapPoint value;
        public int axis;
    }
}
