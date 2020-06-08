varying vec4 position; 
            // this is a varying variable in the fragment shader
         
         void main()
         {
            gl_FragColor = position;
               // Here the fragment shader reads intput(!) from the 
               // varying variable. The red, gree, blue, and alpha 
               // component of the fragment color are set to the 
               // values in the varying variable. (The alpha 
               // component of the fragment doesn't matter here.) 
         }