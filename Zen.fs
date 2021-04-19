/*{
    "CATEGORIES": [
        "Zen",
        "Enso"
    ],
    "CREDIT": "Chaos.of.Zen",
    "DESCRIPTION": "Enso/Zen symbol",
    "IMPORTED": {
    },
    "ISFVSN": "2"
}
*/


#define PI 3.14159265359
#define HALF_PI 1.57079632675
#define TWO_PI 6.283185307

#define SECONDS 6.0
#define COUNT 32

#define initialAngle 3.14159265359

#define pulse false
#define rotation false
#define shineMovement true

vec2 randomNoise(vec2 uv)
{    
    uv = vec2( dot(uv,vec2(127.1,311.7)),
              dot(uv,vec2(29.5,183.3)) );
    if(shineMovement)
        return -1.0 + 2.0*fract(sin(uv)*43758.5453123);
    else
        return -1.0 + 2.0*fract(uv*43758.5453123);
}

float randomVignette(vec2 uv)
{
    if(shineMovement)
        return fract(sin(dot(uv.yx,vec2(14.7891,43.123)))*312991.41235);
    else
        return fract(dot(uv.yx,vec2(14.7891,43.123))*312991.41235);
}

float randomMovement(in float x)
{
    if(shineMovement)
        return fract(sin(x)*43758.5453123);
    else
        return fract(x*43758.5453123);
}


float vignetteNoise(vec2 uv) {
    vec2 i = floor(uv);
    vec2 f = fract(uv);

    vec2 u = f*f*(3.0-2.0*f);

    return mix( mix( dot( randomNoise(i + vec2(0.0,0.0)), f - vec2(0.0,0.0) ),
                     dot( randomNoise(i + vec2(1.0,0.0)), f - vec2(1.0,0.0) ), u.x),
                mix( dot( randomNoise(i + vec2(0.0,1.0)), f - vec2(0.0,1.0) ),
                     dot( randomNoise(i + vec2(1.0,1.0)), f - vec2(1.0,1.0) ), u.x), u.y);
}

mat2 rotate(float angle)
{
    return mat2( cos(angle),-sin(angle),sin(angle),cos(angle) );
}

vec2 ratio(vec2 uv)
{
    return  vec2(
            max(uv.x/uv.y,1.0),
            max(uv.y/uv.x,1.0)
            );
}

vec2 center(vec2 uv)
{
    float aspect = RENDERSIZE.x/RENDERSIZE.y;
    uv.x = uv.x * aspect - aspect * 0.5 + 0.5;
    return uv;
}

vec3 time()
{
    float period = mod(TIME,SECONDS);
    vec3 t = vec3(fract(TIME/SECONDS),period, 1.0-fract(period));
    return t;       // return fract(length),period,period phase
}

float scene(vec2 uv, vec3 t)
{
    uv = uv * 2.0 - 1.0;

    float seed = 29192.173;
    float center = length(uv-0.5) - 0.5;

    float n_scale = 0.12;

    float n_1 = vignetteNoise(uv + PI) * n_scale;
    float n_2 = vignetteNoise(uv+seed - PI) * n_scale;
    if(rotation) {
        float n_1 = vignetteNoise(uv + sin(PI*t.x)) * n_scale;
        float n_2 = vignetteNoise(uv+seed - sin(PI*t.x)) * n_scale;
    }

    float d = 1.0;
    for(int i = 1; i <= COUNT; i++)
    {
        float spread = 1.0 / float(i);
        float speed = ceil(3.0*spread);
        float r = randomMovement(float(i)*5.0 + seed);
        float r_scalar = r * 2.0 - 1.0;

        vec2 pos = uv - vec2(0.0);

        pos *= rotate(initialAngle);

        if(rotation) {
            pos += vec2(0.01) * rotate(TWO_PI * r_scalar + TWO_PI * t.x * speed * sign(r_scalar));
            pos *= rotate(TWO_PI * r_scalar + TWO_PI * t.x * speed * sign(r_scalar) + vignetteNoise(pos + float(i) + TIME) );
            pos += mix(n_1,n_2,0.5+0.5*sin(TWO_PI*t.x*speed));
            pos *= rotate(TWO_PI * r_scalar + TWO_PI * t.x * speed * sign(r_scalar) + vignetteNoise(pos + float(i) + TIME) );
        }

        float s = .45 + .126 * r;

        float a = atan(pos.y,pos.x)/PI;
        a = abs(a);
        a = smoothstep(0.0,1.0,a);

        float c = length(pos);
        c = abs(c-s);
        c -= 0.0004 + .0125 * a;

        d = min(d,c);
    }

    return d;
}

void main() {
    // timing
    vec3 t = time();
    // space
    vec2 uv = gl_FragCoord.xy/RENDERSIZE.xy;
    uv = center( uv );
    uv = uv * 2.0 - 1.0;

    if(pulse) 
        uv = uv * (1.0 + .03 * sin(TWO_PI*t.x));

    uv = uv * (1.0 + .03 * TWO_PI);
    uv = uv * 0.5 + 0.5;
    // scene
    float s = scene(uv, t);
    // aa
    float pixelSmoothing = 2.0;
    float aa = ratio(RENDERSIZE.xy).x/RENDERSIZE.x*pixelSmoothing;
    
    // background color
    vec3 color = vec3(0.08, 0., 0.);
    color = mix(color,vec3(1.0),1.0-smoothstep(-aa,aa,s));
    color = 1.0 - color;
        
    // vignette
    float size = length(uv-.5)-1.33;
    float vignette = (size) * 0.75 + randomVignette(uv) *.08;        
    color = mix(color,vec3(0.0, 0.0, 0.0),vignette+.5);
	
    float d = vignetteNoise(uv*7.0+TIME*0.25);
    
    gl_FragColor = vec4(color,mix(.25,.25+.75*d,1.0-smoothstep(-aa,aa,s)));
}

