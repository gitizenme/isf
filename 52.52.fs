/*{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/slGXzm by ChaosOfZen.  a flare snowflake",
    "IMPORTED": {
    },
    "INPUTS": [
        {
            "DEFAULT": 0.01,
            "LABEL": "blur",
            "MAX": 1,
            "MIN": 0,
            "NAME": "Blur",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "LABEL": "Radius",
            "MAX": 1,
            "MIN": 0,
            "NAME": "Radius",
            "TYPE": "float"
        },
        {
            "DEFAULT": 8,
            "MAX": 32,
            "MIN": 1,
            "NAME": "Octaves",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.75,
            "LABEL": "Amplitude",
            "MAX": 1,
            "MIN": 0,
            "NAME": "Amplitude",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.9,
            "LABEL": "Frequency",
            "MAX": 1,
            "MIN": 0.9,
            "NAME": "Frequency",
            "TYPE": "float"
        },
        {
            "DEFAULT": [
                1,
                1,
                0,
                1
            ],
            "LABEL": "Color1",
            "NAME": "Color1",
            "TYPE": "color"
        },
        {
            "DEFAULT": [
                0,
                1,
                1,
                1
            ],
            "LABEL": "Color2",
            "NAME": "Color2",
            "TYPE": "color"
        },
        {
            "DEFAULT": [
                0,
                0,
                0,
                1
            ],
            "LABEL": "Background",
            "NAME": "Background",
            "TYPE": "color"
        }
    ],
    "ISFVSN": "2"
}
*/







#define PI 3.1415926
#define TWO_PI (PI * 2.)


float Circle(vec2 uv, vec2 p, float r, float blur)
{
    float d = length(uv-p);
    
    return smoothstep(r,r-blur,d);
}

float random(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

float noise (in vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);

    // Four corners in 2D of a tile
    float a = random(i);
    float b = random(i + vec2(1.0, 0.0));
    float c = random(i + vec2(0.0, 1.0));
    float d = random(i + vec2(1.0, 1.0));

    vec2 u = f * f * (3.0 - 2.0 * f);

    return mix(a, b, u.x) +
            (c - a)* u.y * (1.0 - u.x) +
            (d - b) * u.x * u.y;
}

#define OCTAVES 8
float fbm (in vec2 st) {
    // Initial values
    float value = 0.0;
    float amplitude = Amplitude; //.5;
    float frequency = Frequency; // 0.;
    //
    // Loop of octaves
    for (float i = 0.; i < Octaves; i += 1.) {
        value += amplitude * noise(st);
        value *= frequency;
        st *= frequency;
        amplitude *= Amplitude;
    }
    return value;
}

float blur = Blur; //.1;
float radius = Radius; //.1;

int bpm = 125;
float bbpm = 1. / 4.;  // beats per measure
float spm = (bbpm * (float(bpm) / 60.)); // seconds per measure

void main() {



    // Normalized pixel coordinates (from 0 to 1)
    vec2 uv = gl_FragCoord.xy/RENDERSIZE.xy;
    
    uv -= .5;
    uv.x *= RENDERSIZE.x/RENDERSIZE.y;
    float c1 = Circle(uv, vec2(.0,.0), 0., .3);
    float c2 = Circle(uv, vec2(.0,.0), 0., .5);

    float sTime = TIME * spm;

    for (float i = 0.; i < TWO_PI; i+=.9)
    {
        vec2 p = vec2(0. + sin(i + sTime) * fbm(uv + sTime) * 0.2,0. + cos(i + sTime) * fbm(uv + sTime) * 0.2);
        c1 += Circle(uv, p, radius, blur);
    }
    for (float i = 0.; i > -TWO_PI; i-=.9)
    {
        vec2 p = vec2(0. + sin(i - sTime) * fbm(uv + sTime) * .4,0. + cos(i - sTime) * fbm(uv + sTime) * .4);
        c2 += Circle(uv, p, radius, blur);
    }


    vec3 color = vec3(Background.rgb);

    // Time varying pixel color
    float cTime = clamp(sTime + Amplitude + Frequency, 0.01, 0.99);
    vec3 col1 = cos(cTime + uv.xyx + vec3(Color1.r, Color1.g, Color1.b)) * 1.3;
    vec3 col2 = sin(cTime + uv.xyx + vec3(Color2.r, Color2.g, Color2.b)) * 1.01;    

    // col2 = clamp(col2, 0.01, 0.99);

    // color += mix(c1 * col1, c2 * col2, 0.25);
    color += c1 * col1 * col1 * col1 + c2 * col2 * col2 * col2;

    // color.r = clamp(c1 * col1.r * col1.r * col1.r, 0.1, 0.9);
    // color.g = clamp(c1 * col1.g * col1.g * col1.g, 0.1, 0.9);
    // color.b = clamp(c1 * col1.b * col1.b * col1.b, 0.1, 0.9);


    // color = pow(color, vec3(1. / 1.8)) * 2.;
    // color = pow(color, vec3(2.)) * 3.;
    // color = pow(color, vec3(1. / 2.2));

    // color = pow(color, vec3(2.)) * 1.5;

    // color = pow(color, vec3(0.4545));

	vec2 uvV = gl_FragCoord.xy / RENDERSIZE.xy;
	uvV *=  1.0 - uvV.yx;
	float vig = uvV.x * uvV.y * 15.0; // multiply with sth for intensity
	vig = pow(vig, 0.35); // change pow for modifying the extend of the  vignette
	color = mix(color, vec3(vig), 0.3);

    // vec2 q = gl_FragCoord.xy / RENDERSIZE.xy;
    // color *= 0.7 + 0.3 * pow(16.0 * q.x * q.y * (1.0 - q.x) * (1.0 - q.y), 0.2);

    // color += sqrt( clamp(Background.rgb,0.0,1.0) );


    // Output to screen
    gl_FragColor = vec4(color, 1.0);
}


