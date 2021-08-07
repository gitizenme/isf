/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/7lSXRt by ChaosOfZen.  Simple SDF morphing example",
    "IMPORTED": {
    },
    "INPUTS": [
    ]
}

*/


// Made by Darko Supe (omegasbk)
// 27.6.2021.
// This example shows how easy it is to 
// morph shapes using signed distance function 
// interpolation.

#define MAX_STEPS 100
#define MAX_DIST 100.
#define SURF_DIST .005

#define PI 3.14159265359

const vec3 lightPosition = vec3(0, 3, 4);
const vec3 lightColor    = vec3(0.2, 0.2, 0.2) * 2.;


float sdEllipsoid( vec3 p, vec3 r )
{
  float k0 = length(p/r);
  float k1 = length(p/(r*r));
  return k0*(k0-1.0)/k1;
}

float getDistSphere(in vec3 p, in float r) 
{
    vec4 s = vec4(0, 0, 0, r);
    
    float sphereDist = length(p - s.xyz) - s.w;    
    return sphereDist;
}

float getDistCube(in vec3 p) 
{
    vec3 b = vec3(0.8);
    vec3 q = abs(p) - b;
    return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0);
}

float getDist(in vec3 p)
{
    float d = 0.;

    float ellipse1 = sdEllipsoid(p, vec3(2., .5, 1.25));
    float ellipse2 = sdEllipsoid(p, vec3(0.4, 1.6, 1.2));

    d = min(ellipse1, ellipse2);

    return d;
}

vec3 getNormal(in vec3 p) 
{
    int body;
    float d = getDist(p);
    
    vec2 e = vec2(.01, 0); 
    
    vec3 n = d - vec3(
        getDist(p - e.xyy), 
        getDist(p - e.yxy), 
        getDist(p - e.yyx));
        
    return normalize(n);
}

float rayMarch(in vec3 ro, in vec3 rd)
{
    float dO = 0.;
    
    for (int i = 0; i < MAX_STEPS; i++)
    {
        vec3 p = ro + rd * dO;
        float dS = getDist(p);
        dO += dS;
        
        if (dO > MAX_DIST || dS < SURF_DIST) 
            break;
    }
    
    return dO;
}

vec3 getLight(in vec3 p, in vec3 rd)
{    
	vec3 normal = getNormal(p);
    vec3 lightDir = normalize(p - lightPosition);
    
    float cosa = pow(0.5+0.5*dot(normal, -lightDir), 3.0);
    float cosr = max(dot(-rd, reflect(lightDir, normal)), 0.0);
    
    vec3 ambiant = vec3(0.42);
    vec3 diffuse = vec3(0.8 * cosa);
    vec3 phong = vec3(0.5 * pow(cosr, 16.0));
    
    return lightColor * (ambiant + diffuse + phong);
}

void main() {



    float time = TIME * 3.;
    // Normalized pixel coordinates (from 0 to 1)
    vec2 uv = (gl_FragCoord.xy - 0.5 * RENDERSIZE.xy)/RENDERSIZE.y;
    vec3 col = vec3(0);
    vec3 ro = vec3(0., 0., 0.);
    
    // Camera movement
    float focalLength = 4.;
    float rotation = TIME / 2.;
    ro = vec3(cos(rotation), 0., sin(rotation)) * focalLength;
    vec3 rd = normalize(vec3(0.) - ro);
    vec3 right = normalize(cross(rd, vec3(0., 1., 0.)));
    vec3 up = cross(right, rd);
   	rd =  normalize(uv.x * right + uv.y * up + rd);
        
    int body;
    float d = rayMarch(ro, rd);
    if (d < MAX_DIST)
    {
        vec3 p = ro + rd * d;   
        
        vec3 n = getNormal(p);
        col = vec3(getLight(p, rd)) * 2.; 
        col *= vec3(1, 0, 0);
    }
    else
    {
        col = 0.5 + 0.5 * sin(TIME * 0.5 + uv.yxy + vec3(0,4,8));
    }
    
    // Output to screen
    gl_FragColor = vec4(col,1.0);
}
