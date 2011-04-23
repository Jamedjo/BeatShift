using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace ContentPipelineProject
{
    /// <summary>
    /// Import a model including vertex and indice data.
    /// Needed as BePU physics's helper method to do this breaks when normal/tangent/binormal data is included for normal mapping.
    /// Extended from TrianglePickingSample as reccomended on BePU forums.
    /// </summary>
    [ContentProcessor(DisplayName = "BeatShift Advanced ModelProcessor")]
    public class BShiftAdvancedModelProcessor : BShiftModelProcessor
    {
        public override ModelContent Process(NodeContent input,
                                             ContentProcessorContext context)
        {

            MeshContent mesh = input as MeshContent;
            if (mesh != null)
            {
                MeshHelper.CalculateTangentFrames(mesh,
                    VertexChannelNames.TextureCoordinate(0),
                    VertexChannelNames.Tangent(0),
                    VertexChannelNames.Binormal(0));
            }

            

            // Use base ModelProcessor class to do the actual model processing
            ModelContent model = base.Process(input, context);

            return model;
        }

        void ExtractVerticesAndIndices(NodeContent node)
        {
            // Is this node a mesh?
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Look up the absolute transform of the mesh.
                Matrix absoluteTransform = mesh.AbsoluteTransform;

                // Loop over all the pieces of geometry in the mesh.
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    int baseElement = vertices.Count;
                    foreach (Vector3 v in geometry.Vertices.Positions)
                    {
                        vertices.Add(Vector3.Transform(v, absoluteTransform));
                    }
                    foreach (int i in geometry.Indices)
                    {
                        indices.Add(baseElement + i);
                    }
                }
            }

            // Recursively scan over the children of this node.
            foreach (NodeContent child in node.Children)
            {
                ExtractVerticesAndIndices(child);
            }
        }
    }


    /// <summary>
    /// Import a model including vertex and indice data.
    /// Needed as BePU physics's helper method to do this breaks when normal/tangent/binormal data is included for normal mapping.
    /// Extended from TrianglePickingSample as reccomended on BePU forums.
    /// </summary>
    [ContentProcessor(DisplayName = "BeatShift ModelProcessor")]
    public class BShiftModelProcessor : ModelProcessor
    {
        protected List<Vector3> vertices = new List<Vector3>();
        protected List<int> indices = new List<int>();

        public override ModelContent Process(NodeContent input,
                                             ContentProcessorContext context)
        {

            // Look up the input vertex/indice positions.
            ExtractVerticesAndIndices(input);

            // The model Tag property can hold any type of data. Using built-in types 
            // means that ContentTypeWriter and ContentTypeReaders are not needed to process this data.
            // By using a dictionary that maps strings to objects, we can store many different
            // objects of different  kinds of information inside the single Tag value.

            Dictionary<string, object> tagData = new Dictionary<string, object>();

            // Store vertex/indice information in the tag data, as arrays of Vector3.
            tagData.Add("Vertices", vertices.ToArray());
            //tagData.Add("triVertices", triVertices.ToArray());
            tagData.Add("Indices", indices.ToArray());


            // Use base ModelProcessor class to do the actual model processing
            ModelContent model = base.Process(input, context);

            model.Tag = tagData;

            return model;
        }

        void ExtractVerticesAndIndices(NodeContent node)
        {
            // Is this node a mesh?
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Look up the absolute transform of the mesh.
                Matrix absoluteTransform = mesh.AbsoluteTransform;

                // Loop over all the pieces of geometry in the mesh.
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    int baseElement = vertices.Count;
                    foreach (Vector3 v in geometry.Vertices.Positions)
                    {
                        vertices.Add(Vector3.Transform(v, absoluteTransform));
                    }
                    foreach (int i in geometry.Indices)
                    {
                        indices.Add(baseElement + i);
                    }
                }
            }

            // Recursively scan over the children of this node.
            foreach (NodeContent child in node.Children)
            {
                ExtractVerticesAndIndices(child);
            }
        }
    }


}
