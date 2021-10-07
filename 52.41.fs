/*
{
    "CATEGORIES": [
        "Bezier",
        "Particle"
    ],
    "DESCRIPTION": "Particle rotation fun",
    "IMPORTED": {
    },
    "INPUTS": [
    ]
}

*/

float SCALE = 0.5;
float SPEED = 0.25;
float INTENSITY = 20.0;
float LENGTH = 0.5;
float RADIUS = 0.020;
float FADING = 0.125;
float GLOW = 4.0;

#define M_2_PI 6.28318530

// optimized 2d version of https://www.shadertoy.com/view/ldj3Wh
vec2 sdBezier(vec2 pos, vec2 A, vec2 B, vec2 C)
{    
    vec2 a = B - A;
    vec2 b = A - 2.0*B + C;
    vec2 c = a * 2.0;
    vec2 d = A - pos;

    float kk = 1.0 / dot(b,b);
    float kx = kk * dot(a,b);
    float ky = kk * (2.0*dot(a,a)+dot(d,b)) / 3.0;
    float kz = kk * dot(d,a);      

    vec2 res;

    float p = ky - kx*kx;
    float p3 = p*p*p;
    float q = kx*(2.0*kx*kx - 3.0*ky) + kz;
    float h = q*q + 4.0*p3;

    h = sqrt(h);
    vec2 x = (vec2(h, -h) - q) / 2.0;
    vec2 uv = sign(x)*pow(abs(x), vec2(1.0/3.0));
    float t = clamp(uv.x+uv.y-kx, 0.0, 1.0);

    return vec2(length(d+(c+b*t)*t),t);
}

vec2 circle(float t){
    float x = SCALE * sin(t);
    float y = SCALE * cos(t);
    return vec2(x, y);
}

vec2 leminiscate(float t){
    float x = (SCALE * (cos(t) / (1.0 + sin(t) * sin(t))));
    float y = (SCALE * (sin(t) * cos(t) / (1.0 + sin(t) * sin(t))));
    return vec2(x, y);
}

// inspired by https://www.shadertoy.com/view/wdy3DD
float mapinfinite(vec2 pos,float sp){
    float t = fract(-SPEED * TIME*sp);
    float dl = LENGTH / INTENSITY;
    vec2 p1 = leminiscate(t * M_2_PI);
    vec2 p2 = leminiscate((dl + t) * M_2_PI);
    vec2 c = (p1 + p2) / 2.0;
    float d = 1e9;
    
    for(float i = 2.0; i < INTENSITY; i++){
        p1 = p2;
        p2 = leminiscate((i * dl + t) * M_2_PI);
        vec2 c_prev = c;
        c = (p1 + p2) / 2.;
        vec2 f = sdBezier(pos, c_prev, p1, c);
        d = min(d, f.x + FADING * (f.y + i) / INTENSITY);
    }
    return d;
}

float mapcircle(vec2 pos,float sp){
    float t = fract(-SPEED * TIME*sp);
    float dl = LENGTH / INTENSITY;
    vec2 p1 = circle(t * M_2_PI);
    vec2 p2 = circle((dl + t) * M_2_PI);
    vec2 c = (p1 + p2) / 2.0;
    float d = 1e9;
    
    for(float i = 2.0; i < INTENSITY; i++){
        p1 = p2;
        p2 = circle((i * dl + t) * M_2_PI);
        vec2 c_prev = c;
        c = (p1 + p2) / 2.;
        vec2 f = sdBezier(pos, c_prev, p1, c);
        d = min(d, f.x + FADING * (f.y + i) / INTENSITY);
    }
    return d;
}

void main() {

    vec2 uv = (2. * gl_FragCoord.xy - RENDERSIZE.xy) / RENDERSIZE.y;
	
    float dist1 = mapcircle(uv.yx*vec2(1.0,0.66),1.0);
	float dist2 = mapinfinite(uv.xy*vec2(0.66,1.0),2.0);
	float dist3 = mapcircle(uv.xy*vec2(1.0,0.88),4.0);
    
    vec3 col1 = vec3(1.0, 0.55, 0.25) * pow(RADIUS/dist1, GLOW);
	vec3 col2 = vec3(0.55, 1.00, 0.25) * pow(RADIUS/dist2, GLOW);
	vec3 col3 = vec3(0.25, 0.55, 1.00) * pow(RADIUS/dist3, GLOW);
	
	vec3 col=(col1+col2+col3)*(2.*GLOW);
    
    gl_FragColor = vec4(col, 1.0);
}
