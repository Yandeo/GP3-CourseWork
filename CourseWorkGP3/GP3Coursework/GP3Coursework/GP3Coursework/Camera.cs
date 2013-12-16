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
using Microsoft.Xna.Framework.Graphics;

namespace GP3Coursework
{

    class Camera
    {

        public Matrix projectionMatrix;
        public Matrix worldMatrix;


        public Matrix camViewMatrix;        //Cameras view
        public Matrix camRotationMatrix;    //Rotation Matrix for camera to reflect movement around Y Axis
        public Vector3 camPosition;         //Position of Camera in world
        public Vector3 camLookat;           //Where the camera is looking or pointing at
        public Vector3 camTransform;        //Used for repositioning the camer after it has been rotated
        public float camRotationSpeed;      //Defines the amount of rotation
        public float camYaw;                //Cumulative rotation on Y
        public int position;


        public Camera(Vector3 position, Vector3 Lookat)
        {
            camPosition = position;
            camLookat = Lookat;
        }

        public void InitializeTransform(GraphicsDeviceManager graphics)
        {
            //viewMatrix = Matrix.CreateLookAt(new Vector3(-2, 3, 10),Vector3.Zero,Vector3.Up);
            //camPosition = new Vector3(position, 0, 10);
            //camLookat = Vector3.Zero;
            camViewMatrix = Matrix.CreateLookAt(camPosition, camLookat, Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), (float)graphics.GraphicsDevice.Viewport.Width / (float)graphics.GraphicsDevice.Viewport.Height, 1.0f, 1000.0f);

            worldMatrix = Matrix.Identity;

            camRotationSpeed = 1f / 60f;
        }

        public void camUpdate()
        {
            camRotationMatrix = Matrix.CreateRotationY(camYaw);
            camTransform = Vector3.Transform(Vector3.Forward, camRotationMatrix);
            camLookat = camPosition + camTransform;
            camViewMatrix = Matrix.CreateLookAt(camPosition, camLookat, Vector3.Down);
        }


    }
}


