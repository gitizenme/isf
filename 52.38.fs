/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/Ws3XWl by iq.  Carving a series of sphere fields of higher frequencies and smaller sizes. Unlike simply doing an FBM displacement on a solid, this yields a distance field (bound).",
    "IMPORTED": {
    },
    "INPUTS": [
    ]
}

*/


// The MIT License
// Copyright © 2019 Inigo Quilez
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


// This shader uses a a grid of spheres to carve out fractal detail from
// a solid block. Unlike naive SDF disaplcemente by a traditional fBM,
// this shader produces a field that is a valid SDF, so there's no need
// to reduce the raymarcher's step size to get artifact free visuals.
//
// The article that explains this technique can be found here:
//
//     https://iquilezles.org/www/articles/fbmsdf/fbmsdf.htm
//
// A additive synthesis example of this technique, here: 
//
//     https://www.shadertoy.com/view/3dGSWR



// 0 = lattice
// 1 = simplex
#define NOISE 0

float rand(float n){return fract(sin(n) * 43758.5453123);}

float rand(vec2 n) { 
	return fract(sin(dot(n, vec2(12.9898, 4.1414))) * 43758.5453);
}

float hash11x(float p){
	float fl = floor(p);
  float fc = fract(p);
	return mix(rand(fl), rand(fl + 1.0), fc);
}
	
float hash11(vec2 n) {
	const vec2 d = vec2(0.0, 1.0);
  vec2 b = floor(n), f = smoothstep(vec2(0.0), vec2(1.0), fract(n));
	return mix(mix(rand(b), rand(b + d.yx), f.x), mix(rand(b + d.xy), rand(b + d.yy), f.x), f.y);
}

float hash21(vec2 p){
	vec2 ip = floor(p);
	vec2 u = fract(p);
	u = u*u*(3.0-2.0*u);
	
	float res = mix(
		mix(rand(ip),rand(ip+vec2(1.0,0.0)),u.x),
		mix(rand(ip+vec2(0.0,1.0)),rand(ip+vec2(1.0,1.0)),u.x),u.y);
	return res*res;
}

float mod289(float x){return x - floor(x * (1.0 / 289.0)) * 289.0;}
vec4 mod289(vec4 x){return x - floor(x * (1.0 / 289.0)) * 289.0;}
vec4 perm(vec4 x){return mod289(((x * 34.0) + 1.0) * x);}

float hash31(vec3 p){
    vec3 a = floor(p);
    vec3 d = p - a;
    d = d * d * (3.0 - 2.0 * d);

    vec4 b = a.xxyy + vec4(0.0, 1.0, 0.0, 1.0);
    vec4 k1 = perm(b.xyxy);
    vec4 k2 = perm(k1.xyxy + b.zzww);

    vec4 c = k2 + a.zzzz;
    vec4 k3 = perm(c);
    vec4 k4 = perm(c + 1.0);

    vec4 o1 = fract(k3 * (1.0 / 41.0));
    vec4 o2 = fract(k4 * (1.0 / 41.0));

    vec4 o3 = o2 * d.z + o1 * (1.0 - d.z);
    vec2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);

    return o4.y * d.y + o4.x * (1.0 - d.y);
}

// please, do not use in real projects - replace this by something better
float hash(vec3 p)  
{
    return hash31(p);
}

// http://iquilezles.org/www/articles/distfunctions/distfunctions.htm
float sdBox( vec3 p, vec3 b )
{
    vec3 d = abs(p) - b;
    return min(max(d.x,max(d.y,d.z)),0.0) + length(max(d,0.0));
}

// http://iquilezles.org/www/articles/smin/smin.htm
float smax( float a, float b, float k )
{
    float h = max(k - abs(a - b), 0.0);
    return max(a, b) + h * h * 0.25 / k;
}

// http://iquilezles.org/www/articles/boxfunctions/boxfunctions.htm
vec2 iBox( in vec3 ro, in vec3 rd, in vec3 rad ) 
{
    vec3 m = 1.0 / rd;
    vec3 n = m * ro;
    vec3 k = abs(m) * rad;
    vec3 t1 = -n - k;
    vec3 t2 = -n + k;
	float tN = max( max( t1.x, t1.y ), t1.z );
	float tF = min( min( t2.x, t2.y ), t2.z );
	if(tN > tF || tF < 0.0) return vec2(-1.0);
	return vec2( tN, tF );
}

