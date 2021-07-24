/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/NtSXRW by ChaosOfZen.  This is the end result of an Art of Code video on YouTube",
    "IMPORTED": {
    },
    "INPUTS": [
        {
            "NAME": "iMouse",
            "TYPE": "point2D"
        }
    ]
}

*/



#define MAX_STEPS 100
#define MAX_DIST 100.
#define SURF_DIST .001

#define S(a, b, t) smoothstep(a, b, t)

mat2 Rot(float a) {
    float s = sin(a);
    float c = cos(a);
    return mat2(c, -s, s, c);
}

float sdBox(vec3 p, vec3 s) {
    p = abs(p)-s;
	return length(max(p, 0.))+min(max(p.x, max(p.y, p.z)), 0.);
}

float sdGyroid(vec3 p, float scale, float thickness, float bias) {
	p *= scale;
    return abs(dot(sin(p), cos(p.zxy))-bias)/scale-thickness;
}

vec3 Transform(vec3 p) {
    p.xy *= Rot(p.z*.15);
	p.z -= TIME*.1;
    p.y -= .3;
    return p;
}

float GetDist(vec3 p) {
    p = Transform(p);
    
    float box = sdBox(p, vec3(1));
   	
   	float g1 = sdGyroid(p, 5.23, .03, 1.4);
    float g2 = sdGyroid(p, 10.76, .03, .3);
    float g3 = sdGyroid(p, 20.76, .03, .3);
    float g4 = sdGyroid(p, 35.76, .03, .3);
    float g5 = sdGyroid(p, 60.76, .03, .3);
    float g6 = sdGyroid(p, 110.76, .03, .3);
    //float g = min(g1, g2); // union
    //float g = max(g1, -g2); // subtraction
    g1 -= g2*.4;
    g1 -= g3*.3;
    g1 += g4*.2;
    g1 += g5*.2;
    g1 += g6*.3;
    
    float d = g1*.8;//max(box, g1*.8);
    return d;
}

float RayMarch(vec3 ro, vec3 rd) {
	float dO=0.;
    
    for(int i=0; i<MAX_STEPS; i++) {
    	vec3 p = ro + rd*dO;
        float dS = GetDist(p);
        dO += dS;
        if(dO>MAX_DIST || abs(dS)<SURF_DIST) break;
    }
    
    return dO;
}

vec3 GetNormal(vec3 p) {
	float d = GetDist(p);
    vec2 e = vec2(.02, 0);
    
    vec3 n = d - vec3(
        GetDist(p-e.xyy),
        GetDist(p-e.yxy),
        GetDist(p-e.yyx));
    
    return normalize(n);
}

vec3 GetRayDir(vec2 uv, vec3 p, vec3 l, float z) {
    vec3 f = normalize(l-p),
        r = normalize(cross(vec3(0,1,0), f)),
        u = cross(f,r),
        c = p+f*z,
        i = c + uv.x*r + uv.y*u,
        d = normalize(i-p);
    return d;
}

vec3 Background(vec3 rd) {
	vec3 col = vec3(0);
    float t = TIME;
    
    float y = rd.y*.5+.5;
    
    col += (1.-y)*vec3(1, .4, .1)*2.;
    
    float a = atan(rd.x, rd.z);
    float flames = sin(a*10.+t)*sin(a*7.-t)*sin(a*6.);
    flames *= S(.8, .5, y);
    col += flames;
    col = max(col, 0.);
    col += S(.5, .0, y);
    return col;
}

void main() {



    vec2 uv = (gl_FragCoord.xy-.5*RENDERSIZE.xy)/RENDERSIZE.y;
	vec2 m = iMouse.xy/RENDERSIZE.xy;
    float t = TIME;
    vec3 col = vec3(0);
    
    uv += sin(uv*20.+t)*.01;
    
    vec3 ro = vec3(0, 0, -.03);
    ro.yz *= Rot(-m.y*3.14+1.);
    ro.xz *= Rot(-m.x*6.2831);
    
    vec3 lookat = vec3(0,0,0);
    vec3 rd = GetRayDir(uv, ro, lookat, .8);
    float d = RayMarch(ro, rd);
    
    if(d<MAX_DIST) {
    	vec3 p = ro + rd * d;
    	vec3 n = GetNormal(p);
        
        float height = p.y;
        
        p = Transform(p);
        
    	float dif = n.y*.5+.5;
    	col += dif*dif;  
        
        float g2 = sdGyroid(p, 10.76, .03, .3);
        col *= S(-.1, .1, g2);	// blackening
        
        float crackWidth = -.02+S(0., -.5, n.y)*.04;
        float cracks = S(crackWidth, -.03, g2);
        float g3 = sdGyroid(p+t*.1, 5.76, .03, .0);
        float g4 = sdGyroid(p-t*.05, 4.76, .03, .0);
        
        cracks *= g3*g4*20.+.2*S(.2, .0, n.y);
        
        col += cracks*vec3(1, .4, .1)*3.;
        float g5 = sdGyroid(p-vec3(0,t,0), 3.76, .03, .0);
        
        col += g5*vec3(1,.4,.1);
        
       col += S(0., -2., height)*vec3(1,.4,.1);
        
    }
    col = mix(col, Background(rd), S(0., 7., d));
    //col = Background(rd);
    
    col *= 1.-dot(uv,uv);
    gl_FragColor = vec4(col,1.0);
}
