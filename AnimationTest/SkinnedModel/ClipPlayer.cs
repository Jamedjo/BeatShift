using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using SkinnedModel;

namespace SkinnedModel
{
    public class ClipPlayer
    {
        private Matrix[] boneTransforms, skinTransforms, worldTransforms;
        AnimationClip currentClip;
        private IList<Keyframe> keyframeList;
        private SkinningData skinData;
        private float fps;
        private TimeSpan startTime, endTime, currentTime;
        private TimeSpan startTimeSwitch, endTimeSwitch;
        private bool isSwitching = false;
        private bool isLooping=false;
        private float blend = 0;
        private bool isPlayDirectionReversed = false;

        public ClipPlayer(SkinningData skd, float fps)
        {
            currentClip = skd.AnimationClips["Take 001"];
            keyframeList = currentClip.Keyframes;
            skinData = skd;
            boneTransforms = new Matrix[skd.BindPose.Count];
            skinTransforms = new Matrix[skd.BindPose.Count];
            worldTransforms = new Matrix[skd.BindPose.Count];
            startTime = TimeSpan.FromMilliseconds(0);
            currentTime = startTime;
            endTime = currentTime;
            this.fps = fps;
        }

        public void play(float startFrame,
                            float endFrame, bool loop)
        {
            //this.currentClip = clip
            if (endFrame < startFrame) isPlayDirectionReversed = true;
            else isPlayDirectionReversed = false;

            startTime = TimeSpan.FromMilliseconds(startFrame / fps * 1000);
            endTime = TimeSpan.FromMilliseconds(endFrame / fps * 1000);
            currentTime = startTime;
            isLooping = loop;
            //keyframeList = currentClip.Keyframes;
        }

        public void switchRange(float startFrame, float endFrame)
        {
            isSwitching = true;

            if (endFrame < startFrame) isPlayDirectionReversed = true;
            else isPlayDirectionReversed = false;

            startTimeSwitch = TimeSpan.FromMilliseconds(startFrame / fps * 1000);
            endTimeSwitch = TimeSpan.FromMilliseconds(endFrame / fps * 1000);
        }

        public bool inRange(float startFrame, float endFrame)
        {
            TimeSpan sRange = TimeSpan.FromMilliseconds(startFrame / fps * 1000);
            TimeSpan eRange = TimeSpan.FromMilliseconds(endFrame / fps * 1000);
            if (currentTime >= sRange && currentTime <= eRange)
                return true;
            else
                return false;
        }

        public Matrix getWorldTransform(int id)
        {
            return worldTransforms[id];
        }

        private Matrix[] GetTransformsFromTime(TimeSpan ts)
        {
            Matrix[] xforms = new Matrix[skinData.BindPose.Count];
            skinData.BindPose.CopyTo(xforms, 0);
            int keyNum = 0;
            while (keyNum < keyframeList.Count)
            {
                Keyframe key = keyframeList[keyNum];
                if (key.Time > ts) break;
                xforms[key.Bone] = key.Transform;
                keyNum++;
            }
            return xforms;
        }

        private Matrix[] GetTransformsFromTime(float a)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(a / fps * 1000);
            return GetTransformsFromTime(ts);
        }

        private Matrix[] BlendTransforms(Matrix[] fromTransforms,
                                        Matrix[] toTransforms)
        {
            for (int i = 0; i < fromTransforms.Length; i++)
            {
                Vector3 vt1; Vector3 vs1; Quaternion q1;
                fromTransforms[i].Decompose(out vs1, out q1, out vt1);

                Vector3 vt2; Vector3 vs2; Quaternion q2;
                toTransforms[i].Decompose(out vs2, out q2, out vt2);

                Vector3 vtBlend = Vector3.Lerp(vt1, vt2, blend);
                Vector3 vsBlend = Vector3.Lerp(vs1, vs2, blend);
                Quaternion qBlend = Quaternion.Slerp(q1, q2, blend);

                toTransforms[i] = Matrix.CreateScale(vsBlend) *
                            Matrix.CreateFromQuaternion(qBlend) *
                            Matrix.CreateTranslation(vtBlend);
            }
            return toTransforms;
        }

        public void Update(GameTime gameTime, Matrix root)
        {
            if (!isPlayDirectionReversed)
                currentTime += gameTime.ElapsedGameTime;
            else currentTime -= gameTime.ElapsedGameTime;
            
            boneTransforms = GetTransformsFromTime(currentTime);

            if ( (!isPlayDirectionReversed && currentTime >= endTime) || (isPlayDirectionReversed && currentTime<=endTime))
            {
                if (isLooping)
                    currentTime = startTime;
                else
                    currentTime = endTime;
            }

            if (isSwitching)
            {
                blend += 0.1f;
                boneTransforms = BlendTransforms(boneTransforms,
                                GetTransformsFromTime(startTimeSwitch));
                if (blend > 1)
                {
                    isSwitching = false;
                    startTime = startTimeSwitch;
                    endTime = endTimeSwitch;
                    currentTime = startTime;
                    blend = 0;
                }

            }


            worldTransforms[0] = boneTransforms[0] * root;
            //adjust the children
            for (int i = 1; i < worldTransforms.Length; i++)
            {
                int parent = skinData.SkeletonHierarchy[i];
                worldTransforms[i] = boneTransforms[i] *
                                       worldTransforms[parent];
            }

            //update the skins
            for (int i = 0; i < skinTransforms.Length; i++)
            {
                skinTransforms[i] = skinData.InverseBindPose[i] *
                                    worldTransforms[i];
            }
        }

        public Matrix[] GetSkinTransforms()
        {
            return skinTransforms;
        }
    }
}
