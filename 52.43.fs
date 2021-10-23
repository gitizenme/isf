/*{
    "CATEGORIES": [
        "generator",
        "ball"
    ],
    "CREDIT": "by ChaosOfZen",
    "DESCRIPTION": "Color Ball",
    "INPUTS": [
        {
            "MAX": [
                1,
                1
            ],
            "MIN": [
                -1,
                -1
            ],
            "NAME": "offset",
            "TYPE": "point2D"
        },
        {
            "DEFAULT": 0,
            "MAX": 2,
            "MIN": -2,
            "NAME": "rotation",
            "TYPE": "float"
        },
        {
            "DEFAULT": 5.5,
            "MAX": 15,
            "MIN": 1,
            "NAME": "size",
            "TYPE": "float"
        },
        {
            "DEFAULT": 0.1,
            "MAX": 0.15,
            "MIN": 0.03,
            "NAME": "depth",
            "TYPE": "float"
        },
        {
            "DEFAULT": 2.5,
            "MAX": 4,
            "MIN": 0.1,
            "NAME": "density",
            "TYPE": "float"
        },
        {
            "DEFAULT": -3,
            "MAX": 9,
            "MIN": -9,
            "NAME": "rateX",
            "TYPE": "float"
        },
        {
            "DEFAULT": -0.5,
            "MAX": 9,
            "MIN": -9,
            "NAME": "rateY",
            "TYPE": "float"
        },
        {
            "DEFAULT": 3,
            "MAX": 9,
            "MIN": -9,
            "NAME": "rateZ",
            "TYPE": "float"
        },
        {
            "DEFAULT": [
                1,
                1,
                1,
                1
            ],
            "LABEL": "baseColor",
            "NAME": "baseColor",
            "TYPE": "color"
        },
        {
            "DEFAULT": [
                0.5,
                0.5,
                0.5,
                1
            ],
            "LABEL": "backgroundColor",
            "NAME": "backgroundColor",
            "TYPE": "color"
        }
    ],
    "ISFVSN": "2"
}
*/

#define saturate(oo) clamp(oo, 0.0, 1.0)
#define MarchSteps 4
#define Radius 1.5
#define NoiseSteps 4
#define Color1 baseColor
#define Color2 vec4(1.0, 0.8, 0.2, 1.0)
#define Color3 vec4(1.0, 0.03, 0.0, 1.0)
#define Color4 vec4(0.4, 0.02, 0.02, 1.0)

float maxFreq = 1.5;
float qWidth = 0.03;
float minFreq = 0.1;


vec3 mod196(vec3 x) { return x - floor(x * (1.0 / 196.0)) * 196.0; }
vec4 mod196(vec4 x) { return x - floor(x * (1.0 / 196.0)) * 196.0; }
vec4 permute(vec4 x) { return mod196(((x*56.0)+1.0)*x); }
vec4 taylorInvSqrt(vec4 r){ return 1.79284291400159 - 0.85373472095314 * r; }

