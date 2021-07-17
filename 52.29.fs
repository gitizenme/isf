/*{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/7lXXWj by ChaosOfZen.  Just experiment",
    "IMPORTED": {
    },
    "INPUTS": [
        {
            "DEFAULT": 1,
            "LABEL": "M1",
            "MAX": 1,
            "MIN": 0,
            "NAME": "M1",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.9,
            "LABEL": "M2",
            "MAX": 1,
            "MIN": 0,
            "NAME": "M2",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.5,
            "LABEL": "R1",
            "MAX": 1,
            "MIN": 0,
            "NAME": "R1",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.5,
            "LABEL": "R2",
            "MAX": 1,
            "MIN": 0,
            "NAME": "R2",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.5,
            "LABEL": "V1",
            "MAX": 1,
            "MIN": 0,
            "NAME": "V1",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.95,
            "LABEL": "V2",
            "MAX": 1,
            "MIN": 0,
            "NAME": "V2",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.095,
            "LABEL": "burn",
            "MAX": 1,
            "MIN": 0.1,
            "NAME": "burn",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.095,
            "LABEL": "blend",
            "MAX": 1,
            "MIN": 0,
            "NAME": "blend",
            "TYPE": "float"
        },
        {
            "DEFAULT": [
                0.3333333333333333,
                0.6666666666666666,
                1,
                1
            ],
            "LABEL": "color",
            "NAME": "color",
            "TYPE": "color"
        },
        {
            "DEFAULT": [
                0,
                0,
                0,
                1
            ],
            "LABEL": "background",
            "NAME": "background",
            "TYPE": "color"
        }
    ],
    "ISFVSN": "2"
}
*/


#define MAX_ITER 250
// #define M1 1.0
// #define M2 0.9
// #define R1 0.5
// #define R2 0.5
// #define V1 0.5
// #define V2 0.95

vec2 rotate(vec2 uv, float radians)
{
  mat2 rot = mat2(cos(radians), -sin(radians),
                   sin(radians), cos(radians));
 
  return uv.xy * rot;
}


float bpm = 138.; // beats per minute
float bbpm = 4.;  // beats per measure


void main() {
    vec2 uv = ( gl_FragCoord.xy - .5 * RENDERSIZE.xy ) / RENDERSIZE.y;

    float spm = (bbpm * 60. / bpm) / 4.; // seconds per measure

    float timeScale = TIME / spm;
    uv = rotate(uv, timeScale / 4.);   

    vec2 z = vec2(0.0, 0.0);
	float p = 0.0;
	float dist = 0.0;

	float x1 = tan(timeScale);
	float y1 = sin(timeScale);
	float x2 = tan(timeScale);
	float y2 = sin(timeScale);


	for (int i=0; i < MAX_ITER; ++i)
	{
		z *= 2.0;
		z = mat2(z, -z.y, z.x) * z + uv;
		p = M1 / sqrt((z.x - x1) * (z.x - x1) + (z.y - y1) * (z.y - y1)) + M2 / sqrt((z.x - x2) * (z.x - x2) + (z.y - y2) * (z.y - y2));
		dist = max(dist, p);
	}

	dist *= burn;

    vec4 fColor = background;
    vec4 sColor = color;
    if(dist > blend) {
        sColor.r = sin(timeScale / 4.);
        sColor.g = cos(timeScale / 2.);
        sColor.b = tan(timeScale / 16.);
    }
    else {
        sColor.r = tan(timeScale / 16.);
        sColor.g = sin(timeScale / 4.);
        sColor.b = cos(timeScale / 2.);
    }

    
    fColor += vec4(dist * sColor.r, dist * sColor.g, dist * sColor.b, 1.0);
    fColor.rgb = clamp(vec3(0.0001), vec3(0.9999), fColor.rgb) + 0.01;

	gl_FragColor = fColor;
}
