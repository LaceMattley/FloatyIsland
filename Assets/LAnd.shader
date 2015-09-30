    Shader "Custom/NewShader" {
    Properties {
      _MainTex ("Texture", 2D) = "white" {}
	  _HighTex ("Texture", 2D) = "white" {}
	  _DirtTex ("Dirt Texture", 2D) = "white" {}
	  _BlendFactor ("BlendFactor", float) = 5.0
	  _BlendDistance ("BlendDistance", float) = 10.0
	  _WorldPos ("WorldPos", Vector) = ( 0,0,0,0)
	  _LineThickness("Line thickness", float) =2.0
    }
    SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
	  #pragma target 3.0
      #pragma surface surf Lambert vertex:vert
      struct Input {
          float2 uv_MainTex;
		  float3 normal;
		  float3 worldPos;		  
      };


      void vert (inout appdata_full v, out Input o) {
          UNITY_INITIALIZE_OUTPUT(Input,o);
		  o.normal = v.normal;
		  
      }

	  sampler2D _HighTex;
      sampler2D _MainTex;
	  sampler2D _DirtTex;
	  float _BlendFactor;
	  float _LineThickness;
	  float _BlendDistance;
	  float4 _WorldPos;


      void surf (Input IN, inout SurfaceOutput o) {

		  float height = (IN.worldPos.y - _WorldPos.y);
		  float dotVal = saturate((1.0f-dot(float3(0.0f,1.0f,0.0f),IN.normal))/0.2f);
          float3 normalColor = lerp(tex2D (_MainTex, IN.uv_MainTex).rgb,tex2D (_DirtTex, IN.uv_MainTex).rgb,dotVal);
		  float3 highColor = tex2D (_HighTex, IN.uv_MainTex).rgb;
		  float lerpVal = (height-_BlendFactor)/_BlendDistance;
		  lerpVal += dotVal*0.5f;
		  lerpVal = saturate(lerpVal);

		  float localX = IN.worldPos.x - _WorldPos.x;
		  float localZ = IN.worldPos.z - _WorldPos.z;
		  float stripeValX = fmod(localX,1.0f);
		  float stripeValZ = fmod(localZ,1.0f);
		  if (abs(stripeValX) < _LineThickness || abs(stripeValZ) < _LineThickness )
		  {
			o.Albedo = float4(1.0f,1.0f,1.0f,1.0f);
		  }
		  else
		  {
			o.Albedo = lerp(normalColor, highColor, (lerpVal));          
		  }
      }
      ENDCG
    } 
    Fallback "Diffuse"
  }