//---------------------------------------------------------------
// A random SDF - it places spheres of random sizes in a grid
//---------------------------------------------------------------

float sdBase( in vec3 p )
{
#if NOISE==0
    vec3 i = floor(p);
    vec3 f = fract(p);

	#define RAD(r) ((r) * (r) * 0.7)
    #define SPH(i, f, c) length(f - c) - RAD(hash(i + c))
    
    return min(min(min(SPH(i, f, vec3(0, 0, 0)),
                       SPH(i, f, vec3(0, 0, 1))),
                   min(SPH(i, f, vec3(0, 1, 0)),
                       SPH(i, f, vec3(0, 1, 1)))),
               min(min(SPH(i, f, vec3(1, 0, 0)),
                       SPH(i, f, vec3(1, 0, 1))),
                   min(SPH(i, f, vec3(1, 1, 0)),
                       SPH(i, f, vec3(1, 1, 1)))));
#else
    const float K1 = 0.333333333;
    const float K2 = 0.166666667;
    
    vec3 i = floor(p + (p.x + p.y + p.z) * K1);
    vec3 d0 = p - (i - (i.x + i.y + i.z) * K2);
    
    vec3 e = step(d0.yzx, d0);
	vec3 i1 = e * (1.0 - e.zxy);
	vec3 i2 = 1.0 - e.zxy *(1.0 - e);
    
    vec3 d1 = d0 - (i1  - 1.0 * K2);
    vec3 d2 = d0 - (i2  - 2.0 * K2);
    vec3 d3 = d0 - (1.0 - 3.0 * K2);
    
    float r0 = hash(i + 0.0);
    float r1 = hash(i + i1);
    float r2 = hash(i + i2);
    float r3 = hash(i + 1.0);

    #define SPH(d, r) length(d) -r *r * 0.55

    return min( min(SPH(d0, r0),
                    SPH(d1, r1)),
                min(SPH(d2, r2),
                    SPH(d3, r3)));
#endif
}

//---------------------------------------------------------------
// subtractive fbm
//---------------------------------------------------------------
vec2 sdFbm( in vec3 p, float d )
{
    const mat3 m = mat3( 0.00,  0.80,  0.60, 
                        -0.80,  0.36, -0.48,
                        -0.60, -0.48,  0.64 );
    float t = 0.0;
	float s = 1.0;
    for(int i=0; i<7; i++ )
    {
        float n = s * sdBase(p);
    	d = smax(d, -n, 0.2 * s );
        t += d;
        p = 2.0 * m * p;
        s = 0.5 * s;
    }
    
    return vec2(d, t);
}

vec2 map( in vec3 p )
{
    // box
    float d = sdBox(p, vec3(1.0));

    // fbm
    vec2 dt = sdFbm( p+0.5, d);

    dt.y = 1.0+dt.y*2.0; dt.y = dt.y*dt.y;
    
    return dt;
}

const float precis = 0.0005;

vec2 raycast( in vec3 ro, in vec3 rd )
{
	vec2 res = vec2(-1.0);

    // bounding volume    
    vec2 dis = iBox( ro, rd, vec3(1.0) ) ;
    if( dis.y<0.0 ) return res;

    // raymarch
    float t = dis.x;
	for( int i=0; i<256; i++ )
	{
        vec3 pos = ro + t*rd;
		vec2 h = map( pos );
        res.x = t;
        res.y = h.y;
        
		if( h.x<precis || t>dis.y ) break;
		t += h.x;
	}

	if( t>dis.y ) res = vec2(-1.0);
	return res;
}

// http://iquilezles.org/www/articles/normalsSDF/normalsSDF.htm
vec3 calcNormal( in vec3 pos )
{
    vec2 e = vec2(1.0,-1.0)*0.5773*precis;
    return normalize( e.xyy*map( pos + e.xyy ).x + 
					  e.yyx*map( pos + e.yyx ).x + 
					  e.yxy*map( pos + e.yxy ).x + 
					  e.xxx*map( pos + e.xxx ).x );
}

