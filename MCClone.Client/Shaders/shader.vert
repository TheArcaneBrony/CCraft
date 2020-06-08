varying vec4 position; 
           // this is a varying variable in the vertex shader
         
        void main()
        {
            position = 0.5 * (gl_Vertex + vec4(1.0, 1.0, 1.0, 0.0));
               // Here the vertex shader writes output(!) to the 
               // varying variable. We add 1.0 to the x, y, and z 
               // coordinates and multiply by 0.5, because the 
               // coordinates of the cube are between -1.0 and 1.0 
               // but we need them between 0.0 and 1.0 
            gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
         }