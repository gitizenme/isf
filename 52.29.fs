/*
{
    "CATEGORIES": [
        "Automatically Converted",
        "Shadertoy"
    ],
    "DESCRIPTION": "Automatically converted from https://www.shadertoy.com/view/7lXXWj by ChaosOfZen.  Just experiment",
    "IMPORTED": {
    },
    "INPUTS": [
    ]
}

*/


#define MAX_ITER 250
#define m1 1.0
#define m2 0.9
#define r1 0.5
#define r2 0.5
#define v1 0.5
#define v2 0.95

vec2 rotate (vec2 vertex, float rads)
{
  mat2 tmat = mat2(cos(rads), -sin(rads),
                   sin(rads), cos(rads));
 
  return vertex.xy * tmat;
}



void main() {
    vec2 uv = ( gl_FragCoord.xy - .5*RENDERSIZE.xy ) / RENDERSIZE.y;

    uv = rotate(uv, 0.35 * TIME);

    vec2 z = vec2(0.0, 0.0);
	float p = 0.0;
	float dist = 0.0;
	float x1 = tan(TIME * v1) * r1;
	float y1 = sin(TIME * v1) * r1;
	float x2 = tan(TIME * v2) * r2;
	float y2 = sin(TIME * v2) * r2;

	for (int i=0; i < MAX_ITER; ++i)
	{
		z *= 2.0;
		z = mat2(z, -z.y, z.x) * z + uv;
		p = m1 / sqrt((z.x - x1) * (z.x - x1) + (z.y - y1) * (z.y - y1)) + m2/sqrt((z.x - x2) * (z.x - x2) + (z.y - y2) * (z.y - y2));
		dist = max(dist, p);
	}

	dist *= 0.0099;

	gl_FragColor = vec4(dist / 0.3, dist * dist / 0.03, dist / 0.112, 1.0);
}