// http://iquilezles.org/www/articles/rmshadows/rmshadows.htm
float calcSoftShadow(vec3 ro, vec3 rd, float tmin, float tmax, float w)
{
    // bounding volume    
    vec2 dis = iBox( ro, rd, vec3(1.0) ) ;
    if( dis.y<0.0 ) return 1.0;
    
    tmin = max(tmin,dis.x);
	tmax = min(tmax,dis.y);
    
    float t = tmin;
    float res = 1.0;
    for( int i=0; i<128; i++ )
    {
     	float h = map(ro + t*rd).x;
        res = min( res, h/(w*t) );
    	t += clamp(h, 0.005, 0.50);
        if( res<-1.0 || t>tmax ) break;
    }
    res = max(res,-1.0); // clamp to [-1,1]

    return 0.25*(1.0+res)*(1.0+res)*(2.0-res); // smoothstep
}

#define HW_PERFORMANCE 0
#if HW_PERFORMANCE==0
#define AA 1
#else
#define AA 1   // make this 2 or 3 for antialiasing
#endif

#define ZERO min(FRAMEINDEX,0)

void main() {



    vec3 tot = vec3(0.0);
    
    #if AA>1
    for( int m=ZERO; m<AA; m++ )
    for( int n=ZERO; n<AA; n++ )
    {
        // pixel coordinates
        vec2 o = vec2(float(m),float(n)) / float(AA) - 0.5;
        vec2 p = (2.0*(gl_FragCoord.xy+o)-RENDERSIZE.xy)/RENDERSIZE.y;
        float d = 0.5*sin(gl_FragCoord.x*147.0)*sin(gl_FragCoord.y*131.0);
        #else    
        vec2 p = (2.0*gl_FragCoord.xy-RENDERSIZE.xy)/RENDERSIZE.y;
        #endif
   
        // camera anim
        float an = -0.1*TIME;
        vec3 ro = 4.0*vec3( cos(an), 0.4, sin(an) );
        vec3 ta = vec3( 0.0, -0.35, 0.0 );
        // camera matrix	
        vec3  cw = normalize( ta-ro );
        vec3  cu = normalize( cross(cw,vec3(0.0,1.0,0.0)) );
        vec3  cv = normalize( cross(cu,cw) );
        vec3  rd = normalize( p.x*cu + p.y*cv + 2.7*cw );
        // render
        vec3 col = vec3(0.01);
        vec2 tm = raycast( ro, rd );
        float t = tm.x;
        if( t>0.0 )
        {
            vec3  pos = ro + t*rd;
            vec3  nor = calcNormal( pos );
            float occ = tm.y*tm.y;
            // material
            vec3 mate = mix( vec3(0.6,0.3,0.1), vec3(1), tm.y )*0.7;
            // key light
            {
            vec3 lig = normalize(vec3(1.0,0.5,0.6));
            float dif = dot(lig,nor);
            if( dif>0.0 ) dif *= calcSoftShadow(pos+nor*0.001,lig,0.001,10.0,0.003);
            dif = clamp(dif,0.0,1.0);
            vec3 hal = normalize(lig-rd);
            float spe = clamp(dot(hal,nor),0.0,1.0);
            spe = pow(spe,4.0)*dif*(0.04+0.96*pow(max(1.0-dot(hal,lig),0.0),5.0));
            col = vec3(0.0);
            col += mate*1.5*vec3(1.30,0.85,0.75)*dif;
            col +=      9.0*spe;
            }
            // ambient light
            {
            col += mate*0.2*vec3(0.40,0.45,0.60)*occ*(0.6+0.4*nor.y);
            }
        }
        // tonemap
        col = col*1.7/(1.0+col);
        
        // gamma
        col = pow(col,vec3(0.4545));
        
        tot += col;
	#if AA>1
    }
    tot /= float(AA*AA);
    #endif
    // vignetting
    vec2 q = gl_FragCoord.xy/RENDERSIZE.xy;
    tot *= 0.7 + 0.3*pow(16.0*q.x*q.y*(1.0-q.x)*(1.0-q.y),0.2);
    
    // cheap dithering
    tot += sin(gl_FragCoord.x*114.0)*sin(gl_FragCoord.y*211.1)/512.0;
    gl_FragColor=vec4(tot,1.0);
}