float snoise(vec3 v)
{
	const vec2  C = vec2(1.0/6.0, 1.0/3.0);
	const vec4  D = vec4(0.0, 0.5, 1.0, 2.0);

	vec3 i  = floor(v + dot(v, C.yyy));
	vec3 x0 = v - i + dot(i, C.xxx);
	vec3 g = step(x0.yzx, x0.xyz);
	vec3 l = 1.0 - g;
	vec3 i1 = min(g.xyz, l.zxy);
	vec3 i2 = max(g.xyz, l.zxy);
	vec3 x1 = x0 - i1 + C.xxx;
	vec3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
	vec3 x3 = x0 - D.yyy;      // -1.0+3.0*C.x = -0.5 = -D.y
 
 	i = mod196(i);
	vec4 p = permute( permute( permute( i.z + vec4(0.0, i1.z, i2.z, 1.0)) + i.y + vec4(0.0, i1.y, i2.y, 1.0 )) + i.x + vec4(0.0, i1.x, i2.x, 1.0 ));

	float n_ = 0.142857142857;
	vec3  ns = n_ * D.wyz - D.xzx;
	vec4 j = p - 49.0 * floor(p * ns.z * ns.z);  
	vec4 x_ = floor(j * ns.z);
	vec4 y_ = floor(j - 7.0 * x_);    
	vec4 x = x_ *ns.x + ns.yyyy;
	vec4 y = y_ *ns.x + ns.yyyy;
	vec4 h = 1.0 - abs(x) - abs(y);
	vec4 b0 = vec4(x.xy, y.xy);
	vec4 b1 = vec4(x.zw, y.zw);
	vec4 s0 = floor(b0) * 2.0 + 1.0;
	vec4 s1 = floor(b1) * 2.0 + 1.0;
	vec4 sh = -step(h, vec4(0.0));
	vec4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
	vec4 a1 = b1.xzyw + s1.xzyw * sh.zzww;
	vec3 p0 = vec3(a0.xy, h.x);
	vec3 p1 = vec3(a0.zw, h.y);
	vec3 p2 = vec3(a1.xy, h.z);
	vec3 p3 = vec3(a1.zw, h.w);

	vec4 norm = taylorInvSqrt(vec4(dot(p0,p0), dot(p1,p1), dot(p2, p2), dot(p3,p3)));
	p0 *= norm.x;
	p1 *= norm.y;
	p2 *= norm.z;
	p3 *= norm.w;
	vec4 m = max(0.6 - vec4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
	m = m * m;

	return 35.0 * dot( m*m, vec4( dot(p0,x0), dot(p1,x1), dot(p2,x2), dot(p3,x3)));
}

float Turbulence(vec3 position, float minFreq, float maxFreq, float qWidth)
{
	float value = 0.0;
	float cutoff = clamp(0.5/qWidth, 0.0, maxFreq);
	float fade;
	float fOut = minFreq;
	for(int i=NoiseSteps ; i>=0 ; i--)
	{
		if(fOut >= 0.5 * cutoff) break;
		fOut *= 2.0;
		value += abs(snoise(position * fOut))/fOut;
	}
	fade = clamp(2.0 * (cutoff-fOut)/cutoff, 0.0, 1.0);
	value += fade * abs(snoise(position * fOut))/fOut;

	return 1.0-value;
}

float SphereDist(vec3 position)
{
	return length(position) - Radius;
}

vec4 Shade(float distance)
{
	// float c1 = saturate(distance * 5.0 + 0.5);
	// float c2 = saturate(distance * 5.0);
	// float c3 = saturate(distance * 3.4 - 0.5);

	float sc1 = clamp(distance * 5.0 + 0.5, 0., smoothstep(0.01, 0.08, distance));
	float sc2 = clamp(distance * 5.0, 0., smoothstep(0.01, 0.08, distance));
	float sc3 = clamp(distance * 3.4 - 0.5, 0., smoothstep(0.01, 0.08, distance));

	vec4 a = mix(Color1, Color2, sc1);
	vec4 b = mix(a, Color3, sc2);
	vec4 c = mix(b, Color4, sc3);
	return c; 
}

float RenderScene(vec3 position, out float distance)
{
	float noise = Turbulence(position * density + vec3(rateZ, rateX, rateY), minFreq, maxFreq, qWidth) * depth;
	noise = saturate(abs(noise));
	distance = SphereDist(position) - noise;
		
	return noise;
}

vec4 March(vec3 rayOrigin, vec3 rayStep, vec4 bgCol)
{
	vec3 position = rayOrigin;
	float distance;
	float displacement;
	for(int step = MarchSteps; step >=0  ; --step)
	{
		displacement = RenderScene(position, distance);
		if(distance < 0.05) break;
		position += rayStep * distance;
	}

	return mix(Shade(displacement), bgCol, float(distance >= 0.5));
}

bool IntersectSphere(vec3 ro, vec3 rd, vec3 pos, float radius, out vec3 intersectPoint)
{
	vec3 relDistance = (ro - pos);
	float b = dot(relDistance, rd);
	float c = dot(relDistance, relDistance) - radius*radius;
	float d = b*b - c;
	intersectPoint = ro + rd*(-b - sqrt(d));
	
	return d >= 0.0;
}

mat2 rotate2d(float _angle){
    return mat2(cos(_angle),-sin(_angle),
                sin(_angle),cos(_angle));
}

void main(void)
{
	vec2 p = (gl_FragCoord.xy / RENDERSIZE.xy) * 2.0 - 1.0;
	// p += offset;
	p.x *= RENDERSIZE.x/RENDERSIZE.y;

	p.xy *= rotate2d(sin(offset.x * TIME) + cos(offset.y * TIME));

	float rotx = rotation * 4.0;
	float roty = -rotation * 4.0;
	float zoom = 10.0 - (size);
	vec3 ro = zoom * normalize(vec3(cos(roty), cos(rotx), sin(roty)));
	vec3 ww = normalize(vec3(0.0, 0.0, 0.0) - ro);
	vec3 uu = normalize(cross( vec3(0.0, 1.0, 0.0), ww));
	vec3 vv = normalize(cross(ww, uu));
	vec3 rd = normalize(p.x*uu + p.y*vv + 1.5*ww);
	vec4 col = vec4(backgroundColor.r, backgroundColor.g, backgroundColor.b, 1.0);
	vec3 origin;
	vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
	uv *=  1.0 - uv.yx;
	float vig = uv.x * uv.y * 15.0; // multiply with sth for intensity
	vig = pow(vig, 0.25); // change pow for modifying the extend of the  vignette
	col *= vec4(backgroundColor.r, backgroundColor.g, backgroundColor.b, 1.);
	col *= vig;
	if(IntersectSphere(ro, rd, vec3(0.0), Radius + depth*7.0, origin))
	{
		col = March(origin, rd, col);
	}
	
	gl_FragColor = col;
}