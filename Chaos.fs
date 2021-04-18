/*{
    "CATEGORIES": [
        "Chaos",
        "Symbol"
    ],
    "CREDIT": "Chaos.of.Zen",
    "DESCRIPTION": "Shader of the symbol for Chaos",
    "IMPORTED": {
    },
    "ISFVSN": "2"
}
*/



#define PI 3.14159
#define TWO_PI 2.*PI

// geometry functions from iq / mercury
void drawArrows(inout vec2 p, float c) {
	float m = TWO_PI / c;
	float a = mod(atan(p.x, p.y) - m*.5, m) - m*.5;
	p = vec2(cos(a), sin(a)) * length(p);
}

mat2 radToDec(float a) {
	float c = cos(a), s = sin(a);
	return mat2(c, s, -s, c);
}

float box(vec3 p, vec3 b)
{
	vec3 d = abs(p) - b;
	return min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0));
}

float length8(vec2 p) {
	p = pow(p, vec2(8.));
	return pow(p.x + p.y, 1. / 8.);
}

float torus82(vec3 p, vec2 t)
{
	vec2 q = vec2(length(p.xz) - t.x, p.y);
	return length8(q) - t.y;
}

float cone(vec3 p, vec2 c)
{
	// c must be normalized
	float q = length(p.xy);
	return dot(c, vec2(q, p.z));
}

float clampSmooth(float a, float b, float r) {
	return clamp(.5 + .5*(b - a) / r, 0., 1.);
}

float smoothMin(float a, float b, float r) {
	float h = clampSmooth(a, b, r);
	return mix(b, a, h) - r*h*(1. - h);
}

float smoothMax(float a, float b, float r) {
	float h = clampSmooth(a, b, r);
	return mix(a, b, h) + r*h*(1. - h);
}

float de(vec3 p) {

	// p.xz *= radToDec(TIME);
	// p.zy *= radToDec(TIME);
	// p.xy *= radToDec(TIME);

	float torusRadiusOuter = .1; // TODO make var
	float torusRadiusInner = .1; // TODO make var
	float d = torus82(p.yzx, vec2(torusRadiusInner, torusRadiusOuter));

	float numberOfArrows = 8.; // TODO make var
	drawArrows(p.xy, numberOfArrows);
	d = smoothMin(d, box(p, vec3(1.3, .1, .1)), .1);

	p.x -= 1.7;
	p.xy *= radToDec(-PI*.5);

	d = smoothMin(d, max(cone(p.zxy, normalize(vec2(.4, .2))), -p.y - .4), .1);
	return d;
}

vec3 normal(in vec3 pos)
{
	vec2 e = vec2(1., -1.)*.5773*.0005;
	return normalize(e.xyy*de(pos + e.xyy) +
		e.yyx*de(pos + e.yyx) +
		e.yxy*de(pos + e.yxy) +
		e.xxx*de(pos + e.xxx));
}

void main() {
	vec2 q = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 uv = (q - .5) * RENDERSIZE.xx / RENDERSIZE.yx;
	vec3 ro = vec3(0, 0, 3.5);
	vec3 rd = normalize(vec3(uv, -1));
	vec3 p;
	float t = 0.;
	for (float i = 0.; i < 1.; i += .01) {
		p = ro + rd*t;
		float d = de(p);
		if (d<.001 || t>100.) break;
		t += d;
	}

	float backgoundAlpha = 1.0;
	vec4 backgroundColor = vec4(1., 1., 1., backgoundAlpha);
	vec4 col = backgroundColor;

	float shapeAlpha = 1.0;
	vec4 shapeColor = vec4(.0, .0, .0, shapeAlpha);
	if (t <= 100.) {
		col = shapeColor;
	}

	float brightnessAll = 0.5;
	float brightness = 0.5;
	float edgeBrightness = 0.1;
	float centerBrightness = 8.0;
	col *= brightness + brightnessAll*pow(centerBrightness*q.x*q.y*(1.0 - q.x)*(1.0 - q.y), edgeBrightness);
	
	float alpha = 1.;
	gl_FragColor = vec4(col);
}
