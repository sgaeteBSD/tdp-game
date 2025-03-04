Shader "Unlit/TestShader"
{
    // a note:
    // for possible functions n such, https://developer.download.nvidia.com/cg/index_stdlib.html
    // this is what we can use


    // every material has a shader!
    // the material can change properties of the shader
    // two materials can use the same shader, but one material might make color "blue" while the other does "red"
    Properties
    {
        // allows a float value to show up when we place this shader onto a material
        _Value ("Value", Float) = 1.0
        _Color ("Color", Color) = (1,1,1,1)
        _ColorA ("ColorA", Color) = (1,1,1,1)
        _ColorB ("ColorB", Color) = (1,1,1,1)
        _ColorStart ("Color Start", Range(0, 1)) = 0
        _ColorEnd ("Color End", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {

            // TRANSPARENCY BLENDING:
                // all transparency blending goes down with THIS EQUATION:
                ///// src * A +- dst * B ///////
                // where src is the opaque color and dst is whatever's behind the object
                // we can modify A, B, and change whether we're adding or subtracting. that is how we get whatever effect we want
                // ADDITIVE BLENDING is good for flashy/lighting effects
                //// to achieve additive blending, we do src * 1 + dst * 1
                // MULTIPLICATIVE BLENDING
                //// set A to dst and B to 0, meaning it's: src * dst + 0

                // BLENDING MODES are, however, defined in the pass.
                // you add it by, before CGPROGRAM, just saying something like "Blend One One" for, say, additive blending
                
                // HOWEVER, transparency creates the issue of the depth buffer..... as in it has sorting issues on when to render the stuff behind it
                // "ZWrite Off" tells that we don't want to write to the depth buffer
                // also change the tags! it should be "RenderType"="Transparent" and you should add another one, "Queue"="Transparent"
                // ALSO CULLING!
                // if you want to be able to see both sides of a transparent object, you can do "Cull Off"
                // there's also "Cull Front" if you don't wanna see the front side, and "Cull Back" for default
                // ALSO ALSO ZTEST!
                // ZTest LEqual is default, ZTest Always makes it so it renders above everything, ZTest GEqual makes it so it only draws if it's behind something

            // Blend One One // additive
            // Blend DstColor Zero // multiply

            // from here to "ENDCG", we're now all in HLSL code! everything outside of this is ShaderLab
            CGPROGRAM
            // tells unity which functions we want to be our vertex shader and our fragment shader
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // a preprocessor "define", which makes it so any instance of "TAU" is going to be directly replaced with that number.
            // sounds like a variable, but it doesn't have any of the functionality of one.
            // basically speaking, when the program runs, it just goes through and replaces all instances of "TAU" with this number. nothing more, nothing less.
            #define TAU 6.28318530718

            float _Value; // automatically grabs the value from properties
            float4 _Color;
            float4 _ColorA;
            float4 _ColorB;
            float _ColorStart;
            float _ColorEnd;

            // usually called "appdata", but MeshData is a clearer name
            // the data is all per-vertex
            // automatically filled out by unity
            struct MeshData
            {
                // the colon is called a "semantic"
                // it tells the compiler that we want the position data to be passed into the field "vertex"
                // the normal is, of course, the normalized direction that the vertex is pointing.
                // it's used to make things look smooth, or not smooth.
                // TEXCOORD0 refers to uv channel 0, which is usually the first uv channel (as in, there are different channels for normal maps, lightmaps, etc...)
                // you can create more uvs, e.g. "uv1" and instead assign it with TEXCOORD1
                // if all you want to do is apply a texture to a mesh, you only really need vertex and uv positions :)

                float4 vertex : POSITION; // vertex position
                float3 normals : NORMAL;
                float2 uv0 : TEXCOORD0; // uv coordinates
            };
        

            // v2f is unity's default name for that gets passed from the vertex shader to the fragment shader
            // we could name it something else, however, like "Interpolators"
            // everything we're going to pass from the vertex shader to the fragment shader HAS to exist in this struct
            // we're calling these interpolators in this instance because there are WAY more fragments/pixels than there are vertices...
            // so, the fragment shader will have to interpolate data between vertices
            // for example, it will try to smoothly interpolate between two separate vertex normals pointing in different positions
            // this goes for any data between the two of them, though! 
            // e.g., if one vertex has red and a connected vertex has blue, it'll interpolate from red to blue, becoming purple inbetween
            struct Interpolators
            {
                float4 vertex : SV_POSITION; // clip space position of the vertex
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1; // for whatever reason, TEXCOORD0 doesn't have the same functionality in this struct than it does in MeshData
                // ^^ so it's basically just whatever data you want
                // if i wanna add anything else, we just continue on by adding TEXCOORD1, 2, 3...
            };


            // the vertex shader
            // controls the position of things
            // think minecraft shaders: leaves being pushed by wind, water flowing, the acid shader, etc...
            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex); // converts local space to clip space, used in the pipeline for rasterization & clipping
                // ^^ this is also what makes it move around in the camera! otherwise, it'd just be stuck there.
                // we could get rid of the unity function and just make it v.vertex if we wanted to see that
                // that's useful for something like post-processing shaders where you're covering the entire screen

                o.uv = v.uv0;

                o.normal = v.normals; // just passing data through! we can assign anything to this, unity wont scream at us. but let's keep it making sense
                // ^^ make sure, of course, that whatever you're passing through IS in fact declared inside of our MeshData struct
                // if i wanted this to be WORLD SPACE normals, i could just make it UnityObjectToWorldNormal(v.normals);
                // we could technically do that cast directly in our fragment shader, as well. it's faster to do it here, though
                return o;
            }

            // explained in the frag function! not in a usual shader program!
            float InverseLerp(float a, float b, float v) {
                return (v-a)/(b-a);
            }


            // A NOTE FOR FRAG FUNCTION:
            // as you've noticed, hlsl creates vectors by simply putting a number at the end of the datatype
            // half2, float3, etc... we can also use this naming condition for matrices, like float 4x4
            // "fixed", however, is a low precision datatype (where float is 32bit and half is 16bit, fixed is ~12bit?)
            // it's mostly good for data from -1 to 1, but those are the numbers where clip space thrives, so it goes nuts for that shit
            // in this case, though, we're changing the fixed4 datatype of frag to float4. 
            // idk why, im watching freya holmer for this info, and she just sorta did it lol

            // the fragment shader
            // controls the color of things
            // the semantic "SV_Target" is just telling the fragment shader that we need to output this to the frame buffer
            float4 frag (Interpolators i) : SV_Target
            {
                // hello world! this just returns the color red.
                // return float4(1,0,0,1);

                // we could also do "swizzling"
                // basically, in "otherValue", it can just take specific pieces of myValue and switch them around
                // so float2 otherValue = myValue.rg (or myValue.xy) will just take red and green...
                // or float4 otherValue = myValue.xxxx would take the x value and just replace every other value with the x value
                // float4 myValue;
                // float2 otherValue = myValue.xy;
                
                // since i.normal is a float3, when casting, it automatically populates xyz. all we need to do is set the fourth component, "w", aka "a"/alpha
                // to gain the normal data, we of course need to pass it in our vert function
                // return float4(i.normal, 1);

                // but what about gradients...
                // literally just lerp(). the third value determines which direction -- x for horiz, y for vert
                //  float4 outColor = lerp( _ColorA, _ColorB, i.uv.x );
                //  return outColor;

                // okay but what if i wanna control the start and end of the color and not just use the uv???
                // we inverse lerping...
                // float t = InverseLerp(_ColorStart, _ColorEnd, i.uv.x);
                // float4 outColor = lerp( _ColorA, _ColorB, t );
                // return outColor;
                // HOWEVER... shaders dont natively clamp everything, so this results in a bunch of other colors rather than just the two
                // clamping to the rescue, not allowing the color to go below 0 or above 1!
                // which FOR SOME REASON clamping is called "saturate". we just use it like we're casting.
                float t = saturate(InverseLerp(_ColorStart, _ColorEnd, i.uv.x));
                float4 outColor = lerp( _ColorA, _ColorB, t );
                return outColor;

                // this is unity's built in global time variable
                // x = t/20, y = t, z = t*2, w = t*3. these are just varying degrees of time for the sake of it where t = regular time
                // _Time.xyzw
            }
            ENDCG
        }
    }
}
